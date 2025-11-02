using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Enemies;

internal sealed class BlueEnemyComponent : BehaviorComponent, IRespawnable
{
    internal static readonly Vector2 SpriteOffset = new(-1, 5);

    private readonly EntityFactory _entityFactory;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;

    private const double BaseVelocity = 20;
    private double _currentVelocity = -BaseVelocity;
    private Vector2 _startPosition;

    public BlueEnemyComponent(Entity entity, EntityFactory entityFactory) : base(entity)
    {
        _entityFactory = entityFactory;
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
        Movement.ApplyGravity(_kinematicRigidBody2DComponent);

        var contacts = _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();

        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();

                if (contact2D.CollisionNormal.Y < 0)
                {
                    var playerKinematicComponent = playerComponent.Entity.GetComponent<KinematicRigidBody2DComponent>();
                    playerKinematicComponent.LinearVelocity = playerKinematicComponent.LinearVelocity.WithY(200);

                    Die();
                }
                else
                {
                    playerComponent.KillPlayer();
                }

                break;
            }

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

        Movement.UpdateSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);
    }

    public void Respawn()
    {
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _startPosition
        });
        _currentVelocity = -BaseVelocity;
    }

    private void Die()
    {
        Entity.RemoveAfterFixedTimeStep();

        RespawnService.AddOneTimeRespawnAction(() =>
        {
            var (tx, ty) = Geometry.GetTileCoordinates(_startPosition);
            _entityFactory.CreateBlueEnemy(Scene, tx, ty);
        });

        _entityFactory.CreateBlueEnemyDeathAnimation(Scene, _transform2DComponent.Translation + SpriteOffset);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueEnemyComponentFactory : ComponentFactory<BlueEnemyComponent>
{
    private readonly EntityFactory _entityFactory;

    public BlueEnemyComponentFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    protected override BlueEnemyComponent CreateComponent(Entity entity) => new(entity, _entityFactory);
}