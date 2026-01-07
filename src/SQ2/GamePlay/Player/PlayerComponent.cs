using System;
using System.Linq;
using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Development;
using SQ2.GamePlay.Boss.Blue;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;

namespace SQ2.GamePlay.Player;

internal sealed class PlayerComponent : BehaviorComponent, IRespawnable
{
    // Input Actions
    private const string MoveLeftAction = "MoveLeft";
    private const string MoveRightAction = "MoveRight";
    private const string MoveUpAction = "MoveUp";
    private const string MoveDownAction = "MoveDown";
    private const string JumpAction = "Jump";

    // Debugging
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.Ladders;
    private readonly IDebugRenderer _debugRenderer;

    // Movement and Physics
    private KinematicRigidBody2DComponent _kinematicBodyComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;
    private InputComponent _inputComponent = null!;
    private int _jumpPressFrames;
    private bool _lastJumpState;

    // Animation
    private SpriteAnimationComponent _spriteAnimationComponent = null!;
    private Transform2DComponent _spriteTransformComponent = null!;
    private Transform2D _spriteDefaultTransform;
    private double _ladderClimbingAnimationTimer;

    // Ladders
    private readonly Vector2 _ladderClimbRange = new(9, 9);
    private AxisAlignedRectangle[] _ladderHitBoxes = Array.Empty<AxisAlignedRectangle>();
    private int _reclimbAfterJumpCooldown;

    // Respawn and Checkpoints
    private readonly RespawnService _respawnService;
    private PlayerSpawnPointComponent _playerSpawnPointComponent = null!;
    internal CheckPointComponent? ActiveCheckPoint { get; set; }

    public PlayerComponent(Entity entity, IDebugRenderer debugRenderer, RespawnService respawnService) : base(entity)
    {
        _debugRenderer = debugRenderer;
        _respawnService = respawnService;
    }

