using System;
using System.Diagnostics;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Components.GamePlay.Common;
using SQ2.Components.GamePlay.Player;

namespace SQ2.Components.GamePlay.Enemies;

internal sealed class YellowEnemyComponent : BehaviorComponent
{
    private const bool EnableDebugRendering = true;
    private readonly IDebugRenderer _debugRenderer;
    private Transform2DComponent? _transform2DComponent;
    private KinematicRigidBody2DComponent? _kinematicRigidBody2DComponent;
    private RectangleColliderComponent? _rectangleColliderComponent;
    private Transform2DComponent? _playerTransform2DComponent;
    private RectangleColliderComponent? _playerRectangleColliderComponent;
    private PlayerComponent? _playerComponent;
    private AxisAlignedRectangle _detector;
    private State _state = State.Ready;

    public YellowEnemyComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _kinematicRigidBody2DComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _playerTransform2DComponent = Query.GetPlayerTransform2DComponent(Scene);
        _playerRectangleColliderComponent = Query.GetPlayerRectangleColliderComponent(Scene);
        _playerComponent = Query.GetPlayerComponent(Scene);

        _detector = CreateDetector();
    }

    public override void OnFixedUpdate()
    {
        switch (_state)
        {
            case State.Ready:
                ReadyStateUpdate();
                break;
            case State.Falling:
                FallingStateUpdate();
                break;
            case State.Lifting:
                // Lifting state logic.
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        KillPlayerOnContact();
    }

    private void ReadyStateUpdate()
    {
        Debug.Assert(_playerTransform2DComponent != null, nameof(_playerTransform2DComponent) + " != null");
        Debug.Assert(_playerRectangleColliderComponent != null, nameof(_playerRectangleColliderComponent) + " != null");

        var playerAABB = new AxisAlignedRectangle(_playerTransform2DComponent.Translation, _playerRectangleColliderComponent.Dimensions);
        if (_detector.Overlaps(playerAABB))
        {
            _state = State.Falling;
        }
    }

    private void FallingStateUpdate()
    {
        Debug.Assert(_kinematicRigidBody2DComponent != null, nameof(_kinematicRigidBody2DComponent) + " != null");
        Debug.Assert(_rectangleColliderComponent != null, nameof(_rectangleColliderComponent) + " != null");

        const double fallingAcceleration = 500;
        const double maxFallingSpeed = 250;

        _kinematicRigidBody2DComponent.LinearVelocity += new Vector2(0, -fallingAcceleration * GameTime.FixedDeltaTimeSeconds);
        if (_kinematicRigidBody2DComponent.LinearVelocity.Y < -maxFallingSpeed)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(-maxFallingSpeed);
        }

        if (_rectangleColliderComponent.IsColliding)
        {
            _kinematicRigidBody2DComponent.LinearVelocity = _kinematicRigidBody2DComponent.LinearVelocity.WithY(0);
            _state = State.Lifting;
        }
    }

    private void KillPlayerOnContact()
    {
        Debug.Assert(_rectangleColliderComponent != null, nameof(_rectangleColliderComponent) + " != null");
        Debug.Assert(_playerComponent != null, nameof(_playerComponent) + " != null");
        if (!_rectangleColliderComponent.IsColliding)
        {
            return;
        }

        var contacts = _rectangleColliderComponent.GetContacts();
        foreach (var contact in contacts)
        {
            if (contact.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                _playerComponent.Respawn();
                break;
            }
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (EnableDebugRendering)
        {
            _debugRenderer.DrawRectangle(_detector, Color.Red, Matrix3x3.Identity);
        }
    }

    private AxisAlignedRectangle CreateDetector()
    {
        Debug.Assert(_transform2DComponent != null, nameof(_transform2DComponent) + " != null");
        var (tx, ty) = Geometry.GetTileCoordinates(_transform2DComponent.Translation);
        var detectorHeight = 0;

        for (var i = 1; i < 10; i++)
        {
            if (Query.TileHitTest(Scene, tx, ty - i))
            {
                break;
            }

            detectorHeight++;
        }

        return Geometry.GetWorldRectangle(tx, ty - 1, tx, ty - detectorHeight);
    }

    private enum State
    {
        Ready,
        Falling,
        Lifting
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class YellowEnemyComponentFactory : ComponentFactory<YellowEnemyComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public YellowEnemyComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override YellowEnemyComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}