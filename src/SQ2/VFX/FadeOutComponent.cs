using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;

namespace SQ2.VFX;

internal sealed class FadeOutComponent : BehaviorComponent
{
    private RectangleRendererComponent _rectangleRendererComponent = null!;
    private readonly TimeSpan _fadeOutDuration = TimeSpan.FromSeconds(1);
    private TimeSpan _elapsedFadeOutTime;

    public FadeOutComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        Entity.CreateComponent<Transform2DComponent>();
        _rectangleRendererComponent = Entity.CreateComponent<RectangleRendererComponent>();
        _rectangleRendererComponent.SortingLayerName = GlobalSettings.SortingLayers.CameraEffects;
        _rectangleRendererComponent.Color = Color.FromArgb(0, 0, 0, 0);
        _rectangleRendererComponent.Dimensions = GlobalSettings.ViewSize * 2;
        _rectangleRendererComponent.FillInterior = true;

        _elapsedFadeOutTime = TimeSpan.Zero;
    }

    public override void OnUpdate(GameTime gameTime)
    {
        _elapsedFadeOutTime += gameTime.DeltaTime;

        var ratio = _fadeOutDuration.TotalSeconds > 0
            ? _elapsedFadeOutTime.TotalSeconds / _fadeOutDuration.TotalSeconds
            : 1.0;
        ratio = Math.Clamp(ratio, 0.0, 1.0);

        var color = _rectangleRendererComponent.Color;
        var alpha = (byte)Math.Round(255 * ratio);
        _rectangleRendererComponent.Color = Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class FadeOutComponentFactory : ComponentFactory<FadeOutComponent>
{
    protected override FadeOutComponent CreateComponent(Entity entity) => new(entity);
}