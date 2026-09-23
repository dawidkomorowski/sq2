using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using Geisha.Engine.Windowing;
using SQ2.Core;

namespace SQ2.MainMenu.SettingsView;

internal sealed class SettingsViewComponent : BehaviorComponent
{
    private const string ActionBackToMainView = "BackToMainView";
    private const string ActionToggleOption = "ToggleOption";
    private readonly IWindowingSystem _windowingSystem;
    private InputComponent _inputComponent = null!;

    private TextRendererComponent _displayModeText = null!;

    public SettingsViewComponent(Entity entity, IWindowingSystem windowingSystem) : base(entity)
    {
        _windowingSystem = windowingSystem;
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = InputMapping.CreateBuilder()
            .MapAction(ActionBackToMainView, Key.Escape)
            .MapAction(ActionToggleOption, Key.Enter)
            .Build();

        _inputComponent.BindAction(ActionBackToMainView, OnAction_NavigateBackToMainView);
        _inputComponent.BindAction(ActionToggleOption, OnAction_ToggleOption);

        _inputComponent.Enabled = false; // Transition component activates view.

        var containerEntity = Entity.CreateChildEntity();
        containerEntity.CreateComponent<Transform2DComponent>();
        var containerRenderer = containerEntity.CreateComponent<RectangleRendererComponent>();
        containerRenderer.SortingLayerName = GlobalSettings.SortingLayers.Menu;
        containerRenderer.Color = Color.FromArgb(192, 0, 0, 0);
        containerRenderer.Dimensions = new Vector2(220, 100);
        containerRenderer.FillInterior = true;

        _displayModeText = CreateOptionLabel(containerEntity, new Vector2(0, 40));
        RefreshOptions();
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

    private void OnAction_ToggleOption()
    {
        SettingsService.ToggleDisplayMode(_windowingSystem);
        SaveSettings();
        RefreshOptions();
    }

    private void SaveSettings()
    {
        var settings = new Settings
        {
            DisplayMode = _windowingSystem.DisplayMode
        };
        SettingsService.SaveSettings(settings);
    }

    private void RefreshOptions()
    {
        _displayModeText.Text = $"Display Mode: {_windowingSystem.DisplayMode}";
    }

    private static TextRendererComponent CreateOptionLabel(Entity parent, Vector2 position)
    {
        var entity = parent.CreateChildEntity();
        var transform = entity.CreateComponent<Transform2DComponent>();
        transform.Translation = position;
        var textRenderer = entity.CreateComponent<TextRendererComponent>();
        textRenderer.Color = Color.White;
        textRenderer.TextAlignment = TextAlignment.Leading;
        textRenderer.MaxWidth = 200;
        textRenderer.Pivot = new Vector2(100, 0);
        textRenderer.FontSize = FontSize.FromDips(12);
        textRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

        return textRenderer;
    }
}

internal sealed class SettingsViewComponentFactory : ComponentFactory<SettingsViewComponent>
{
    private readonly IWindowingSystem _windowingSystem;

    public SettingsViewComponentFactory(IWindowingSystem windowingSystem)
    {
        _windowingSystem = windowingSystem;
    }

    protected override SettingsViewComponent CreateComponent(Entity entity) => new(entity, _windowingSystem);
}