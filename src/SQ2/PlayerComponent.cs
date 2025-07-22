using System.Diagnostics;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics.Components;

namespace SQ2;

internal sealed class PlayerComponent : BehaviorComponent
{
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private InputComponent? _inputComponent;

    public PlayerComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _inputComponent = Entity.GetComponent<InputComponent>();
    }

    public override void OnFixedUpdate()
    {
        Debug.Assert(_kinematicRigidBody2DComponent != null, nameof(_kinematicRigidBody2DComponent) + " != null");
        Debug.Assert(_inputComponent != null, nameof(_inputComponent) + " != null");

        // Basic gravity simulation.
        // TODO Maybe move it to GlobalSettings?
        var gravity = new Vector2(0, -100);

        _kinematicRigidBody2DComponent.LinearVelocity += gravity * GameTime.FixedDeltaTimeSeconds;

        // Basic player movement.
        const double velocity = 30;
        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(0);
        if (_inputComponent.HardwareInput.KeyboardInput.Left)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(-velocity);
        }

        if (_inputComponent.HardwareInput.KeyboardInput.Right)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(velocity);
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlayerComponentFactory : ComponentFactory<PlayerComponent>
{
    protected override PlayerComponent CreateComponent(Entity entity) => new(entity);
}