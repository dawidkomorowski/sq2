using System;
using System.Diagnostics;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.Core;
using SQ2.MainMenu.MainView;
using SQ2.MainMenu.SelectLevelView;
using SQ2.MainMenu.StatsView;

namespace SQ2.MainMenu;

internal sealed class ViewTransitionComponent : BehaviorComponent
{
    public enum View
    {
        MainView,
        SelectLevelView,
        StatsView
    }

    private readonly Vector2 _viewCenter = Vector2.Zero;
    private readonly Vector2 _mainViewOutside = new(0, 400);
    private readonly Vector2 _otherViewOutside = new(0, -400);

    private View _previousView = View.MainView;
    private View _currentView = View.MainView;
    private readonly TimeSpan _transitionTime = TimeSpan.FromMilliseconds(200);
    private TimeSpan _transitionTimer;
    private bool _transitionCompleted;

    public ViewTransitionComponent(Entity entity) : base(entity)
    {
        _transitionTimer = _transitionTime;
    }

    public MainViewComponent? MainViewComponent { get; set; }
    public SelectLevelViewComponent? SelectLevelViewComponent { get; set; }
    public StatsViewComponent? StatsViewComponent { get; set; }

    public void ChangeView(View view)
    {
        if (_currentView is View.MainView && view is View.MainView)
        {
            throw new ArgumentException("Cannot change from main view to main view.");
        }

        if (_currentView is not View.MainView && view is not View.MainView)
        {
            throw new ArgumentException("Non main view must always change to main view.");
        }

        _previousView = _currentView;
        _currentView = view;
        _transitionTimer = TimeSpan.Zero;
        _transitionCompleted = false;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        Debug.Assert(MainViewComponent != null, nameof(MainViewComponent) + " is null");
        Debug.Assert(SelectLevelViewComponent != null, nameof(SelectLevelViewComponent) + " is null");
        Debug.Assert(StatsViewComponent != null, nameof(StatsViewComponent) + " != null");

        if (_transitionCompleted) return;

        _transitionTimer += timeStep.DeltaTime;
        if (_transitionTimer > _transitionTime)
        {
            _transitionTimer = _transitionTime;
            _transitionCompleted = true;
        }

        var alpha = Ease.InOutSine(_transitionTimer / _transitionTime);

        var mainViewTransform = MainViewComponent.Entity.GetComponent<Transform2DComponent>();
        var selectLevelViewTransform = SelectLevelViewComponent.Entity.GetComponent<Transform2DComponent>();
        var statsViewTransform = StatsViewComponent.Entity.GetComponent<Transform2DComponent>();

        // Initial layout.
        mainViewTransform.Translation = _viewCenter;
        selectLevelViewTransform.Translation = _otherViewOutside;
        statsViewTransform.Translation = _otherViewOutside;

        Transform2DComponent otherViewTransform;

        if (_currentView is View.MainView)
        {
            otherViewTransform = _previousView switch
            {
                View.SelectLevelView => selectLevelViewTransform,
                View.StatsView => statsViewTransform,
                View.MainView => mainViewTransform,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            otherViewTransform = _currentView switch
            {
                View.SelectLevelView => selectLevelViewTransform,
                View.StatsView => statsViewTransform,
                View.MainView => mainViewTransform,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        if (_previousView is View.MainView && _currentView is not View.MainView)
        {
            mainViewTransform.Translation = Vector2.Lerp(_viewCenter, _mainViewOutside, alpha);
            otherViewTransform.Translation = Vector2.Lerp(_otherViewOutside, _viewCenter, alpha);
        }
        else if (_previousView is not View.MainView && _currentView is View.MainView)
        {
            mainViewTransform.Translation = Vector2.Lerp(_mainViewOutside, _viewCenter, alpha);
            otherViewTransform.Translation = Vector2.Lerp(_viewCenter, _otherViewOutside, alpha);
        }

        if (_transitionCompleted)
        {
            switch (_currentView)
            {
                case View.MainView:
                    MainViewComponent.OnView_Activated();
                    break;
                case View.SelectLevelView:
                    SelectLevelViewComponent.OnView_Activated();
                    break;
                case View.StatsView:
                    StatsViewComponent.OnView_Activated();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ViewTransitionComponentFactory : ComponentFactory<ViewTransitionComponent>
{
    protected override ViewTransitionComponent CreateComponent(Entity entity) => new(entity);
}