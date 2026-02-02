using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Development;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;

namespace SQ2.GamePlay.Enemies;

internal sealed class Bat2EnemyComponent : BehaviorComponent, IRespawnable
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BatEnemy;
    private readonly IDebugRenderer _debugRenderer;
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private Vector2 _initialPosition;
    private const double AgroRange = 100;

    private State _state = State.Idle;
    private TimeSpan _stateTime;

    #region State variables

    private Vector2 _diveDirection;

    #endregion

    public Bat2EnemyComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
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
                break;
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
                OnStunned();
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
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawCircle(new Circle(_initialPosition, AgroRange), Color.Red);
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

        if (_stateTime > TimeSpan.FromSeconds(1))
        {
            var toPlayer = _playerTransform.Translation - _transform2DComponent.Translation;
            _diveDirection = toPlayer.Unit;
            _state = State.Dive;
        }
    }

    private void OnDive(Contact2D[] contacts)
    {
        _kinematicRigidBody2DComponent.LinearVelocity = _diveDirection * 150;

        if (contacts.Length > 0)
        {
            _state = State.Stunned;
        }
    }

    private void OnStunned()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;

        if (_stateTime > TimeSpan.FromSeconds(2))
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
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
    }

    private bool PlayerInAgroRange() => _playerTransform.Translation.Distance(_initialPosition) <= AgroRange;

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

    public Bat2EnemyComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override Bat2EnemyComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}