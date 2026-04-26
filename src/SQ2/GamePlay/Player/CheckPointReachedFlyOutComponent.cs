using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.GamePlay.Player;

internal sealed class CheckPointReachedFlyOutComponent : BehaviorComponent
{
    private Transform2DComponent _transform = null!;
    private TextRendererComponent _textRenderer = null!;
    private readonly TimeSpan _duration = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _fadeOutStartTime = TimeSpan.FromSeconds(1);
    private TimeSpan _time = TimeSpan.Zero;

    public CheckPointReachedFlyOutComponent(Entity entity) : base(entity)
    {
    }

    public Vector2 StartPosition { get; set; }

    public override void OnStart()
    {
        _transform = Entity.CreateComponent<Transform2DComponent>();
        _transform.Translation = StartPosition;

        _textRenderer = Entity.CreateComponent<TextRendererComponent>();
        _textRenderer.SortingLayerName = GlobalSettings.SortingLayers.Hud;
        _textRenderer.Text = "Checkpoint Reached!";
        _textRenderer.TextAlignment = TextAlignment.Center;
        _textRenderer.MaxWidth = 100;
        _textRenderer.Pivot = new Vector2(50, 0);
        _textRenderer.FontSize = FontSize.FromDips(10);
        _textRenderer.Color = Color.White;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        _time += timeStep.DeltaTime;
        _transform.Translation = StartPosition + new Vector2(0, _time.TotalSeconds * 10);

        var alpha = Math.Max(0, (_time - _fadeOutStartTime).TotalSeconds) / (_duration - _fadeOutStartTime).TotalSeconds;
        _textRenderer.Color = Color.Lerp(Color.White, Color.FromArgb(0, 255, 255, 255), alpha);

        if (_time > _duration)
        {
            Entity.RemoveAfterFullFrame();
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CheckPointReachedFlyOutComponentFactory : ComponentFactory<CheckPointReachedFlyOutComponent>
{
    protected override CheckPointReachedFlyOutComponent CreateComponent(Entity entity) => new(entity);
}