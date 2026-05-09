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
    private readonly Vector2 _mainViewOutside = new(-400, 0);
    private readonly Vector2 _selectLevelViewOutside = new(400, 0);

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

    public void ChangeView(View view)
    {
        _currentView = view;
        _transitionTimer = TimeSpan.Zero;
        _transitionCompleted = false;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        Debug.Assert(MainViewComponent != null, nameof(MainViewComponent) + " is null");
        Debug.Assert(SelectLevelViewComponent != null, nameof(SelectLevelViewComponent) + " is null");

        if (_transitionCompleted)
        {
            return;
        }

        _transitionTimer += timeStep.DeltaTime;
        if (_transitionTimer > _transitionTime)
        {
            _transitionTimer = _transitionTime;
            _transitionCompleted = true;
        }

        var alpha = EaseInOutSine(_transitionTimer / _transitionTime);

        var mainViewTransform = MainViewComponent.Entity.GetComponent<Transform2DComponent>();
        var selectLevelViewTransform = SelectLevelViewComponent.Entity.GetComponent<Transform2DComponent>();

        switch (_currentView)
        {
            case View.MainView:
                mainViewTransform.Translation = Vector2.Lerp(_mainViewOutside, _viewCenter, alpha);
                selectLevelViewTransform.Translation = Vector2.Lerp(_viewCenter, _selectLevelViewOutside, alpha);
                break;
            case View.SelectLevelView:
                mainViewTransform.Translation = Vector2.Lerp(_viewCenter, _mainViewOutside, alpha);
                selectLevelViewTransform.Translation = Vector2.Lerp(_selectLevelViewOutside, _viewCenter, alpha);
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static double EaseInOutSine(double x) => -(Math.Cos(Math.PI * x) - 1) / 2;
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ViewTransitionComponentFactory : ComponentFactory<ViewTransitionComponent>
{
    protected override ViewTransitionComponent CreateComponent(Entity entity) => new(entity);
}