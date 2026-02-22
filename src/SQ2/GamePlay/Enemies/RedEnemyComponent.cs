using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Enemies;

internal sealed class RedEnemyComponent : BehaviorComponent, IRespawnable, IProximityActivatable
{
    private readonly ProximityActivationService _proximityActivationService;

    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _transform2DComponent = null!;

    private const double BaseVelocity = 20;
    private double _currentVelocity;
    private Vector2 _startPosition;

    public RedEnemyComponent(Entity entity, ProximityActivationService proximityActivationService) : base(entity)
    {
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
                playerComponent.KillPlayer();
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
internal sealed class RedEnemyComponentFactory : ComponentFactory<RedEnemyComponent>
{
    private readonly ProximityActivationService _proximityActivationService;

    public RedEnemyComponentFactory(ProximityActivationService proximityActivationService)
    {
        _proximityActivationService = proximityActivationService;
    }

    protected override RedEnemyComponent CreateComponent(Entity entity) => new(entity, _proximityActivationService);
}