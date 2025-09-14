using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;

namespace SQ2.Components.GamePlay.LevelGeometry;

internal sealed class DropPlatformComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private Vector2 _startPosition;

    public DropPlatformComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();

        _startPosition = _transform2DComponent.Translation;
    }

    public override void OnFixedUpdate()
    {
        const double dropSpeed = 50;
        const double liftSpeed = 25;
        var hasLoad = false;

        if (_rectangleColliderComponent.IsColliding)
        {
            var contacts = _rectangleColliderComponent.GetContacts();
            foreach (var contact in contacts)
            {
                if (contact.CollisionNormal.Y < 0)
                {
                    hasLoad = true;
                    break;
                }
            }
        }

        if (hasLoad)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, -dropSpeed);
        }
        else
        {
            if (_transform2DComponent.Translation.Y >= _startPosition.Y)
            {
                _transform2DComponent.Translation = _startPosition;
                _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
                return;
            }

            _kinematicRigidBody2DComponent.LinearVelocity = new Vector2(0, liftSpeed);
        }
    }

    // TODO: Add reset to start position when player respawns on a checkpoint
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DropPlatformComponentFactory : ComponentFactory<DropPlatformComponent>
{
    protected override DropPlatformComponent CreateComponent(Entity entity) => new(entity);
}