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
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;
using System;
using System.Diagnostics;
using System.Linq;

namespace SQ2.GamePlay.Player;

internal sealed class PlayerComponent : BehaviorComponent
{
    // Input Actions
    private const string MoveLeftAction = "MoveLeft";
    private const string MoveRightAction = "MoveRight";
    private const string MoveUpAction = "MoveUp";
    private const string MoveDownAction = "MoveDown";
    private const string JumpAction = "Jump";

    // Debugging
    private readonly IDebugRenderer _debugRenderer;

    // Movement and Physics
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private RectangleColliderComponent? _rectangleColliderComponent;
    private Transform2DComponent? _transform2DComponent;
    private InputComponent _inputComponent = null!;
    private int _jumpPressFrames;
    private bool _lastJumpState;

    // Ladders
    private readonly Vector2 _ladderClimbRange = new(8, 16);
    private AxisAlignedRectangle[] _ladderHitBoxes = Array.Empty<AxisAlignedRectangle>();
    private int _reclimbAfterJumpCooldown;

    // Respawn and Checkpoints
    private PlayerSpawnPointComponent? _playerSpawnPointComponent;
    private PlayerCheckPointComponent[] _checkPoints = Array.Empty<PlayerCheckPointComponent>();
    private int _currentCheckPointIndex = -1;

    public PlayerComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _inputComponent = Entity.GetComponent<InputComponent>();

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

        _checkPoints = Scene.RootEntities
            .Where(e => e.HasComponent<PlayerCheckPointComponent>())
            .Select(e => e.GetComponent<PlayerCheckPointComponent>())
            .ToArray();

