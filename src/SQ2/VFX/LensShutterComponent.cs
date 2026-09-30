using System;
using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;

namespace SQ2.VFX;

internal sealed class LensShutterComponent : BehaviorComponent
{
    private Transform2DComponent _transform = null!;
    private readonly List<Blade> _blades = new();
    private TimeSpan _timer;
    private readonly TimeSpan _duration = TimeSpan.FromSeconds(3);
    private bool _completed;

    public LensShutterComponent(Entity entity) : base(entity)
    {
    }

    public Action? OnComplete { get; set; }

    public override void OnStart()
    {
        _transform = Entity.CreateComponent<Transform2DComponent>();

        CreateBlade(new Vector2(1000, 0));
        CreateBlade(new Vector2(-1000, 0));
        CreateBlade(new Vector2(0, 1000));
        CreateBlade(new Vector2(0, -1000));
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        if (_completed)
        {
            return;
        }

        _timer += timeStep.UnscaledDeltaTime;

        if (_timer > _duration)
        {
            _timer = _duration;
            _completed = true;
        }

        var alpha = _timer / _duration;
        alpha = Ease.InSine(alpha);

        foreach (var blade in _blades)
        {
            blade.Transform2DComponent.Translation = Vector2.Lerp(blade.InitialPosition, blade.TargetPosition, alpha);
        }

        _transform.Rotation = alpha * Math.PI * 2;

        if (_completed)
        {
            OnComplete?.Invoke();
        }
    }

    public void Open()
    {
    }

    public void Close()
    {
    }

    private void CreateBlade(Vector2 position)
    {
        var entity = Entity.CreateChildEntity();

        var transform = entity.CreateComponent<Transform2DComponent>();
        transform.Translation = position;

        var renderer = entity.CreateComponent<RectangleRendererComponent>();
        renderer.SortingLayerName = GlobalSettings.SortingLayers.CameraEffects;
        renderer.Color = Color.Black;
        renderer.FillInterior = true;
        renderer.Dimensions = new Vector2(1000, 1000);

        var blade = new Blade
        {
            InitialPosition = position,
            TargetPosition = Vector2.Zero,
            Transform2DComponent = transform,
            RectangleRendererComponent = renderer
        };

        _blades.Add(blade);
    }

    private record struct Blade
    {
        public Vector2 InitialPosition { get; set; }
        public Vector2 TargetPosition { get; set; }
        public Transform2DComponent Transform2DComponent { get; set; }
        public RectangleRendererComponent RectangleRendererComponent { get; set; }
    }
}

internal sealed class LensShutterComponentFactory : ComponentFactory<LensShutterComponent>
{
    protected override LensShutterComponent CreateComponent(Entity entity) => new(entity);
}