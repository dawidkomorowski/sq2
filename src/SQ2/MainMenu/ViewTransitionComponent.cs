using System;
using System.Diagnostics;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.MainMenu.MainView;
using SQ2.MainMenu.SelectLevelView;

namespace SQ2.MainMenu;

internal sealed class ViewTransitionComponent : BehaviorComponent
{
    public enum View
    {
        MainView,
        SelectLevelView
    }

    private readonly Vector2 _viewCenter = Vector2.Zero;
    private readonly Vector2 _mainViewOutside = new(-200, 0);
    private readonly Vector2 _selectLevelViewOutside = new(200, 0);

    public ViewTransitionComponent(Entity entity) : base(entity)
    {
    }

    public MainViewComponent? MainViewComponent { get; set; }
    public SelectLevelViewComponent? SelectLevelViewComponent { get; set; }

    public View CurrentView { get; set; } = View.MainView;

    public override void OnUpdate(in TimeStep timeStep)
    {
        Debug.Assert(MainViewComponent != null, nameof(MainViewComponent) + " is null");
        Debug.Assert(SelectLevelViewComponent != null, nameof(SelectLevelViewComponent) + " is null");

        var mainViewTransform = MainViewComponent.Entity.GetComponent<Transform2DComponent>();
        var selectLevelViewTransform = SelectLevelViewComponent.Entity.GetComponent<Transform2DComponent>();

        switch (CurrentView)
        {
            case View.MainView:
                mainViewTransform.Translation += (_viewCenter - mainViewTransform.Translation) * timeStep.DeltaTimeSeconds;
                selectLevelViewTransform.Translation = _selectLevelViewOutside;

                MainViewComponent.OnView_Activated();
                break;
            case View.SelectLevelView:
                mainViewTransform.Translation = _mainViewOutside;
                selectLevelViewTransform.Translation = _viewCenter;

                SelectLevelViewComponent.OnView_Activated();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ViewTransitionComponentFactory : ComponentFactory<ViewTransitionComponent>
{
    protected override ViewTransitionComponent CreateComponent(Entity entity) => new(entity);
}