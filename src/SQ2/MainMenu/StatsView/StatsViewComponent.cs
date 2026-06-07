using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;

namespace SQ2.MainMenu.StatsView;

internal sealed class StatsViewComponent : BehaviorComponent
{
    private const string ActionBackToMainView = "BackToMainView";
    private InputComponent _inputComponent = null!;

    private readonly GameStateService _gameStateService;

    public StatsViewComponent(Entity entity, GameStateService gameStateService) : base(entity)
    {
        _gameStateService = gameStateService;
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = InputMapping.CreateBuilder()
            .MapAction(ActionBackToMainView, Key.Escape)
            .Build();

        _inputComponent.BindAction(ActionBackToMainView, OnAction_NavigateBackToMainView);

        _inputComponent.Enabled = false; // Transition component activates view.

        var c = Entity.CreateChildEntity();
        c.CreateComponent<Transform2DComponent>();
        var r = c.CreateComponent<RectangleRendererComponent>();
        r.Color = Color.Red;
        r.Dimensions = new Vector2(100, 100);
        r.FillInterior = true;
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
internal sealed class StatsViewComponentFactory : ComponentFactory<StatsViewComponent>
{
    private readonly GameStateService _gameStateService;

    public StatsViewComponentFactory(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    protected override StatsViewComponent CreateComponent(Entity entity) => new(entity, _gameStateService);
}