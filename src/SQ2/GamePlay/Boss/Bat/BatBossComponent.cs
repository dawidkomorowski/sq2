using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;
using SQ2.GamePlay.Enemies;

namespace SQ2.GamePlay.Boss.Bat;

internal sealed class BatBossComponent : BehaviorComponent, IRespawnable
{
    private readonly EntityFactory _entityFactory;

    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;

    private double _secondsTimer;

    public BatBossComponent(Entity entity, EntityFactory entityFactory) : base(entity)
    {
        _entityFactory = entityFactory;
    }

    public Vector2 TargetPoint { get; set; }
    public double Velocity { get; set; }
    public DropType Drop { get; set; }
    public double DropAfterSeconds { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
    }

    public override void OnFixedUpdate()
    {
        _secondsTimer += GameTime.FixedDeltaTimeSeconds;

        HandleDrop();

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

    public void Respawn()
    {
        Entity.RemoveAfterFixedTimeStep();
    }

    private void HandleDrop()
    {
        if (_secondsTimer >= DropAfterSeconds)
        {
            if (Drop is DropType.BlueEnemy)
            {
                var initialMovementDirection = TargetPoint.X > _transform2DComponent.Translation.X ? MovementDirection.Right : MovementDirection.Left;
                var entity = _entityFactory.CreateBlueEnemy(Scene, _transform2DComponent.Translation + new Vector2(0, -20), initialMovementDirection, false, 0);
                entity.GetComponent<BlueEnemyComponent>().RemoveOnRespawn = true;
                _secondsTimer = 0;
            }

            Drop = DropType.None;
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BatBossComponentFactory : ComponentFactory<BatBossComponent>
{
    private readonly EntityFactory _entityFactory;

    public BatBossComponentFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    protected override BatBossComponent CreateComponent(Entity entity) => new(entity, _entityFactory);
}