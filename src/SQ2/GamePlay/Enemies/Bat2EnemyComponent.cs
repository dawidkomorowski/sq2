using System;
using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Enemies;

internal sealed class Bat2EnemyComponent : BehaviorComponent, IRespawnable
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BatEnemy;
    private readonly IDebugRenderer _debugRenderer;
    private readonly RespawnService _respawnService;
    private readonly EntityFactory _entityFactory;
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private SpriteAnimationComponent _spriteAnimationComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private Vector2 _initialPosition;
    private const double IdleAgroRange = 100;
    private const double ChaseAgroRange = 200;

    private State _state = State.Idle;
    private TimeSpan _stateTime;

    #region State variables

    private Vector2 _diveDirection;
    private Vector2 _diveStartPosition;

    #endregion

    public Bat2EnemyComponent(Entity entity, IDebugRenderer debugRenderer, RespawnService respawnService, EntityFactory entityFactory) : base(entity)
    {
        _debugRenderer = debugRenderer;
        _respawnService = respawnService;
        _entityFactory = entityFactory;
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _spriteAnimationComponent = Entity.GetComponent<SpriteAnimationComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);

        _initialPosition = _transform2DComponent.Translation;
        _state = State.Idle;
    }

    public override void OnFixedUpdate()
    {
        var contacts = _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();

        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();
                playerComponent.KillPlayer();
                return;
            }

            if (contact2D.OtherCollider.Entity.Root.HasComponent<SpikesComponent>())
            {
                Die();
                return;
            }
        }

        var stateBefore = _state;

        switch (_state)
        {
            case State.Idle:
                OnIdle();
                break;
            case State.Aim:
                OnAim();
                break;
            case State.Dive:
                OnDive(contacts);
                break;
            case State.Stunned:
                OnStunned(contacts);
                break;
            case State.Return:
                OnReturn();
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
            _debugRenderer.DrawCircle(new Circle(_initialPosition, IdleAgroRange), Color.Blue);
            _debugRenderer.DrawCircle(new Circle(_initialPosition, ChaseAgroRange), Color.Red);
        }
    }

    public void Respawn()
    {
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _initialPosition
        });
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        _state = State.Idle;
        _spriteAnimationComponent.Resume();
    }

    private void Die()
    {
        Entity.RemoveAfterFixedTimeStep();
        _respawnService.AddOneTimeRespawnAction(() => { _entityFactory.CreateBat2Enemy(Scene, _initialPosition.X, _initialPosition.Y); });
    }

    private void OnIdle()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;

        if (PlayerInAgroRange())
        {
            _state = State.Aim;
        }
    }

    private void OnAim()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;

        if (_playerTransform.Translation.Y + 50 > _transform2DComponent.Translation.Y)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = Vector2.UnitY * 75;
        }

        if (_stateTime > TimeSpan.FromSeconds(1))
        {
            var toPlayer = _playerTransform.Translation - _transform2DComponent.Translation;
            _diveDirection = toPlayer.Unit;

            // Simple logic to unstuck the bat if it is very close to the previous dive position.
            if (_transform2DComponent.Translation.Distance(_diveStartPosition) < 20 && Random.Shared.NextDouble() > 0.5)
            {
                _diveDirection = Random.Shared.NextDouble() > 0.5 ? _diveDirection.Normal : -_diveDirection.Normal;
                _diveDirection = (Matrix3x3.CreateRotation(Angle.Deg2Rad(Random.Shared.Next(-30, 30))) * _diveDirection.Homogeneous).ToVector2();
            }

            _diveStartPosition = _transform2DComponent.Translation;
            _state = State.Dive;
        }
    }

    private void OnDive(Contact2D[] contacts)
    {
        _spriteAnimationComponent.Position = 0.25;
        _spriteAnimationComponent.Pause();

        _kinematicRigidBody2DComponent.LinearVelocity = _diveDirection * 150;

        if (_stateTime < TimeSpan.FromSeconds(0.1))
        {
            // Allow some time to leave the aiming position.
            return;
        }

        if (_transform2DComponent.Translation.Distance(_diveStartPosition) > 200)
        {
            _spriteAnimationComponent.Resume();
            _state = State.Stunned;
        }

        foreach (var contact in contacts)
        {
            if (!contact.OtherCollider.Entity.HasComponent<Bat2EnemyComponent>())
            {
                _spriteAnimationComponent.Resume();
                _state = State.Stunned;
            }
        }
    }

    private void OnStunned(Contact2D[] contacts)
    {
        if (contacts.Length > 0)
        {
            var contact = contacts[0];
            _kinematicRigidBody2DComponent.LinearVelocity = contact.CollisionNormal * 10;
        }
        else
        {
            _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        }

        if (_stateTime > TimeSpan.FromSeconds(1))
        {
            if (PlayerInAgroRange())
            {
                _state = State.Aim;
            }
            else
            {
                _state = State.Return;
            }
        }
    }

    private void OnReturn()
    {
        if (_transform2DComponent.Translation.Distance(_initialPosition) < 10)
        {
            _diveStartPosition = Vector2.Zero;
            _state = State.Idle;
        }

        if (PlayerInAgroRange())
        {
            _state = State.Aim;
        }

        var toInitialPosition = _initialPosition - _transform2DComponent.Translation;
        _kinematicRigidBody2DComponent.LinearVelocity = toInitialPosition.Unit * 50;
    }

    private bool PlayerInAgroRange()
    {
        return _state is State.Idle
            ? _playerTransform.Translation.Distance(_initialPosition) <= IdleAgroRange
            : _playerTransform.Translation.Distance(_initialPosition) <= ChaseAgroRange;
    }

    private enum State
    {
        Idle,
        Aim,
        Dive,
        Stunned,
        Return
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Bat2EnemyComponentFactory : ComponentFactory<Bat2EnemyComponent>
{
    private readonly IDebugRenderer _debugRenderer;
    private readonly RespawnService _respawnService;
    private readonly EntityFactory _entityFactory;

    public Bat2EnemyComponentFactory(IDebugRenderer debugRenderer, RespawnService respawnService, EntityFactory entityFactory)
    {
        _debugRenderer = debugRenderer;
        _respawnService = respawnService;
        _entityFactory = entityFactory;
    }

    protected override Bat2EnemyComponent CreateComponent(Entity entity) => new(entity, _debugRenderer, _respawnService, _entityFactory);
}