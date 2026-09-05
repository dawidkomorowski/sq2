using System;
using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Boss.Pumpkin;

internal sealed class PumpkinBossComponent : BehaviorComponent, IRespawnable
{
    internal static readonly Vector2 SpriteOffset = new(0, 2);

    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private Transform2DComponent _spriteTransformComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;

    private Transform2DComponent _playerTransform = null!;

    private Vector2 _initialPosition;
    private readonly List<Contact2D> _contacts = new();
    private double _animationTimer;
    private State _state = State.WaitingForPlayer;
    private TimeSpan _stateTime;
    private bool _isOnGround = false;
    private bool _playerIsToTheRight = false;

    public PumpkinBossComponent(Entity entity) : base(entity)
    {
    }

    public Sprite? Back { get; set; }
    public Sprite? Front { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _spriteTransformComponent = Entity.Children[0].GetComponent<Transform2DComponent>();
        _spriteRendererComponent = Entity.Children[0].GetComponent<SpriteRendererComponent>();

        _playerTransform = Query.GetPlayerTransformComponent(Scene);
        Query.GetCameraMovementComponent(Scene).PointOfInterest = _transform2DComponent;

        _initialPosition = _transform2DComponent.Translation;

        _spriteRendererComponent.Sprite = Back;
    }

    public override void OnFixedUpdate()
    {
        Movement.ApplyGravity(_kinematicRigidBody2DComponent);
        _isOnGround = false;

        var contacts = _rectangleColliderComponent.GetContactsAsSpan(_contacts);
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.Y > 0)
            {
                _isOnGround = true;
            }

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
            case State.WaitingForPlayer:
                OnWaitingForPlayer();
                break;
            case State.Idle:
                OnIdle();
                break;
            case State.LowJumping:
                OnLowJumping();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_state), _state, $"Unexpected PumpkinBoss state: {_state}");
        }

        _stateTime += TimeStep.FixedDeltaTime;

        if (stateBefore != _state)
        {
            _stateTime = TimeSpan.Zero;
        }
    }

    private void OnWaitingForPlayer()
    {
        const double triggerRadius = 100;
        if (_transform2DComponent.Translation.Distance(_playerTransform.Translation) < triggerRadius)
        {
            _state = State.Idle;
            _spriteRendererComponent.Sprite = Front;
        }
    }

    private void OnIdle()
    {
        if (_stateTime > TimeSpan.FromSeconds(2))
        {
            _state = State.LowJumping;
        }

        AnimateByTime();
    }

    private void OnLowJumping()
    {
        FindDirectionToPlayer();
        var baseHorizontalSpeed = 50;
        var horizontalSpeed = _playerIsToTheRight ? baseHorizontalSpeed : -baseHorizontalSpeed;

        if (_isOnGround)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(horizontalSpeed, 100);
        }

        AnimateByHeight(18);
    }

    private void FindDirectionToPlayer()
    {
        _playerIsToTheRight = _transform2DComponent.Translation.X < _playerTransform.Translation.X;
    }

    public void Respawn()
    {
        _state = State.WaitingForPlayer;
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with { Translation = _initialPosition });
        _spriteRendererComponent.Sprite = Back;
        ResetAnimation();
    }

    #region Animation

    private void AnimateByTime()
    {
        const double animationSpeed = 2;
        _animationTimer += TimeStep.FixedDeltaTimeSeconds * animationSpeed;
        SetAnimationState(_animationTimer, 0.1);
    }

    private void AnimateByHeight(double maxY)
    {
        var diffY = Math.Abs(_transform2DComponent.Translation.Y - _initialPosition.Y);
        diffY = Math.Min(diffY, maxY);

        var animationPosition = diffY / maxY * 0.5 - 0.25;
        SetAnimationState(animationPosition, 0.25);
    }

    private void SetAnimationState(double animationPosition, double stretchFactor)
    {
        var verticalScale = 1 + Math.Sin(animationPosition * Math.PI * 2) * stretchFactor;
        _spriteTransformComponent.Scale = new Vector2(1, verticalScale);
        var animationYOffset = (verticalScale - 1) * GlobalSettings.TileSize.Height;
        _spriteTransformComponent.Translation = SpriteOffset + new Vector2(0, animationYOffset);
    }

    private void ResetAnimation()
    {
        _animationTimer = 0;
        SetAnimationState(_animationTimer, 0);
    }

    #endregion

    private enum State
    {
        WaitingForPlayer,
        Idle,
        LowJumping
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PumpkinBossComponentFactory : ComponentFactory<PumpkinBossComponent>
{
    protected override PumpkinBossComponent CreateComponent(Entity entity) => new(entity);
}