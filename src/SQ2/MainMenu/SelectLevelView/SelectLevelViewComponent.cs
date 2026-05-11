using System.Collections.Generic;
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
    private const string ActionNavigateBackToMainView = "NavigateUp";
    private InputComponent _inputComponent = null!;
    private readonly List<Transform2DComponent> _levelPreviewTransforms = new();

    public SelectLevelViewComponent(Entity entity) : base(entity)
    {
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = InputMapping.CreateBuilder()
            .MapAction(ActionNavigateBackToMainView, Key.Escape)
            .Build();

        _inputComponent.BindAction(ActionNavigateBackToMainView, OnAction_NavigateBackToMainView);

        _inputComponent.Enabled = false; // Transition component activates view.

        foreach (var levelInfo in LevelInfo.Levels)
        {
            var levelPreviewEntity = Entity.CreateChildEntity();
            var levelPreviewTransform = levelPreviewEntity.CreateComponent<Transform2DComponent>();
            var levelPreviewComponent = levelPreviewEntity.CreateComponent<LevelPreviewComponent>();
            levelPreviewComponent.LevelInfo = levelInfo;

            var index = _levelPreviewTransforms.Count;
            levelPreviewTransform.Translation = new Vector2(index * 300, 0);

            _levelPreviewTransforms.Add(levelPreviewTransform);
        }
    }

    public void OnView_Activated()
    {
        _inputComponent.Enabled = true;
    }

    private void OnAction_NavigateBackToMainView()
    {
        _inputComponent.Enabled = false;
        ViewTransitionComponent?.ChangeView(ViewTransitionComponent.View.MainView);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class SelectLevelViewComponentFactory : ComponentFactory<SelectLevelViewComponent>
{
    protected override SelectLevelViewComponent CreateComponent(Entity entity) => new(entity);
}