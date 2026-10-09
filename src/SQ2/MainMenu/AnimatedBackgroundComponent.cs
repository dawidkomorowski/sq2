using System;
using System.Linq;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using SQ2.VFX;

namespace SQ2.MainMenu;

internal sealed class AnimatedBackgroundComponent : BehaviorComponent
{
    private Transform2DComponent _startTransform = null!;
    private Transform2DComponent _endTransform = null!;
    private Transform2DComponent _transform = null!;
    private bool _waitingForTransitionToComplete;

    public AnimatedBackgroundComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        var cameraPoints = Scene.RootEntities.Where(e => e.HasComponent<MenuCameraPointComponent>()).ToArray();
        if (cameraPoints.Length != 2)
        {
            throw new InvalidOperationException("Invalid number of camera points.");
        }

        var point1 = cameraPoints[0];
        var point2 = cameraPoints[1];

        if (point1.GetComponent<MenuCameraPointComponent>().IsStart == point2.GetComponent<MenuCameraPointComponent>().IsStart)
        {
            throw new InvalidOperationException("Missing camera start/end point.");
        }

        if (point1.GetComponent<MenuCameraPointComponent>().IsStart)
        {
            _startTransform = point1.GetComponent<Transform2DComponent>();
            _endTransform = point2.GetComponent<Transform2DComponent>();
        }
        else
        {
            _startTransform = point2.GetComponent<Transform2DComponent>();
            _endTransform = point1.GetComponent<Transform2DComponent>();
        }

        _transform = Entity.GetComponent<Transform2DComponent>();
        _transform.Translation = _startTransform.Translation;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        const double cameraSpeed = 25;

        var startEndTranslation = _endTransform.Translation - _startTransform.Translation;
        var direction = startEndTranslation.Unit;

        _transform.Translation += direction * cameraSpeed * timeStep.UnscaledDeltaTimeSeconds;

        if (_transform.Translation.Distance(_startTransform.Translation) > startEndTranslation.Length)
        {
            if (!_waitingForTransitionToComplete)
            {
                _waitingForTransitionToComplete = true;

                var fadeOutEntity = Entity.CreateChildEntity();
                var fadeOutComponent = fadeOutEntity.CreateComponent<FadeInOutComponent>();
                fadeOutComponent.SortingLayerName = GlobalSettings.SortingLayers.MenuAnimatedBackground;

                fadeOutComponent.OnComplete = () =>
                {
                    _transform.Translation = _startTransform.Translation;
                    fadeOutEntity.RemoveAfterFullFrame();
                    _waitingForTransitionToComplete = false;

                    var fadeInEntity = Entity.CreateChildEntity();
                    var fadeInComponent = fadeInEntity.CreateComponent<FadeInOutComponent>();
                    fadeInComponent.Mode = FadeInOutComponent.FadeMode.In;
                    fadeInComponent.SortingLayerName = GlobalSettings.SortingLayers.MenuAnimatedBackground;
                    fadeInComponent.OnComplete = fadeInEntity.RemoveAfterFullFrame;
                };
            }
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class AnimatedBackgroundComponentFactory : ComponentFactory<AnimatedBackgroundComponent>
{
    protected override AnimatedBackgroundComponent CreateComponent(Entity entity) => new(entity);
}