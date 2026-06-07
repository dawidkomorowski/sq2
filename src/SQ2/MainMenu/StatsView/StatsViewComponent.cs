using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
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

        var containerEntity = Entity.CreateChildEntity();
        containerEntity.CreateComponent<Transform2DComponent>();
        var containerRenderer = containerEntity.CreateComponent<RectangleRendererComponent>();
        containerRenderer.Color = Color.FromArgb(255, 128, 128, 128);
        containerRenderer.Dimensions = new Vector2(220, 100);
        containerRenderer.FillInterior = true;

        CreateStatLabel(containerEntity, new Vector2(0, 40), $"Total Playtime: {FormatPlaytime(_gameStateService.TotalPlaytime)}");
        CreateStatLabel(containerEntity, new Vector2(0, 20), $"Total Deaths:   {_gameStateService.TotalDeaths}");
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

    private static void CreateStatLabel(Entity parent, Vector2 position, string text)
    {
        var entity = parent.CreateChildEntity();
        var transform = entity.CreateComponent<Transform2DComponent>();
        transform.Translation = position;
        var textRenderer = entity.CreateComponent<TextRendererComponent>();
        textRenderer.Text = text;
        textRenderer.Color = Color.White;
        textRenderer.TextAlignment = TextAlignment.Leading;
        textRenderer.MaxWidth = 200;
        textRenderer.Pivot = new Vector2(100, 0);
        textRenderer.FontSize = FontSize.FromDips(12);
    }

    private static string FormatPlaytime(TimeSpan playtime) => $"{(int)playtime.TotalHours}h {playtime.Minutes}min";
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