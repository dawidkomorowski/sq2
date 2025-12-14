using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Development;
using System;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Boss.Blue;

internal sealed class BlueBossProjectileComponent : BehaviorComponent, IRespawnable
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BlueBoss;
    private readonly IDebugRenderer _debugRenderer;
    private Transform2DComponent _transform2DComponent = null!;
    private Transform2DComponent _playerTransform2DComponent = null!;
    private RectangleColliderComponent _playerRectangleColliderComponent = null!;
    private PlayerComponent _playerComponent = null!;
    private TimeSpan _lifetime;
    private readonly Rectangle _hitBox = new(new Vector2(0, -5), new Vector2(16, 16));

    public BlueBossProjectileComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public Vector2 Direction { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _playerTransform2DComponent = Query.GetPlayerTransform2DComponent(Scene);
        _playerRectangleColliderComponent = Query.GetPlayerRectangleColliderComponent(Scene);
        _playerComponent = Query.GetPlayerComponent(Scene);
    }

    public override void OnFixedUpdate()
    {
        _lifetime += GameTime.FixedDeltaTime;

        if (_lifetime >= TimeSpan.FromSeconds(5))
        {
            Entity.RemoveAfterFixedTimeStep();
            return;
        }

        const double speed = 200.0;
        _transform2DComponent.Translation += Direction * speed * GameTime.FixedDeltaTime.TotalSeconds;

        var playerAABB = new AxisAlignedRectangle(_playerTransform2DComponent.Translation, _playerRectangleColliderComponent.Dimensions);
        var transformedHitBox = _hitBox.Transform(_transform2DComponent.ToMatrix());
        if (transformedHitBox.Overlaps(playerAABB.ToRectangle()))
        {
            _playerComponent.KillPlayer();
            Entity.RemoveAfterFixedTimeStep();
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawRectangle(_hitBox.GetBoundingRectangle(), Color.FromArgb(255, 255, 255, 0), _transform2DComponent.ToMatrix());
        }
    }

    public void Respawn()
    {
        Entity.RemoveAfterFixedTimeStep();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueBossProjectileComponentFactory : ComponentFactory<BlueBossProjectileComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public BlueBossProjectileComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override BlueBossProjectileComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}