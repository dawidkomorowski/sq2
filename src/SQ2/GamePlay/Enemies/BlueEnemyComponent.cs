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

internal sealed class BlueEnemyComponent : BehaviorComponent, IRespawnable, IProximityActivatable
{
    internal static readonly Vector2 SpriteOffset = new(-1, 5);

    private readonly EntityFactory _entityFactory;
    private readonly RespawnService _respawnService;
    private readonly ProximityActivationService _proximityActivationService;

    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;

    private const double BaseVelocity = 20;
    private double _currentVelocity;
    private Vector2 _startPosition;

    public BlueEnemyComponent(Entity entity, EntityFactory entityFactory, RespawnService respawnService,
        ProximityActivationService proximityActivationService) : base(entity)
    {
        _entityFactory = entityFactory;
        _respawnService = respawnService;
        _proximityActivationService = proximityActivationService;
    }

    public bool RequireActivation { get; set; }
    public MovementDirection InitialMovementDirection { get; set; }

    public override void OnStart()
    {
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();

        _startPosition = _transform2DComponent.Translation;
        _currentVelocity = Movement.GetVelocityForDirection(InitialMovementDirection, BaseVelocity);

        if (RequireActivation)
        {
            _proximityActivationService.Register(this);
        }
    }

    public override void OnFixedUpdate()
    {
        if (RequireActivation && !Active)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
            return;
        }

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

        Movement.UpdateHorizontalSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);
    }

    private void Die()
    {
        Entity.RemoveAfterFixedTimeStep();

        if (RequireActivation)
        {
            _proximityActivationService.Unregister(this);
        }

        _respawnService.AddOneTimeRespawnAction(() =>
        {
            _entityFactory.CreateBlueEnemy(Scene, _startPosition, InitialMovementDirection, RequireActivation, ActivationGroup);
        });

        var offset = new Vector2(SpriteOffset.X * _transform2DComponent.Scale.X, SpriteOffset.Y);
        _entityFactory.CreateBlueEnemyDeathAnimation(Scene, _transform2DComponent.Translation + offset, _transform2DComponent.Scale);
    }

    #region IRespawnable

    public void Respawn()
    {
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _startPosition
        });
        _currentVelocity = Movement.GetVelocityForDirection(InitialMovementDirection, BaseVelocity);
    }

    #endregion

    #region IProximityActivatable

    public Vector2 Position => _transform2DComponent.Translation;
    public int ActivationGroup { get; set; }
    public bool Active { get; set; }

    #endregion
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueEnemyComponentFactory : ComponentFactory<BlueEnemyComponent>
{
    private readonly EntityFactory _entityFactory;
    private readonly RespawnService _respawnService;
    private readonly ProximityActivationService _proximityActivationService;

    public BlueEnemyComponentFactory(EntityFactory entityFactory, RespawnService respawnService, ProximityActivationService proximityActivationService)
    {
        _entityFactory = entityFactory;
        _respawnService = respawnService;
        _proximityActivationService = proximityActivationService;
    }

    protected override BlueEnemyComponent CreateComponent(Entity entity) => new(entity, _entityFactory, _respawnService, _proximityActivationService);
}