using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;
using System.Collections.Generic;

namespace SQ2.GamePlay.Boss.Pumpkin;

internal sealed class PumpkinBossComponent : BehaviorComponent, IRespawnable
{
    internal static readonly Vector2 SpriteOffset = new(0, 2);

    private readonly EntityFactory _entityFactory;
    private readonly RespawnService _respawnService;

    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private Transform2DComponent _spriteTransformComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;

    private Transform2DComponent _playerTransform = null!;
    private CameraMovementComponent _cameraMovementComponent = null!;

    private Vector2 _initialPosition;
    private readonly List<Contact2D> _contacts = new();
    private double _animationTimer;
    private State _state = State.WaitingForPlayer;
    private TimeSpan _stateTime;
    private bool _isOnGround;
    private bool _jumpRight;
    private int _jumpCount;
    private int _jumpCountAfterPassingPlayer;
    private double _jumpHorizontalSpeed;
    private bool _damagedThisFrame;
    private int _health = 3;

    public PumpkinBossComponent(Entity entity, EntityFactory entityFactory, RespawnService respawnService) : base(entity)
    {
        _entityFactory = entityFactory;
        _respawnService = respawnService;
    }

    public Sprite? Back { get; set; }
    public Sprite? Front { get; set; }
    public Sprite? Damage { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _spriteTransformComponent = Entity.Children[0].GetComponent<Transform2DComponent>();
        _spriteRendererComponent = Entity.Children[0].GetComponent<SpriteRendererComponent>();

        _playerTransform = Query.GetPlayerTransformComponent(Scene);
        _cameraMovementComponent = Query.GetCameraMovementComponent(Scene);
        _cameraMovementComponent.PointOfInterest = _transform2DComponent;

        _initialPosition = _transform2DComponent.Translation;

        Respawn();
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

            if (contact2D.CollisionNormal.X > 0)
            {
                _kinematicRigidBody2DComponent.LinearVelocity =
                    _kinematicRigidBody2DComponent.LinearVelocity.WithX(-_kinematicRigidBody2DComponent.LinearVelocity.X);
                _jumpRight = true;
            }

            if (contact2D.CollisionNormal.X < 0)
            {
                _kinematicRigidBody2DComponent.LinearVelocity =
                    _kinematicRigidBody2DComponent.LinearVelocity.WithX(-_kinematicRigidBody2DComponent.LinearVelocity.X);
                _jumpRight = false;
            }

            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();

                if (_state is State.Vulnerable)
                {
                    if (contact2D.CollisionNormal.Y < 0)
                    {
                        var playerKinematicComponent = playerComponent.Entity.GetComponent<KinematicRigidBody2DComponent>();
                        playerKinematicComponent.LinearVelocity = playerKinematicComponent.LinearVelocity.WithY(200);

                        TakeDamage();
                    }
                    else
                    {
                        playerComponent.KillPlayer();
                    }
                }
                else
                {
                    playerComponent.KillPlayer();
                }
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
                OnLowJumping(contacts);
                break;
            case State.PrepareHighJump:
                OnPrepareHighJump();
                break;
            case State.HighJumping:
                OnHighJumping(contacts);
                break;
            case State.Stomp:
                OnStomp();
                break;
            case State.Vulnerable:
                OnVulnerable();
                break;
            case State.Damaged:
                OnDamaged();
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
        _spriteRendererComponent.Sprite = Front;

        if (_stateTime > TimeSpan.FromSeconds(2))
        {
            InitJumpState();
            _state = State.LowJumping;
        }

        AnimateByTime();
    }

    private void OnLowJumping(ReadOnlySpan<Contact2D> contacts)
    {
        // Bounce from walls.
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.X > 0)
            {
                _jumpRight = true;
                _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(-_jumpHorizontalSpeed);
            }

