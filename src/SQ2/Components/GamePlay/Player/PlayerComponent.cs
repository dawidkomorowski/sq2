using System;
using System.Diagnostics;
using System.Linq;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Components.GamePlay.Common;
using SQ2.Components.GamePlay.Enemies;

namespace SQ2.Components.GamePlay.Player;

internal sealed class PlayerComponent : BehaviorComponent
{
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private RectangleColliderComponent? _rectangleColliderComponent;
    private Transform2DComponent? _transform2DComponent;
    private InputComponent? _inputComponent;
    private PlayerSpawnPointComponent? _playerSpawnPointComponent;
    private PlayerCheckPointComponent[] _checkPoints = Array.Empty<PlayerCheckPointComponent>();
    private int _currentCheckPointIndex = -1;

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
        Debug.Assert(_inputComponent != null, nameof(_inputComponent) + " != null");
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");

        GravityPhysics.Update(_kinematicRigidBody2DComponent);

        // Basic player movement.
        const double velocity = 80;
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);
        if (_inputComponent.HardwareInput.KeyboardInput.Left)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(-velocity);
        }

        if (_inputComponent.HardwareInput.KeyboardInput.Right)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(velocity);
        }

        // Flip player sprite based on movement direction.
        if (_kinematicRigidBody2DComponent.LinearVelocity.X > 0)
        {
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with { Scale = new Vector2(-1, 1) });
        }

        if (_kinematicRigidBody2DComponent.LinearVelocity.X < 0)
        {
            _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with { Scale = new Vector2(1, 1) });
        }

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

        // Basic jumping.
        var canJump = false;
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.Y > 0)
            {
                canJump = true;
                break;
            }
        }

        if (canJump && _inputComponent.HardwareInput.KeyboardInput.Up)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(200);
        }

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

    private void Respawn()
    {
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");
        Debug.Assert(_playerSpawnPointComponent != null, nameof(_playerSpawnPointComponent) + " != null");

        foreach (var entity in Scene.RootEntities)
        {
            if (entity.HasComponent<EnemyComponent>())
            {
                entity.GetComponent<EnemyComponent>().Respawn();
            }
        }

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