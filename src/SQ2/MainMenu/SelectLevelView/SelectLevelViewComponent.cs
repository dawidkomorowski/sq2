using System;
using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using SQ2.Core;

namespace SQ2.MainMenu.SelectLevelView;

internal sealed class SelectLevelViewComponent : BehaviorComponent
{
    private const string ActionBackToMainView = "BackToMainView";
    private const string ActionNextLevel = "NextLevel";
    private const string ActionPreviousLevel = "PreviousLevel";

    private InputComponent _inputComponent = null!;
    private readonly List<Transform2DComponent> _levelPreviewTransforms = new();

    private int _lastSelectedLevel;
    private int _selectedLevel;
    private readonly TimeSpan _transitionTime = TimeSpan.FromMilliseconds(200);
    private TimeSpan _transitionTimer;
    private bool _transitionCompleted;

    public SelectLevelViewComponent(Entity entity) : base(entity)
    {
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = InputMapping.CreateBuilder()
            .MapAction(ActionBackToMainView, Key.Escape)
            .MapAction(ActionNextLevel, Key.Right)
            .MapAction(ActionPreviousLevel, Key.Left)
            .Build();

        _inputComponent.BindAction(ActionBackToMainView, OnAction_NavigateBackToMainView);
        _inputComponent.BindAction(ActionNextLevel, OnAction_NextLevel);
        _inputComponent.BindAction(ActionPreviousLevel, OnAction_PreviousLevel);

        _inputComponent.Enabled = false; // Transition component activates view.

        foreach (var levelInfo in LevelInfo.Levels)
        {
            var levelPreviewEntity = Entity.CreateChildEntity();
            var levelPreviewTransform = levelPreviewEntity.CreateComponent<Transform2DComponent>();
            var levelPreviewComponent = levelPreviewEntity.CreateComponent<LevelPreviewComponent>();
            levelPreviewComponent.LevelInfo = levelInfo;

            _levelPreviewTransforms.Add(levelPreviewTransform);
        }
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        if (_transitionCompleted) return;

        _transitionTimer += timeStep.DeltaTime;
        if (_transitionTimer > _transitionTime)
        {
            _transitionTimer = _transitionTime;
            _transitionCompleted = true;
        }

        var alpha = Ease.InOutSine(_transitionTimer / _transitionTime);

        for (var i = 0; i < _levelPreviewTransforms.Count; i++)
        {
            var oldPosition = new Vector2((i - _lastSelectedLevel) * 300, 0);
            var newPosition = new Vector2((i - _selectedLevel) * 300, 0);
            _levelPreviewTransforms[i].Translation = Vector2.Lerp(oldPosition, newPosition, alpha);
        }
    }

    public void OnView_Activated()
    {
        _inputComponent.Enabled = true;
    }

    private void OnAction_NavigateBackToMainView()
    {
        if (!_transitionCompleted) return;

        _inputComponent.Enabled = false;
        ViewTransitionComponent?.ChangeView(ViewTransitionComponent.View.MainView);
    }

    private void OnAction_NextLevel()
    {
        if (!_transitionCompleted) return;

        _lastSelectedLevel = _selectedLevel;
        _selectedLevel = Math.Min(_selectedLevel + 1, LevelInfo.Levels.Length - 1);
        _transitionTimer = TimeSpan.Zero;
        _transitionCompleted = false;
    }

    private void OnAction_PreviousLevel()
    {
        if (!_transitionCompleted) return;

        _lastSelectedLevel = _selectedLevel;
        _selectedLevel = Math.Max(_selectedLevel - 1, 0);
        _transitionTimer = TimeSpan.Zero;
        _transitionCompleted = false;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class SelectLevelViewComponentFactory : ComponentFactory<SelectLevelViewComponent>
{
    protected override SelectLevelViewComponent CreateComponent(Entity entity) => new(entity);
}