    public override void OnStart()
    {
        _kinematicBodyComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _inputComponent = Entity.GetComponent<InputComponent>();
        _spriteAnimationComponent = Entity.Children[0].GetComponent<SpriteAnimationComponent>();
        _spriteTransformComponent = Entity.Children[0].GetComponent<Transform2DComponent>();
        _spriteDefaultTransform = _spriteTransformComponent.Transform;

        _inputComponent.InputMapping = new InputMapping
        {
            ActionMappings =
            {
                new ActionMapping
                {
                    ActionName = MoveLeftAction,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Left)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = MoveRightAction,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Right)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = MoveUpAction,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Up)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = MoveDownAction,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Down)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = JumpAction,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Space)
                        }
                    }
                }
            }
        };

        _playerSpawnPointComponent = Scene.RootEntities.Single(e => e.HasComponent<PlayerSpawnPointComponent>()).GetComponent<PlayerSpawnPointComponent>();

        _ladderHitBoxes = Scene.RootEntities
            .Where(e => e.HasComponent<LadderComponent>())
            .Select(e => e.GetComponent<LadderComponent>().HitBox)
            .ToArray();
    }

    public override void OnFixedUpdate()
    {
        var contacts = GetPlayerContacts();

        if (CheckForCollisionsWithOtherEntities(contacts))
        {
            return;
        }

        var isOnGround = IsOnGround(contacts);
        var isOnLadder = IsOnLadder();

        if (_reclimbAfterJumpCooldown > 0)
        {
            _reclimbAfterJumpCooldown--;
        }

        if (isOnLadder && !isOnGround)
        {
            var linearVelocity = _kinematicBodyComponent.LinearVelocity;

            linearVelocity = ClimbLadderLogic(linearVelocity);

            _kinematicBodyComponent.LinearVelocity = linearVelocity;
        }
        else
        {
            Movement.ApplyGravity(_kinematicBodyComponent);

            var linearVelocity = _kinematicBodyComponent.LinearVelocity;

            linearVelocity = HorizontalMovementLogic(linearVelocity);
            linearVelocity = JumpLogic(linearVelocity, isOnGround);

            if (isOnLadder && _inputComponent.GetActionState(MoveUpAction))
            {
                linearVelocity = ClimbLadderLogic(linearVelocity);
            }

            _kinematicBodyComponent.LinearVelocity = linearVelocity;
        }

        Movement.UpdateHorizontalSpriteFacing(_transform2DComponent, _kinematicBodyComponent);

        UpdateAnimationState(_kinematicBodyComponent.LinearVelocity, isOnGround, isOnLadder);
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            foreach (var ladderHitBox in _ladderHitBoxes)
            {
                _debugRenderer.DrawRectangle(ladderHitBox, Color.Red, Matrix3x3.Identity);
            }

            _debugRenderer.DrawRectangle(new AxisAlignedRectangle(_transform2DComponent.Translation, _ladderClimbRange), Color.Red, Matrix3x3.Identity);
        }
    }

    private Contact2D[] GetPlayerContacts()
    {
        return _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();
    }

    private bool CheckForCollisionsWithOtherEntities(Contact2D[] contacts)
    {
        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.Root.HasComponent<SpikesComponent>())
            {
                KillPlayer();
                return true;
            }

            if (contact2D.OtherCollider.Entity.Root.HasComponent<WaterDeepComponent>())
            {
                KillPlayer();
                return true;
            }
        }

        return false;
    }

    private static bool IsOnGround(Contact2D[] contacts)
    {
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.Y > 0)
            {
                // Ignore jump pads when checking for ground because they automatically launch the player.
                if (contact2D.OtherCollider.Entity.Root.HasComponent<JumpPadComponent>())
                {
                    continue;
                }

                // Ignore blue enemies when checking for ground because they automatically bounce the player.
                if (contact2D.OtherCollider.Entity.Root.HasComponent<BlueEnemyComponent>())
                {
                    continue;
                }

                // Ignore blue boss when checking for ground because it automatically bounce the player.
                if (contact2D.OtherCollider.Entity.Root.HasComponent<BlueBossComponent>())
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    private Vector2 HorizontalMovementLogic(Vector2 linearVelocity)
    {
        const double maxSpeed = 100;
        const double acceleration = 250;
        const double deceleration = 450;

        var moveLeftState = _inputComponent.GetActionState(MoveLeftAction);
        var moveRightState = _inputComponent.GetActionState(MoveRightAction);

        if (moveLeftState && !moveRightState)
        {
            var effectiveAcceleration = linearVelocity.X > 0 ? deceleration : acceleration;
            var verticalVelocity = linearVelocity.X - effectiveAcceleration * GameTime.FixedDeltaTimeSeconds;
            linearVelocity = linearVelocity.WithX(verticalVelocity);
        }

        if (moveRightState && !moveLeftState)
        {
            var effectiveAcceleration = linearVelocity.X < 0 ? deceleration : acceleration;
            var verticalVelocity = linearVelocity.X + effectiveAcceleration * GameTime.FixedDeltaTimeSeconds;
            linearVelocity = linearVelocity.WithX(verticalVelocity);
        }

        if (moveLeftState == moveRightState)
        {
            if (Math.Abs(linearVelocity.X) < deceleration * GameTime.FixedDeltaTimeSeconds)
            {
                linearVelocity = linearVelocity.WithX(0);
            }
            else
            {
                var verticalVelocity = linearVelocity.X - Math.Sign(linearVelocity.X) * deceleration * GameTime.FixedDeltaTimeSeconds;
                linearVelocity = linearVelocity.WithX(verticalVelocity);
            }
        }

        if (Math.Abs(linearVelocity.X) > maxSpeed)
        {
            linearVelocity = linearVelocity.WithX(Math.Sign(linearVelocity.X) * maxSpeed);
        }

        return linearVelocity;
    }

    private Vector2 JumpLogic(Vector2 linearVelocity, bool isOnGround)
    {
        const int maxJumpPressFrames = 30;
        const double baseJumpSpeed = 150;
        const double longJumpAcceleration = 500;

        var jumpState = _inputComponent.GetActionState(JumpAction);

        if (jumpState && _jumpPressFrames is > 0 and < maxJumpPressFrames)
        {
            _jumpPressFrames++;
            var jumpAcceleration = longJumpAcceleration - longJumpAcceleration * _jumpPressFrames / maxJumpPressFrames;
            linearVelocity = linearVelocity.WithY(linearVelocity.Y + jumpAcceleration * GameTime.FixedDeltaTimeSeconds);
        }

        if (isOnGround && _jumpPressFrames == 0 && jumpState && !_lastJumpState)
        {
            _jumpPressFrames++;
            linearVelocity = linearVelocity.WithY(baseJumpSpeed);
        }

        if (!jumpState)
        {
            _jumpPressFrames = 0;
        }

        _lastJumpState = jumpState;

        return linearVelocity;
    }

    private bool IsOnLadder()
    {
        var playerHitBox = new AxisAlignedRectangle(_transform2DComponent.Translation, _ladderClimbRange);
        foreach (var ladderHitBox in _ladderHitBoxes)
        {
            if (ladderHitBox.Overlaps(playerHitBox)) return true;
        }

        return false;
    }

    private Vector2 ClimbLadderLogic(Vector2 linearVelocity)
    {
        if (_reclimbAfterJumpCooldown > 0)
        {
            return linearVelocity;
        }

        const double climbSpeed = 40;

        linearVelocity = new Vector2(0, 0);

        if (_inputComponent.GetActionState(MoveLeftAction))
        {
            linearVelocity += new Vector2(-1, 0);
        }

        if (_inputComponent.GetActionState(MoveRightAction))
        {
            linearVelocity += new Vector2(1, 0);
        }

        if (_inputComponent.GetActionState(MoveUpAction))
        {
            linearVelocity += new Vector2(0, 1);
        }

        if (_inputComponent.GetActionState(MoveDownAction))
        {
            linearVelocity += new Vector2(0, -1);
        }

        linearVelocity = linearVelocity.OfLength(climbSpeed);

        var jumpState = _inputComponent.GetActionState(JumpAction);

        if (jumpState && !_lastJumpState)
        {
            var jumpDirection = -Math.Sign(_transform2DComponent.Scale.X);
            linearVelocity += new Vector2(150 * jumpDirection, 50);
            _reclimbAfterJumpCooldown = 10; // Prevent re-climbing immediately after jump
        }

        _lastJumpState = jumpState;

        return linearVelocity;
    }

    private void UpdateAnimationState(Vector2 linearVelocity, bool isOnGround, bool isOnLadder)
    {
        _spriteTransformComponent.Transform = _spriteDefaultTransform;

        if (isOnGround)
        {
            // On ground
            if (linearVelocity.X != 0)
            {
                // Moving
                if (!_spriteAnimationComponent.IsPlaying)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (_spriteAnimationComponent.Position == 0.5)
                    {
                        // Landing
                        _spriteAnimationComponent.Position = 0;
                    }
                    else
                    {
                        _spriteAnimationComponent.Position = 0.5;
                    }

                    _spriteAnimationComponent.Resume();
                }
            }
            else
            {
                // Idle
                _spriteAnimationComponent.Pause();
                _spriteAnimationComponent.Position = 0;
            }
        }
        else
        {
            if (isOnLadder)
            {
                // Climbing ladder
                _spriteAnimationComponent.Pause();
                _spriteAnimationComponent.Position = 0;

                if (linearVelocity != Vector2.Zero)
                {
                    _ladderClimbingAnimationTimer += GameTime.FixedDeltaTimeSeconds;
                    var leaningAngle = Math.Sin(_ladderClimbingAnimationTimer * 15) * 4;
                    _spriteTransformComponent.Rotation = Angle.Deg2Rad(leaningAngle);
                }
            }
            else
            {
                // In air
                _spriteAnimationComponent.Pause();
                _spriteAnimationComponent.Position = 0.5;
            }
        }
    }

    public void KillPlayer()
    {
        _respawnService.RequestRespawn();
    }

    public void Respawn()
    {
        _kinematicBodyComponent.LinearVelocity = Vector2.Zero;

        if (ActiveCheckPoint is null)
        {
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
            {
                Translation = _playerSpawnPointComponent.Entity.GetComponent<Transform2DComponent>().Translation
            });
        }
        else
        {
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
            {
                Translation = ActiveCheckPoint.Entity.GetComponent<Transform2DComponent>().Translation
            });
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlayerComponentFactory : ComponentFactory<PlayerComponent>
{
    private readonly IDebugRenderer _debugRenderer;
    private readonly RespawnService _respawnService;

    public PlayerComponentFactory(IDebugRenderer debugRenderer, RespawnService respawnService)
    {
        _debugRenderer = debugRenderer;
        _respawnService = respawnService;
    }

    protected override PlayerComponent CreateComponent(Entity entity) => new(entity, _debugRenderer, _respawnService);
}