using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.Development;

namespace SQ2.GamePlay.Common;

internal sealed class LevelCompleteTriggerComponent : BehaviorComponent
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.LevelCompleteTrigger;
    private readonly IDebugRenderer _debugRenderer;

    public LevelCompleteTriggerComponent(Entity entity, IDebugRenderer debugRenderer) : base(entity)
    {
        _debugRenderer = debugRenderer;
    }

    public AxisAlignedRectangle TriggerArea { get; set; }

    public override void OnUpdate(GameTime gameTime)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawRectangle(TriggerArea, Color.Red, Matrix3x3.Identity);
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LevelCompleteTriggerComponentFactory : ComponentFactory<LevelCompleteTriggerComponent>
{
    private readonly IDebugRenderer _debugRenderer;

    public LevelCompleteTriggerComponentFactory(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    protected override LevelCompleteTriggerComponent CreateComponent(Entity entity) => new(entity, _debugRenderer);
}