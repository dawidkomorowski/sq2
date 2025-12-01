using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.GamePlay.Boss.Blue;

internal sealed class BlueBossComponent : BehaviorComponent
{
    internal static readonly Vector2 SpriteOffset = new(0, -1);
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BlueBoss;

    private readonly EntityFactory _entityFactory;

    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteAnimationComponent _spriteAnimationComponent = null!;
    private Transform2DComponent _playerTransform = null!;

    private Transform2DComponent? _debugTransform;
    private TextRendererComponent? _debugText;

    private State _state = State.BeginIdle;
    private TimeSpan _stateTime;

    #region State variables

    private double _chaseSpeed;
    private int _rageChaseCounter;
    private int _shootCounter;
    private ShootPattern _shootPattern = ShootPattern.Single;

    #endregion

    public static class Animations
    {
        public const string Walk = "Walk";
        public const string Shoot = "Shoot";
    }

    public BlueBossComponent(Entity entity, EntityFactory entityFactory) : base(entity)
    {
        _entityFactory = entityFactory;
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteAnimationComponent = Entity.Children[0].GetComponent<SpriteAnimationComponent>();

        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);

        if (_enableDebugDraw)
        {
            var debugEntity = Scene.CreateEntity();
            _debugTransform = debugEntity.CreateComponent<Transform2DComponent>();
            _debugText = debugEntity.CreateComponent<TextRendererComponent>();
            _debugText.OrderInLayer = 10;
            _debugText.FontSize = FontSize.FromDips(10);
        }
    }

    public override void OnFixedUpdate()
    {
        Movement.ApplyGravity(_kinematicRigidBody2DComponent);

        var contacts = _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();

        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();
                playerComponent.KillPlayer();

                break;
            }
        }

        var stateBefore = _state;

        if (_enableDebugDraw && _debugTransform is not null && _debugText is not null)
        {
            _debugTransform.Transform = new Transform2D(_transform2DComponent.Translation + new Vector2(-50, 40), 0, Vector2.One);
            _debugText.Text = "Boss State: " + _state;
        }

        switch (_state)
        {
            case State.BeginIdle:
                OnBeginIdle();
                break;
            case State.Idle:
                OnIdle();
                break;
            case State.BeginChase:
                OnBeginChase();
                break;
            case State.Chase:
                OnChase(contacts);
                break;
            case State.RageChase:
                OnRageChase(contacts);
                break;
            case State.BeginShoot:
                OnBeginShoot();
                break;
            case State.Shoot:
                OnShoot();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_state), _state, $"Unexpected BlueBoss state: {_state}");
        }

        _stateTime += GameTime.FixedDeltaTime;

        if (stateBefore != _state)
        {
            _stateTime = TimeSpan.Zero;
        }


        Movement.UpdateHorizontalSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);
    }

    private void OnBeginIdle()
    {
        _spriteAnimationComponent.PlayAnimation(Animations.Walk);
        _spriteAnimationComponent.PlaybackSpeed = 0.5;
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, _kinematicRigidBody2DComponent.LinearVelocity.Y);
        _state = State.Idle;
    }

    private void OnIdle()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, _kinematicRigidBody2DComponent.LinearVelocity.Y);

        if (_stateTime >= TimeSpan.FromSeconds(2))
        {
            _state = State.BeginShoot;
        }
    }

    private void OnBeginChase()
    {
        _spriteAnimationComponent.PlayAnimation(Animations.Walk);
        var chargeTime = TimeSpan.FromSeconds(1);

        _spriteAnimationComponent.PlaybackSpeed = 0.5 + 4 * _stateTime / chargeTime;

        if (_stateTime >= chargeTime)
        {
            var isRageChase = Random.Shared.Next(0, 2) == 0;
            _state = isRageChase ? State.RageChase : State.Chase;

            _rageChaseCounter = 0;
            var directionToPlayer = _playerTransform.Translation - _transform2DComponent.Translation;
            _chaseSpeed = Math.Sign(directionToPlayer.X) * 100f;

            // Ensure some speed in case the player is exactly above the boss.
            if (_chaseSpeed == 0)
            {
                _chaseSpeed = 100f;
            }
        }
    }

    private void OnChase(Contact2D[] contacts)
    {
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(_chaseSpeed, _kinematicRigidBody2DComponent.LinearVelocity.Y);

        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.X * _chaseSpeed < 0)
            {
                _state = State.BeginIdle;
                break;
            }
        }
    }

    private void OnRageChase(Contact2D[] contacts)
    {
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(_chaseSpeed, _kinematicRigidBody2DComponent.LinearVelocity.Y);

        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.X * _chaseSpeed < 0)
            {
                _chaseSpeed *= -1.5f;
                _rageChaseCounter++;

                if (_rageChaseCounter >= 3)
                {
                    _state = State.BeginIdle;
                }

                break;
            }
        }
    }

    private void OnBeginShoot()
    {
        _spriteAnimationComponent.PlayAnimation(Animations.Shoot);
        _spriteAnimationComponent.PlaybackSpeed = 1;
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, _kinematicRigidBody2DComponent.LinearVelocity.Y);

        if (_stateTime > TimeSpan.FromSeconds(1))
        {
            _shootCounter = 0;
            var patterns = Enum.GetValues<ShootPattern>();
            _shootPattern = patterns[Random.Shared.Next(0, patterns.Length)];

            _state = State.Shoot;
        }
    }

    private void OnShoot()
    {
        switch (_shootPattern)
        {
            case ShootPattern.Single:
                ShootPattern_Single();
                break;
            case ShootPattern.Triple:
                ShootPattern_Triple();
                break;
            case ShootPattern.TripleTriple:
                ShootPattern_TripleTriple();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ShootPattern_Single()
    {
        if (_stateTime > TimeSpan.FromSeconds(0.2) * _shootCounter)
        {
            Shoot();
            _shootCounter++;
        }

        if (_shootCounter >= 1)
        {
            _state = State.BeginChase;
        }
    }

    private void ShootPattern_Triple()
    {
        if (_stateTime > TimeSpan.FromSeconds(0.2) * _shootCounter)
        {
            Shoot();
            _shootCounter++;
        }

        if (_shootCounter >= 3)
        {
            _state = State.BeginChase;
        }
    }

    private void ShootPattern_TripleTriple()
    {
        ShootPattern_Triple();

        if (_shootCounter == 3)
        {
            _shootCounter = 6;
        }

        if (_shootCounter == 9)
        {
            _shootCounter = 12;
        }

        _state = State.Shoot;

        if (_shootCounter >= 15)
        {
            _state = State.BeginChase;
        }
    }

    private void Shoot()
    {
        var spriteEntity = Entity.Children[0];
        foreach (var spikeEntity in spriteEntity.Children)
        {
            var spikeTransform = spikeEntity.GetComponent<Transform2DComponent>();
            var worldTransform = spikeTransform.ComputeWorldTransformMatrix().ToTransform();
            _entityFactory.CreateBlueBossProjectile(
                Scene,
                worldTransform.Translation,
                worldTransform.Rotation,
                (Matrix3x3.CreateRotation(worldTransform.Rotation) * Vector2.UnitY.Homogeneous).ToVector2()
            );
        }
    }

    private enum ShootPattern
    {
        Single,
        Triple,
        TripleTriple
    }

    private enum State
    {
        BeginIdle,
        Idle,
        BeginChase,
        Chase,
        RageChase,
        BeginShoot,
        Shoot
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueBossComponentFactory : ComponentFactory<BlueBossComponent>
{
    private readonly EntityFactory _entityFactory;

    public BlueBossComponentFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    protected override BlueBossComponent CreateComponent(Entity entity) => new(entity, _entityFactory);
}