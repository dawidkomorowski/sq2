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

internal sealed class Bat2EnemyComponent : BehaviorComponent, IRespawnable
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BatEnemy;
    private readonly IDebugRenderer _debugRenderer;
    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicRigidBody2DComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private Vector2 _initialPosition;
    private const double AgroRadius = 100;

    public Bat2EnemyComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);

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

        var toPlayer = _playerTransform.Translation - _transform2DComponent.Translation;
        if (_playerTransform.Translation.Distance(_initialPosition) <= AgroRadius)
        {
            var direction = toPlayer.Unit;
            const float speed = 60f;
            _kinematicRigidBody2DComponent.LinearVelocity = direction * speed;
        }
        else
        {
            _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawCircle(new Circle(_initialPosition, AgroRadius), Color.Red);
        }
    }

    public void Respawn()
    {
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _initialPosition
        });
        _kinematicRigidBody2DComponent.LinearVelocity = Vector2.Zero;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Bat2EnemyComponentFactory : ComponentFactory<Bat2EnemyComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public Bat2EnemyComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override Bat2EnemyComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}