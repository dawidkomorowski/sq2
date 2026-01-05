using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;

namespace SQ2.GamePlay.Boss.Blue;

internal sealed class BlueBossComponent : BehaviorComponent, IRespawnable
{
    internal static readonly Vector2 SpriteOffset1 = new(0, -1);
    private static readonly Vector2 SpriteOffset2 = new(0, 1);
    internal static readonly Vector2 ColliderDimensions1 = new(34, 26);
    private static readonly Vector2 ColliderDimensions2 = new(24, 26);
    private static readonly Vector2 ColliderDimensions3 = new(24, 20);

    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BlueBoss;

    private readonly EntityFactory _entityFactory;
    private readonly IDebugRenderer _debugRenderer;
    private readonly RespawnService _respawnService;

    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteAnimationComponent _spriteAnimationComponent = null!;
    private Transform2DComponent _playerTransform = null!;

    private Transform2DComponent? _debugTransform;
    private TextRendererComponent? _debugText;

    private Vector2 _startPosition;
    private State _state = State.WaitingForPlayer;
    private TimeSpan _stateTime;
    private BossPhase _bossPhase = BossPhase.Phase1;

    private const double TriggerRadius = 100;

    #region State variables

    private int _idleCounter;
    private double _chaseSpeed;
    private int _rageChaseCounter;
    private int _shootCounter;
    private ShootPattern _shootPattern = ShootPattern.Single;
    private bool _hasJumped;
    private Vector2 _positionBeforeJump;

    #endregion

    public static class Animations
    {
        public const string Walk = "Walk";
        public const string Shoot = "Shoot";
    }

    public BlueBossComponent(Entity entity, EntityFactory entityFactory, IDebugRenderer debugRenderer, RespawnService respawnService) : base(entity)
    {
        _entityFactory = entityFactory;
        _debugRenderer = debugRenderer;
        _respawnService = respawnService;
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteAnimationComponent = Entity.Children[0].GetComponent<SpriteAnimationComponent>();

        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);
        Query.GetCameraMovementComponent(Scene).PointOfInterest = _transform2DComponent;

        _startPosition = _transform2DComponent.Translation;