        _ladderHitBoxes = Scene.RootEntities
            .Where(e => e.HasComponent<LadderComponent>())
            .Select(e => e.GetComponent<LadderComponent>().HitBox)
            .ToArray();
    }

    public override void OnFixedUpdate()
    {
        Debug.Assert(_kinematicRigidBody2DComponent != null, nameof(_kinematicRigidBody2DComponent) + " != null");
        Debug.Assert(_rectangleColliderComponent != null, nameof(_rectangleColliderComponent) + " != null");
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");

        var contacts = Array.Empty<Contact2D>();
        if (_rectangleColliderComponent.IsColliding)
        {
            contacts = _rectangleColliderComponent.GetContacts();
        }

        // Check for collisions with other entities.
        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.Root.HasComponent<SpikesComponent>())
            {
                Respawn();
                return;
            }

            if (contact2D.OtherCollider.Entity.Root.HasComponent<EnemyComponent>())
            {
                if (contact2D.CollisionNormal.Y > 0 && contact2D.OtherCollider.Entity.Root.GetComponent<EnemyComponent>().EnemyType is EnemyType.BlueSmall)
                {
                    _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(200);
                }
                else
                {
                    Respawn();
                    return;
                }
            }
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

        var isOnLadder = IsOnLadder();

        if (_reclimbAfterJumpCooldown > 0)
        {
            _reclimbAfterJumpCooldown--;
        }

        if (isOnLadder && !isOnGround)
        {
            var linearVelocity = _kinematicRigidBody2DComponent.LinearVelocity;

            linearVelocity = ClimbLadderLogic(linearVelocity);

            _kinematicRigidBody2DComponent.LinearVelocity = linearVelocity;
        }
        else
        {
            Movement.ApplyGravity(_kinematicRigidBody2DComponent);

            var linearVelocity = _kinematicRigidBody2DComponent.LinearVelocity;

            linearVelocity = HorizontalMovementLogic(linearVelocity);
            linearVelocity = JumpLogic(linearVelocity, isOnGround);

            if (isOnLadder && _inputComponent.GetActionState(MoveUpAction))
            {
                linearVelocity = ClimbLadderLogic(linearVelocity);
            }

            _kinematicRigidBody2DComponent.LinearVelocity = linearVelocity;
        }

        Movement.UpdateSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);

        // Check for checkpoints.
        for (var i = 0; i < _checkPoints.Length; i++)
        {
            var checkPointComponent = _checkPoints[i];
            if (checkPointComponent.Entity.GetComponent<Transform2DComponent>().Translation.Distance(_transform2DComponent.Translation) < 10)
            {
                _currentCheckPointIndex = i;
            }
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");

        foreach (var ladderHitBox in _ladderHitBoxes)
        {
            _debugRenderer.DrawRectangle(ladderHitBox, Color.Red, Matrix3x3.Identity);
        }

        _debugRenderer.DrawRectangle(new AxisAlignedRectangle(_transform2DComponent.Translation, _ladderClimbRange), Color.Black, Matrix3x3.Identity);
    }

    private Vector2 HorizontalMovementLogic(Vector2 linearVelocity)
    {
        const double maxSpeed = 100;
        const double acceleration = 250;
        const double deceleration = 450;

        var moveLeftState = _inputComponent.GetActionState(MoveLeftAction);
        var moveRightState = _inputComponent.GetActionState(MoveRightAction);

        if (moveLeftState)
        {
            var effectiveAcceleration = linearVelocity.X > 0 ? deceleration : acceleration;
            var verticalVelocity = linearVelocity.X - effectiveAcceleration * GameTime.FixedDeltaTimeSeconds;
            linearVelocity = linearVelocity.WithX(verticalVelocity);
        }

        if (moveRightState)
        {
            var effectiveAcceleration = linearVelocity.X < 0 ? deceleration : acceleration;
            var verticalVelocity = linearVelocity.X + effectiveAcceleration * GameTime.FixedDeltaTimeSeconds;
            linearVelocity = linearVelocity.WithX(verticalVelocity);
        }

        if (!moveLeftState && !moveRightState)
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
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");

        var playerHitBox = new AxisAlignedRectangle(_transform2DComponent.Translation, _ladderClimbRange);
        foreach (var ladderHitBox in _ladderHitBoxes)
        {
            if (ladderHitBox.Overlaps(playerHitBox)) return true;
        }

        return false;
    }

    private Vector2 ClimbLadderLogic(Vector2 linearVelocity)
    {
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");

        if (_reclimbAfterJumpCooldown > 0)
        {
            return linearVelocity;
        }

        const double climbSpeed = 40;

        linearVelocity = new Vector2(0, 0);

        if (_inputComponent.GetActionState(MoveLeftAction))
        {
            linearVelocity += new Vector2(-climbSpeed, 0);
        }

        if (_inputComponent.GetActionState(MoveRightAction))
        {
            linearVelocity += new Vector2(climbSpeed, 0);
        }

        if (_inputComponent.GetActionState(MoveUpAction))
        {
            linearVelocity += new Vector2(0, climbSpeed);
        }

        if (_inputComponent.GetActionState(MoveDownAction))
        {
            linearVelocity += new Vector2(0, -climbSpeed);
        }

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

    public void Respawn()
    {
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");
        Debug.Assert(_playerSpawnPointComponent != null, nameof(_playerSpawnPointComponent) + " != null");
        Debug.Assert(_kinematicRigidBody2DComponent != null, nameof(_kinematicRigidBody2DComponent) + " != null");

        foreach (var entity in Scene.RootEntities)
        {
            if (entity.HasComponent<EnemyComponent>())
            {
                entity.GetComponent<EnemyComponent>().Respawn();
            }

            if (entity.HasComponent<YellowEnemyComponent>())
            {
                entity.GetComponent<YellowEnemyComponent>().Respawn();
            }

            if (entity.HasComponent<DropPlatformComponent>())
            {
                entity.GetComponent<DropPlatformComponent>().Respawn();
            }
        }

        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;

        if (_currentCheckPointIndex < 0)
        {
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
            {
                Translation = _playerSpawnPointComponent.Entity.GetComponent<Transform2DComponent>().Translation
            });
        }
        else if (_currentCheckPointIndex < _checkPoints.Length)
        {
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
            {
                Translation = _checkPoints[_currentCheckPointIndex].Entity.GetComponent<Transform2DComponent>().Translation
            });
        }
        else
        {
            // No more checkpoints, respawn at the start.
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
            {
                Translation = _playerSpawnPointComponent.Entity.GetComponent<Transform2DComponent>().Translation
            });
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlayerComponentFactory : ComponentFactory<PlayerComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public PlayerComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override PlayerComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}