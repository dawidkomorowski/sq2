using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;

namespace SQ2.GamePlay.Boss.Bat;

internal sealed class BatBossComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;

    public BatBossComponent(Entity entity) : base(entity)
    {
    }

    public Vector2 TargetPoint { get; set; }
    public double Velocity { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
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

        var dv = Velocity * GameTime.FixedDeltaTimeSeconds;

        var currentPosition = _transform2DComponent.Translation;
        var toTarget = TargetPoint - currentPosition;

        if (toTarget.Length <= dv)
        {
            _transform2DComponent.Translation = TargetPoint;
            Entity.RemoveAfterFixedTimeStep();
        }

        var moveDirection = toTarget.Unit;
        _kinematicRigidBody2DComponent.LinearVelocity = moveDirection * Velocity;

        Movement.UpdateHorizontalSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BatBossComponentFactory : ComponentFactory<BatBossComponent>
{
    protected override BatBossComponent CreateComponent(Entity entity) => new(entity);
}