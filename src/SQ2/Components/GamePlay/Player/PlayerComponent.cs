using System;
using System.Diagnostics;
using System.Linq;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Components.GamePlay.Common;
using SQ2.Components.GamePlay.Enemies;
using SQ2.Components.GamePlay.LevelGeometry;

namespace SQ2.Components.GamePlay.Player;

internal sealed class PlayerComponent : BehaviorComponent
{
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private RectangleColliderComponent? _rectangleColliderComponent;
    private Transform2DComponent? _transform2DComponent;
    private InputComponent _inputComponent = null!;
    private PlayerSpawnPointComponent? _playerSpawnPointComponent;
    private PlayerCheckPointComponent[] _checkPoints = Array.Empty<PlayerCheckPointComponent>();
    private int _currentCheckPointIndex = -1;
    private int _jumpPressFrames;
    private bool _lastJumpPressState;

    public PlayerComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _inputComponent = Entity.GetComponent<InputComponent>();

        _playerSpawnPointComponent = Scene.RootEntities.Single(e => e.HasComponent<PlayerSpawnPointComponent>()).GetComponent<PlayerSpawnPointComponent>();

        _checkPoints = Scene.RootEntities
            .Where(e => e.HasComponent<PlayerCheckPointComponent>())
            .Select(e => e.GetComponent<PlayerCheckPointComponent>())
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

        Movement.ApplyGravity(_kinematicRigidBody2DComponent);

        var linearVelocity = _kinematicRigidBody2DComponent.LinearVelocity;

        linearVelocity = HorizontalMovementLogic(linearVelocity);
        linearVelocity = JumpLogic(linearVelocity, isOnGround);

        _kinematicRigidBody2DComponent.LinearVelocity = linearVelocity;

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

    private Vector2 HorizontalMovementLogic(Vector2 linearVelocity)
    {
        const double maxSpeed = 100;
        const double acceleration = 250;
        const double deceleration = 450;

        if (_inputComponent.HardwareInput.KeyboardInput.Left)
        {
            var effectiveAcceleration = linearVelocity.X > 0 ? deceleration : acceleration;
            var verticalVelocity = linearVelocity.X - effectiveAcceleration * GameTime.FixedDeltaTimeSeconds;
            linearVelocity = linearVelocity.WithX(verticalVelocity);
        }

        if (_inputComponent.HardwareInput.KeyboardInput.Right)
        {
            var effectiveAcceleration = linearVelocity.X < 0 ? deceleration : acceleration;
            var verticalVelocity = linearVelocity.X + effectiveAcceleration * GameTime.FixedDeltaTimeSeconds;
            linearVelocity = linearVelocity.WithX(verticalVelocity);
        }

        if (_inputComponent.HardwareInput.KeyboardInput is { Left: false, Right: false })
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

        if (_inputComponent.HardwareInput.KeyboardInput.Up && _jumpPressFrames is > 0 and < maxJumpPressFrames)
        {
            _jumpPressFrames++;
            var jumpAcceleration = longJumpAcceleration - longJumpAcceleration * _jumpPressFrames / maxJumpPressFrames;
            linearVelocity = linearVelocity.WithY(linearVelocity.Y + jumpAcceleration * GameTime.FixedDeltaTimeSeconds);
        }

        if (isOnGround && _jumpPressFrames == 0 && _inputComponent.HardwareInput.KeyboardInput.Up && !_lastJumpPressState)
        {
            _jumpPressFrames++;
            linearVelocity = linearVelocity.WithY(baseJumpSpeed);
        }

        if (!_inputComponent.HardwareInput.KeyboardInput.Up)
        {
            _jumpPressFrames = 0;
        }

        _lastJumpPressState = _inputComponent.HardwareInput.KeyboardInput.Up;

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
    protected override PlayerComponent CreateComponent(Entity entity) => new(entity);
}