            if (contact2D.CollisionNormal.X < 0)
            {
                _jumpRight = false;
                _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(-_jumpHorizontalSpeed);
            }
        }

        // Jump when on ground.
        if (_isOnGround)
        {
            if (_jumpCount >= 24)
            {
                _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
                InitJumpState();
                _state = State.PrepareHighJump;
                return;
            }

            var playerIsToTheRight = PlayerIsToTheRight();

            if (_jumpCountAfterPassingPlayer >= 2)
            {
                _jumpCountAfterPassingPlayer = 0;
                _jumpRight = playerIsToTheRight;
            }

            var baseHorizontalSpeed = _jumpCount < 12 ? 50 : 100;
            _jumpHorizontalSpeed = _jumpRight ? baseHorizontalSpeed : -baseHorizontalSpeed;

            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(_jumpHorizontalSpeed, 100);
            _jumpCount++;

            if (_jumpRight != playerIsToTheRight)
            {
                _jumpCountAfterPassingPlayer++;
            }
        }

        AnimateByHeight(18);
    }

    private void OnPrepareHighJump()
    {
        if (_stateTime > TimeSpan.FromSeconds(1))
        {
            InitJumpState();
            _state = State.HighJumping;
        }

        AnimateByHeight(18 * 3);
    }

    private void OnHighJumping(ReadOnlySpan<Contact2D> contacts)
    {
        // Bounce from walls.
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.X > 0)
            {
                _jumpRight = true;
                _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(-_jumpHorizontalSpeed);
            }

            if (contact2D.CollisionNormal.X < 0)
            {
                _jumpRight = false;
                _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(-_jumpHorizontalSpeed);
            }
        }

        // Jump when on ground.
        if (_isOnGround)
        {
            if (_jumpCount >= 12)
            {
                _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
                InitJumpState();
                _state = State.Stomp;
                return;
            }

            var horizontalDiff = _playerTransform.Translation.X - _transform2DComponent.Translation.X;
            var horizontalDistance = Math.Abs(horizontalDiff);

            // High jump.
            _jumpHorizontalSpeed = horizontalDiff + ((_jumpCount % 3) - 1) * 40;
            var verticalSpeed = 200 + Math.Abs(horizontalDistance);
            verticalSpeed = Math.Min(verticalSpeed, 300);

            // Low jump if player too close.
            if (horizontalDistance < 50)
            {
                _jumpHorizontalSpeed = Math.Sign(horizontalDiff) * 200;
                verticalSpeed = 100;
            }

            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(_jumpHorizontalSpeed, verticalSpeed);
            _jumpCount++;
        }

        AnimateByHeight(18 * 3);
    }

    private void OnStomp()
    {
        if (_jumpCount == 0)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, 300);
            _jumpCount++;
            return;
        }

        if (_kinematicRigidBody2DComponent.LinearVelocity.Y < 0)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, -300);
        }

        if (_isOnGround)
        {
            _cameraMovementComponent.ShakeCamera();
            _entityFactory.CreateSmokePuffAnimation(Scene, _transform2DComponent.Translation);
            _state = State.Vulnerable;
        }

        AnimateByHeight(18 * 3);
    }

    private void OnVulnerable()
    {
        if (_damagedThisFrame)
        {
            _damagedThisFrame = false;
            _state = State.Damaged;
            return;
        }

        if (_stateTime > TimeSpan.FromSeconds(4))
        {
            _spriteRendererComponent.Sprite = Front;
            _state = State.Idle;
            return;
        }

        _spriteRendererComponent.Sprite = Back;
        ResetAnimation();
    }

    private void OnDamaged()
    {
        _spriteRendererComponent.Sprite = Damage;
        ResetAnimation();

        if (_stateTime > TimeSpan.FromMilliseconds(50))
        {
            _spriteRendererComponent.Sprite = Front;
            _state = State.Idle;
        }
    }

    private void InitJumpState()
    {
        _jumpCount = 0;
        _jumpCountAfterPassingPlayer = 0;
        _jumpRight = PlayerIsToTheRight();
        _jumpHorizontalSpeed = 0;
    }

    private bool PlayerIsToTheRight() => _transform2DComponent.Translation.X < _playerTransform.Translation.X;

    private void TakeDamage()
    {
        _damagedThisFrame = true;
        _health--;

        if (_health <= 0)
        {
            Entity.RemoveAfterFixedTimeStep();
            _respawnService.AddOneTimeRespawnAction(() => { _entityFactory.CreatePumpkinBoss(Scene, _initialPosition); });
            _entityFactory.CreatePumpkinBossDeathAnimation(Scene, _transform2DComponent.Translation + SpriteOffset);
        }
    }

    public void Respawn()
    {
        _state = State.WaitingForPlayer;
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with { Translation = _initialPosition });
        _spriteRendererComponent.Sprite = Back;
        ResetAnimation();
        InitJumpState();
        _damagedThisFrame = false;
        _health = 3;
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
        LowJumping,
        PrepareHighJump,
        HighJumping,
        Stomp,
        Vulnerable,
        Damaged
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PumpkinBossComponentFactory : ComponentFactory<PumpkinBossComponent>
{
    private readonly EntityFactory _entityFactory;
    private readonly RespawnService _respawnService;

    public PumpkinBossComponentFactory(EntityFactory entityFactory, RespawnService respawnService)
    {
        _entityFactory = entityFactory;
        _respawnService = respawnService;
    }

    protected override PumpkinBossComponent CreateComponent(Entity entity) => new(entity, _entityFactory, _respawnService);
}