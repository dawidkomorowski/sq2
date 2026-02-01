using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.Development;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;

namespace SQ2.GamePlay.Enemies;

internal sealed class BatEnemyComponent : BehaviorComponent, IRespawnable
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BatEnemy;
    private readonly IDebugRenderer _debugRenderer;
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private Vector2 _initialPosition;
    private Direction _direction = Direction.ToEnd;

    public BatEnemyComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public Vector2 StartPosition { get; set; }
    public Vector2 EndPosition { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();

        _initialPosition = _transform2DComponent.Translation;
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

        const double v = 60;
        var dv = v * GameTime.FixedDeltaTimeSeconds;

        var targetPosition = _direction switch
        {
            Direction.ToEnd => EndPosition,
            _ => StartPosition
        };

        var currentPosition = _transform2DComponent.Translation;
        var toTarget = targetPosition - currentPosition;

        if (toTarget.Length <= dv)
        {
            _transform2DComponent.Translation = targetPosition;
            _direction = _direction switch
            {
                Direction.ToEnd => Direction.ToStart,
                _ => Direction.ToEnd
            };
        }

        var moveDirection = toTarget.Unit;
        _kinematicRigidBody2DComponent.LinearVelocity = moveDirection * v;

        Movement.UpdateHorizontalSpriteFacing(_transform2DComponent, _kinematicRigidBody2DComponent);
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawCircle(new Circle(StartPosition, 2), Color.Red);
            _debugRenderer.DrawCircle(new Circle(EndPosition, 2), Color.Blue);
        }
    }

    public void Respawn()
    {
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _initialPosition
        });
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        _direction = Direction.ToEnd;
    }

    private enum Direction
    {
        ToStart,
        ToEnd
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BatEnemyComponentFactory : ComponentFactory<BatEnemyComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public BatEnemyComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override BatEnemyComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}