using System.Collections.Generic;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Boss;

internal sealed class KeyComponent : BehaviorComponent, IRespawnable
{
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private PlayerComponent _playerComponent = null!;

    private readonly List<Contact2D> _contacts = new();
    private bool _isFullyDropped;

    public KeyComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _playerTransform = Query.GetPlayerTransformComponent(Scene);
        _playerComponent = Query.GetPlayerComponent(Scene);
    }

    public override void OnFixedUpdate()
    {
        if (_isFullyDropped)
        {
            var distanceToPlayer = _transform2DComponent.Translation.Distance(_playerTransform.Translation);
            if (distanceToPlayer < 13)
            {
                Entity.RemoveAfterFixedTimeStep();
            }
        }
        else
        {
            Movement.ApplyGravity(_kinematicComponent);

            var contacts = _rectangleColliderComponent.GetContactsAsSpan(_contacts);
            foreach (var contact2D in contacts)
            {
                if (contact2D.CollisionNormal.Y > 0)
                {
                    _isFullyDropped = true;
                    _kinematicComponent.EnableCollisionResponse = false;
                    _kinematicComponent.LinearVelocity = Vector2.Zero;
                }
            }
        }
    }

    public void Respawn()
    {
        Entity.RemoveAfterFixedTimeStep();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class KeyComponentFactory : ComponentFactory<KeyComponent>
{
    protected override KeyComponent CreateComponent(Entity entity) => new(entity);
}