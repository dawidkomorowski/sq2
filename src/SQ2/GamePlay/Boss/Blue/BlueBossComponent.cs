using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;
using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core;

namespace SQ2.GamePlay.Boss.Blue;

internal sealed class BlueBossComponent : BehaviorComponent
{
    internal static readonly Vector2 SpriteOffset = new(0, -1);

    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteAnimationComponent _spriteAnimationComponent = null!;
    private Transform2DComponent _playerTransform = null!;

    private State _state = State.BeginIdle;

    private TimeSpan _stateTime;

    private double _chaseSpeed;

    public BlueBossComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteAnimationComponent = Entity.Children[0].GetComponent<SpriteAnimationComponent>();

        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);
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
        _spriteAnimationComponent.PlaybackSpeed = 0.5;
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, _kinematicRigidBody2DComponent.LinearVelocity.Y);
        _state = State.Idle;
    }

    private void OnIdle()
    {
        _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, _kinematicRigidBody2DComponent.LinearVelocity.Y);

        if (_stateTime >= TimeSpan.FromSeconds(2))
        {
            _state = State.BeginChase;
            _stateTime = TimeSpan.Zero;
        }
    }

    private void OnBeginChase()
    {
        var chargeTime = TimeSpan.FromSeconds(1);

        _spriteAnimationComponent.PlaybackSpeed = 0.5 + 4 * _stateTime / chargeTime;

        if (_stateTime >= chargeTime)
        {
            _state = State.Chase;
            _stateTime = TimeSpan.Zero;

            var directionToPlayer = (_playerTransform.Translation - _transform2DComponent.Translation).Unit;
            _chaseSpeed = directionToPlayer.X * 100f;
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

    private enum State
    {
        BeginIdle,
        Idle,
        BeginChase,
        Chase
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueBossComponentFactory : ComponentFactory<BlueBossComponent>
{
    protected override BlueBossComponent CreateComponent(Entity entity) => new(entity);
}