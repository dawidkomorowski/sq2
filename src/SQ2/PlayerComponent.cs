using System.Diagnostics;
using System.Linq;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;

namespace SQ2;

internal sealed class PlayerComponent : BehaviorComponent
{
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private RectangleColliderComponent? _rectangleColliderComponent;
    private Transform2DComponent? _transform2DComponent;
    private InputComponent? _inputComponent;
    private Transform2DComponent? _cameraTransform;

    public PlayerComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _inputComponent = Entity.GetComponent<InputComponent>();

        _cameraTransform = Scene.RootEntities.Single(e => e.HasComponent<CameraComponent>()).GetComponent<Transform2DComponent>();
    }

    public override void OnFixedUpdate()
    {
        Debug.Assert(_kinematicRigidBody2DComponent != null, nameof(_kinematicRigidBody2DComponent) + " != null");
        Debug.Assert(_rectangleColliderComponent != null, nameof(_rectangleColliderComponent) + " != null");
        Debug.Assert(_inputComponent != null, nameof(_inputComponent) + " != null");

        // Basic gravity simulation.
        // TODO Maybe move it to GlobalSettings?
        var gravity = new Vector2(0, -500);

        _kinematicRigidBody2DComponent.LinearVelocity += gravity * GameTime.FixedDeltaTimeSeconds;

        // Limit vertical velocity.
        const int limit = -200;
        if (_kinematicRigidBody2DComponent.LinearVelocity.Y < limit)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(limit);
        }

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

        // Basic jumping.
        var canJump = false;
        if (_rectangleColliderComponent.IsColliding)
        {
            foreach (var contact2D in _rectangleColliderComponent.GetContacts())
            {
                if (contact2D.CollisionNormal.Y > 0)
                {
                    canJump = true;
                    break;
                }
            }
        }

        if (canJump && _inputComponent.HardwareInput.KeyboardInput.Up)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(200);
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        // Basic camera follow.
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");
        Debug.Assert(_cameraTransform != null, nameof(_cameraTransform) + " != null");

        const double minDistance = 30;
        var distanceToPlayer = _cameraTransform.Translation.Distance(_transform2DComponent.Translation);
        if (distanceToPlayer > minDistance)
        {
            const double baseVelocity = 20;
            var distanceFactor = distanceToPlayer - minDistance;
            var directionToPlayer = (_transform2DComponent.Translation - _cameraTransform.Translation).Unit;
            _cameraTransform.Translation += directionToPlayer * distanceFactor * baseVelocity * gameTime.DeltaTimeSeconds;
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlayerComponentFactory : ComponentFactory<PlayerComponent>
{
    protected override PlayerComponent CreateComponent(Entity entity) => new(entity);
}