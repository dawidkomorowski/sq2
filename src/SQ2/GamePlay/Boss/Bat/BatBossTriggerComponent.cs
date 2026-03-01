using System.Collections.Generic;
using System.Linq;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Development;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Boss.Bat;

internal sealed class BatBossTriggerComponent : BehaviorComponent, IRespawnable
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.BatBoss;
    private readonly IDebugRenderer _debugRenderer;
    private readonly List<BatBossSpawnerComponent> _spawners = new();
    private Transform2DComponent _playerTransform2DComponent = null!;
    private RectangleColliderComponent _playerRectangleColliderComponent = null!;
    private bool _hasBeenActivated;

    public BatBossTriggerComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public AxisAlignedRectangle TriggerArea { get; set; }
    public double TimerStartValue { get; set; }

    public override void OnStart()
    {
        _spawners.AddRange(
            Scene.AllEntities
                .Where(e => e.HasComponent<BatBossSpawnerComponent>())
                .Select(e => e.GetComponent<BatBossSpawnerComponent>())
        );

        _playerTransform2DComponent = Query.GetPlayerTransform2DComponent(Scene);
        _playerRectangleColliderComponent = Query.GetPlayerRectangleColliderComponent(Scene);

        if (TimerStartValue > 0)
        {
            SetSpawnerTimerStartValue(TimerStartValue);
        }
    }

    public override void OnFixedUpdate()
    {
        if (_hasBeenActivated) return;

        var playerPosition = _playerTransform2DComponent.Translation;
        var playerDimensions = _playerRectangleColliderComponent.Dimensions;
        var playerHitBox = new AxisAlignedRectangle(playerPosition, playerDimensions);

        if (TriggerArea.Overlaps(playerHitBox))
        {
            _hasBeenActivated = true;

            foreach (var spawner in _spawners)
            {
                spawner.Active = true;
            }
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawRectangle(TriggerArea, Color.Red, Matrix3x3.Identity);
        }
    }

    public void Respawn()
    {
        _hasBeenActivated = false;
    }

    /// <summary>
    ///     Debug only. Sets the start value for the internal timer of all spawners.
    /// </summary>
    private void SetSpawnerTimerStartValue(double value)
    {
        foreach (var spawner in _spawners)
        {
            spawner.TimerStartValue = value;
            spawner.Respawn(); // Force internal timer to be set to the new start value.
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BatBossTriggerComponentFactory : ComponentFactory<BatBossTriggerComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public BatBossTriggerComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override BatBossTriggerComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}