        _kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        if (_enableDebugDraw)
        {
            var debugEntity = Scene.CreateEntity();
            _debugTransform = debugEntity.CreateComponent<Transform2DComponent>();
            _debugText = debugEntity.CreateComponent<TextRendererComponent>();
            _debugText.OrderInLayer = 10;
            _debugText.FontSize = FontSize.FromDips(10);
        }
    }

    public override void OnRemove()
    {
        if (_enableDebugDraw)
        {
            _debugTransform?.Entity.RemoveAfterFixedTimeStep();
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

                if (_bossPhase is not BossPhase.Phase6)
                {
                    playerComponent.KillPlayer();
                }
                else
                {
                    if (contact2D.CollisionNormal.Y < 0)
                    {
                        var playerKinematicComponent = playerComponent.Entity.GetComponent<KinematicRigidBody2DComponent>();
                        playerKinematicComponent.LinearVelocity = playerKinematicComponent.LinearVelocity.WithY(200);

                        Die();
                    }
                    else
                    {
                        playerComponent.KillPlayer();
                    }
                }

                break;
            }
        }

        var stateBefore = _state;

        if (_enableDebugDraw && _debugTransform is not null && _debugText is not null)
        {
            _debugTransform.Transform = new Transform2D(_transform2DComponent.Translation + new Vector2(-50, 40), 0, Vector2.One);
            _debugText.Text = $"Boss State: {_state}{Environment.NewLine}Boss Phase: {_bossPhase}";
        }

        switch (_state)
        {
            case State.WaitingForPlayer:
                OnWaitingForPlayer();
                break;
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
            case State.BeginJumpShoot:
                OnBeginJumpShoot();
                break;
            case State.JumpShoot:
                OnJumpShoot(contacts);
                break;
            case State.BeginChaseShoot:
                OnBeginChaseShoot();
                break;
            case State.ChaseShoot:
                OnChaseShoot(contacts);
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

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawCircle(new Circle(_transform2DComponent.Translation, TriggerRadius), Color.FromArgb(255, 255, 255, 0));
        }
    }

    public void Respawn()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;

        Entity.RemoveAfterFixedTimeStep();

        var (tx, ty) = Geometry.GetTileCoordinates(_startPosition);
        _entityFactory.CreateBlueBoss(Scene, tx, ty);
    }

    private void Die()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;

        Entity.RemoveAfterFixedTimeStep();

        _respawnService.AddOneTimeRespawnAction(() =>
        {
            var (tx, ty) = Geometry.GetTileCoordinates(_startPosition);
            _entityFactory.CreateBlueBoss(Scene, tx, ty);
        });

        var offset = new Vector2(SpriteOffset2.X * _transform2DComponent.Scale.X, SpriteOffset2.Y);
        _entityFactory.CreateBlueBossDeathAnimation(Scene, _transform2DComponent.Translation + offset, _transform2DComponent.Scale);
    }

    private void OnWaitingForPlayer()
    {
        if (_spriteAnimationComponent.CurrentAnimation?.Name != Animations.Shoot)
        {
            _spriteAnimationComponent.PlayAnimation(Animations.Shoot);
        }

        if (_transform2DComponent.Translation.Distance(_playerTransform.Translation) <= TriggerRadius)
        {
            _state = State.BeginIdle;
        }
    }

    private void OnBeginIdle()
    {
        _spriteAnimationComponent.PlayAnimation(Animations.Walk);
        _spriteAnimationComponent.PlaybackSpeed = 0.5;
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);
        _state = State.Idle;
    }

    private void OnIdle()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);

        if (_stateTime >= TimeSpan.FromSeconds(1))
        {
            if (_idleCounter < 5)
            {
                switch (_bossPhase)
                {
                    case BossPhase.Phase1:
                    case BossPhase.Phase2:
                    case BossPhase.Phase3:
                    case BossPhase.Phase4:
                        _idleCounter++;
                        _state = State.BeginShoot;
                        break;
                    case BossPhase.Phase5:
                        _idleCounter++;
                        _state = State.BeginChaseShoot;
                        break;
                    case BossPhase.Phase6:
                        _idleCounter = 0;
                        _state = State.BeginChase;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _idleCounter = 0;
                _state = State.BeginJumpShoot;
            }
        }
    }

    private void OnBeginChase()
    {
        if (_spriteAnimationComponent.CurrentAnimation?.Name != Animations.Walk)
        {
            _spriteAnimationComponent.PlayAnimation(Animations.Walk);
        }

        var chargeTime = TimeSpan.FromSeconds(1);

        _spriteAnimationComponent.PlaybackSpeed = 0.5 + 4 * _stateTime / chargeTime;

        if (_stateTime >= chargeTime)
        {
            _state = _bossPhase switch
            {
                BossPhase.Phase1 => State.Chase,
                BossPhase.Phase2 => Random.Shared.NextDouble() < 0.25 ? State.RageChase : State.Chase,
                BossPhase.Phase3 => Random.Shared.NextDouble() < 0.5 ? State.RageChase : State.Chase,
                BossPhase.Phase4 => Random.Shared.NextDouble() < 0.75 ? State.RageChase : State.Chase,
                BossPhase.Phase5 or BossPhase.Phase6 => State.RageChase,
                _ => throw new ArgumentOutOfRangeException()
            };

            _rageChaseCounter = 0;
            var directionToPlayer = _playerTransform.Translation - _transform2DComponent.Translation;
            const double chaseSpeed = 100;
            _chaseSpeed = Math.Sign(directionToPlayer.X) * chaseSpeed;

            // Ensure some speed in case the player is exactly above the boss.
            if (_chaseSpeed == 0)
            {
                _chaseSpeed = chaseSpeed;
            }
        }
    }

    private void OnChase(Contact2D[] contacts)
    {
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(_chaseSpeed);

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
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(_chaseSpeed);

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
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);

        if (_stateTime > TimeSpan.FromSeconds(1))
        {
            var random = Random.Shared.NextDouble();

            _shootPattern = _bossPhase switch
            {
                BossPhase.Phase1 => ShootPattern.Single,
                BossPhase.Phase2 => random < 0.5 ? ShootPattern.Single : ShootPattern.Triple,
                BossPhase.Phase3 => random < 0.25 ? ShootPattern.Single : random < 0.5 ? ShootPattern.Triple : ShootPattern.TripleTriple,
                BossPhase.Phase4 => random < 0.25 ? ShootPattern.Triple : ShootPattern.TripleTriple,
                BossPhase.Phase5 or BossPhase.Phase6 => ShootPattern.TripleTriple,
                _ => throw new ArgumentOutOfRangeException()
            };

            _shootCounter = 0;
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

    private void OnBeginJumpShoot()
    {
        var directionToPlayer = _playerTransform.Translation - _transform2DComponent.Translation;

        if (directionToPlayer.Length > 100 || _stateTime < TimeSpan.FromMilliseconds(250))
        {
            if (_spriteAnimationComponent.CurrentAnimation?.Name != Animations.Walk)
            {
                _spriteAnimationComponent.PlayAnimation(Animations.Walk);
            }

            _spriteAnimationComponent.PlaybackSpeed = 2;

            var walkSpeed = Math.Sign(directionToPlayer.X) * 50f;
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(walkSpeed);
        }
        else
        {
            _spriteAnimationComponent.PlayAnimation(Animations.Shoot);
            _spriteAnimationComponent.PlaybackSpeed = 1;
            _hasJumped = false;
            _shootCounter = 0;
            _state = State.JumpShoot;
        }
    }

    private void OnJumpShoot(Contact2D[] contacts)
    {
        if (_stateTime < TimeSpan.FromSeconds(1))
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);
            return;
        }

        var isOnGround = false;
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.Y > 0)
            {
                isOnGround = true;
                break;
            }
        }

        if (!_hasJumped)
        {
            if (isOnGround && _kinematicRigidBody2DComponent.LinearVelocity.Y <= 0)
            {
                if (_spriteAnimationComponent.CurrentAnimation?.Name != Animations.Walk)
                {
                    _spriteAnimationComponent.PlayAnimation(Animations.Walk);
                    _spriteAnimationComponent.PlaybackSpeed = 0;
                    _spriteAnimationComponent.Position = 0;
                }

                _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, 385);
                _kinematicRigidBody2DComponent.AngularVelocity = 5;

                _hasJumped = true;
                _positionBeforeJump = _transform2DComponent.Translation;
            }
        }
        else
        {
            var shootCount = _bossPhase is BossPhase.Phase5 ? 5 : 30;

            if (_transform2DComponent.Translation.Y - _positionBeforeJump.Y > 150 && (_shootCounter < shootCount || _transform2DComponent.Rotation != 0))
            {
                _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, 0);

                if (_spriteAnimationComponent.CurrentAnimation?.Name != Animations.Shoot)
                {
                    _spriteAnimationComponent.PlayAnimation(Animations.Shoot);
                    _spriteAnimationComponent.PlaybackSpeed = 0;
                    _spriteAnimationComponent.Position = 0;
                }


                if (_shootCounter < shootCount)
                {
                    if (_stateTime > TimeSpan.FromSeconds(0.6) * _shootCounter + TimeSpan.FromSeconds(2))
                    {
                        Shoot();
                        _shootCounter++;

                        RemoveSpikeForBossPhase();
                    }
                }
                else
                {
                    var remainingAngle = Math.Ceiling(_transform2DComponent.Rotation / (2 * Math.PI)) - _transform2DComponent.Rotation / (2 * Math.PI);

                    if (remainingAngle < 0.02)
                    {
                        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with { Rotation = 0 });
                        _kinematicRigidBody2DComponent.AngularVelocity = 0;
                    }
                }
            }

            if (isOnGround && _kinematicRigidBody2DComponent.LinearVelocity.Y <= 0)
            {
                _kinematicRigidBody2DComponent.AngularVelocity = 0;
                _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);
                _state = State.BeginIdle;

                AdvanceBossPhase();
                UpdateColliderBasedOnPhase();
            }
        }
    }

    private void OnBeginChaseShoot()
    {
        if (_spriteAnimationComponent.CurrentAnimation?.Name != Animations.Walk)
        {
            _spriteAnimationComponent.PlayAnimation(Animations.Walk);
        }

        var chargeTime = TimeSpan.FromSeconds(1);

        _spriteAnimationComponent.PlaybackSpeed = 0.5 + 4 * _stateTime / chargeTime;

        if (_stateTime >= chargeTime)
        {
            var directionToPlayer = _playerTransform.Translation - _transform2DComponent.Translation;
            const double chaseSpeed = 50;
            _chaseSpeed = Math.Sign(directionToPlayer.X) * chaseSpeed;

            // Ensure some speed in case the player is exactly above the boss.
            if (_chaseSpeed == 0)
            {
                _chaseSpeed = chaseSpeed;
            }

            _shootCounter = 0;
            _state = State.ChaseShoot;
        }
    }

    private void OnChaseShoot(Contact2D[] contacts)
    {
        if (_stateTime > TimeSpan.FromSeconds(0.6) * _shootCounter)
        {
            Shoot();
            _shootCounter++;
        }

        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(_chaseSpeed);

        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.X * _chaseSpeed < 0)
            {
                _state = State.BeginIdle;
                break;
            }
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

    private void RemoveSpikeForBossPhase()
    {
        switch (_bossPhase)
        {
            case BossPhase.Phase1:
                // No spikes removed
                break;
            case BossPhase.Phase2:
                RemoveSpike("DiagonalSpike1");
                RemoveSpike("DiagonalSpike2");
                break;
            case BossPhase.Phase3:
                RemoveSpike("LeftSpike2");
                RemoveSpike("RightSpike2");
                break;
            case BossPhase.Phase4:
                RemoveSpike("LeftSpike1");
                RemoveSpike("RightSpike1");
                break;
            case BossPhase.Phase5:
                RemoveSpike("TopLeftSpike");
                RemoveSpike("TopRightSpike");
                break;
            case BossPhase.Phase6:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RemoveSpike(string name)
    {
        var spriteEntity = Entity.Children[0];
        foreach (var spikeEntity in spriteEntity.Children)
        {
            if (spikeEntity.Name == name)
            {
                spikeEntity.RemoveAfterFixedTimeStep();
            }
        }
    }

    private void AdvanceBossPhase()
    {
        _bossPhase = _bossPhase switch
        {
            BossPhase.Phase1 => BossPhase.Phase2,
            BossPhase.Phase2 => BossPhase.Phase3,
            BossPhase.Phase3 => BossPhase.Phase4,
            BossPhase.Phase4 => BossPhase.Phase5,
            BossPhase.Phase5 or BossPhase.Phase6 => BossPhase.Phase6,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void UpdateColliderBasedOnPhase()
    {
        switch (_bossPhase)
        {
            case BossPhase.Phase1:
            case BossPhase.Phase2:
            case BossPhase.Phase3:
            case BossPhase.Phase4:
                break;
            case BossPhase.Phase5:
                _rectangleColliderComponent.Dimensions = ColliderDimensions2;
                break;
            case BossPhase.Phase6:
                _rectangleColliderComponent.Dimensions = ColliderDimensions3;
                var spriteEntity = Entity.Children[0];
                spriteEntity.GetComponent<Transform2DComponent>().Translation = SpriteOffset2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
        WaitingForPlayer,
        BeginIdle,
        Idle,
        BeginChase,
        Chase,
        RageChase,
        BeginShoot,
        Shoot,
        BeginJumpShoot,
        JumpShoot,
        BeginChaseShoot,
        ChaseShoot
    }

    private enum BossPhase
    {
        /// <summary>
        ///     Starts with all spikes.
        /// </summary>
        Phase1,

        /// <summary>
        ///     Starts with all spikes. Removes diagonal spikes after first jump shoot.
        /// </summary>
        Phase2,

        /// <summary>
        ///     Starts with 6/8 spikes. Removes lower left/right spikes after second jump shoot.
        /// </summary>
        Phase3,

        /// <summary>
        ///     Starts with 4/8 spikes. Removes upper left/right spikes after third jump shoot.
        /// </summary>
        Phase4,

        /// <summary>
        ///     Starts with 2/8 spikes. Removes top spikes after fourth jump shoot.
        /// </summary>
        Phase5,

        /// <summary>
        ///     Starts with no spikes.
        /// </summary>
        Phase6
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueBossComponentFactory : ComponentFactory<BlueBossComponent>
{
    private readonly EntityFactory _entityFactory;
    private readonly IDebugRenderer _debugRenderer;
    private readonly RespawnService _respawnService;

    public BlueBossComponentFactory(EntityFactory entityFactory, IDebugRenderer debugRenderer, RespawnService respawnService)
    {
        _entityFactory = entityFactory;
        _debugRenderer = debugRenderer;
        _respawnService = respawnService;
    }

    protected override BlueBossComponent CreateComponent(Entity entity) => new(entity, _entityFactory, _debugRenderer, _respawnService);
}