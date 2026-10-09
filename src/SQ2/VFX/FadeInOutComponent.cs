using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;

namespace SQ2.VFX;

internal sealed class FadeInOutComponent : BehaviorComponent
{
    private RectangleRendererComponent _rectangleRendererComponent = null!;
    private TimeSpan _elapsed;
    private bool _completed;

    public FadeInOutComponent(Entity entity) : base(entity)
    {
    }

    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan CompleteDelay { get; set; } = TimeSpan.Zero;
    public Action? OnComplete { get; set; }
    public string SortingLayerName { get; set; } = GlobalSettings.SortingLayers.CameraEffects;
    public FadeMode Mode { get; set; } = FadeMode.Out;

    public override void OnStart()
    {
        Entity.CreateComponent<Transform2DComponent>();
        _rectangleRendererComponent = Entity.CreateComponent<RectangleRendererComponent>();
        _rectangleRendererComponent.SortingLayerName = SortingLayerName;
        _rectangleRendererComponent.Color = Color.FromArgb(0, 0, 0, 0);
        _rectangleRendererComponent.Dimensions = GlobalSettings.ViewSize * 2;
        _rectangleRendererComponent.FillInterior = true;

        _elapsed = TimeSpan.Zero;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        if (_completed)
        {
            return;
        }

        _elapsed += timeStep.DeltaTime;

        var ratio = Duration.TotalSeconds > 0
            ? _elapsed.TotalSeconds / Duration.TotalSeconds
            : 1.0;
        ratio = Math.Clamp(ratio, 0.0, 1.0);

        if (Mode is FadeMode.In)
        {
            ratio = 1 - ratio;
        }

        var color = _rectangleRendererComponent.Color;
        var alpha = (byte)Math.Round(255 * ratio);
        _rectangleRendererComponent.Color = Color.FromArgb(alpha, color.R, color.G, color.B);

        if (_elapsed > Duration + CompleteDelay)
        {
            _completed = true;
            OnComplete?.Invoke();
        }
    }

    public enum FadeMode
    {
        In,
        Out
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class FadeInOutComponentFactory : ComponentFactory<FadeInOutComponent>
{
    protected override FadeInOutComponent CreateComponent(Entity entity) => new(entity);
}