using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using System;
using System.Diagnostics;

namespace SQ2.Components.GamePlay;

internal sealed class EnemyComponent : BehaviorComponent
{
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private RectangleColliderComponent? _rectangleColliderComponent;

    private const double BaseVelocity = 20;
    private double _currentVelocity = BaseVelocity;

    public EnemyComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
    }

    public override void OnFixedUpdate()
    {
        Debug.Assert(_kinematicRigidBody2DComponent != null, nameof(_kinematicRigidBody2DComponent) + " != null");
        Debug.Assert(_rectangleColliderComponent != null, nameof(_rectangleColliderComponent) + " != null");

        // Basic gravity simulation.
        // TODO Maybe move it to GlobalSettings?
        // TODO Gravity is duplicated in PlayerComponent.
        var gravity = new Vector2(0, -500);

        _kinematicRigidBody2DComponent.LinearVelocity += gravity * GameTime.FixedDeltaTimeSeconds;

        // Limit vertical velocity.
        const int limit = -200;
        if (_kinematicRigidBody2DComponent.LinearVelocity.Y < limit)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(limit);
        }


        // Basic enemy movement logic.
        var contacts = Array.Empty<Contact2D>();
        if (_rectangleColliderComponent.IsColliding)
        {
            contacts = _rectangleColliderComponent.GetContacts();
        }

        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.X < 0)
            {
                // Enemy hit a wall on the right side, change direction.
                _currentVelocity = -BaseVelocity;
                break;
            }

            if (contact2D.CollisionNormal.X > 0)
            {
                // Enemy hit a wall on the left side, change direction.
                _currentVelocity = BaseVelocity;
                break;
            }
        }

        _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithX(_currentVelocity);
    }

    public override void OnUpdate(GameTime gameTime)
    {
        // Implement enemy behavior during regular updates.
    }
}

internal sealed class EnemyComponentFactory : ComponentFactory<EnemyComponent>
{
    protected override EnemyComponent CreateComponent(Entity entity) => new(entity);
}