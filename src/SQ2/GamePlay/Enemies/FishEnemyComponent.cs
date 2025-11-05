using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;

namespace SQ2.GamePlay.Enemies;

internal sealed class FishEnemyComponent : BehaviorComponent, IRespawnable
{
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;
    private Vector2 _startPosition;
    private int _jumpCooldown = 0;

    public FishEnemyComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _startPosition = _transform2DComponent.Translation;
    }

    public override void OnFixedUpdate()
    {
        var contacts = _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();
        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();
                playerComponent.KillPlayer();
                break;
            }
        }

        if (_jumpCooldown > 0)
        {
            _jumpCooldown--;
        }

        if (_jumpCooldown == 0)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(300);
            _jumpCooldown = 180; // Cooldown of 3 second assuming 60 FPS
        }
        else
        {
            if (_transform2DComponent.Translation.Y > _startPosition.Y)
            {
                Movement.ApplyGravity(_kinematicRigidBody2DComponent);
            }
            else
            {
                _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(0);
                _transform2DComponent.Translation = _startPosition;
            }
        }

        Movement.UpdateSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);
    }

    public void Respawn()
    {
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _startPosition
        });
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        _jumpCooldown = 0;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class FishEnemyComponentFactory : ComponentFactory<FishEnemyComponent>
{
    protected override FishEnemyComponent CreateComponent(Entity entity) => new(entity);
}