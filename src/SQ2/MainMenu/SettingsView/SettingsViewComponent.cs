using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using Geisha.Engine.Rendering.Systems;
using Geisha.Engine.Windowing;
using SQ2.Core;

namespace SQ2.MainMenu.SettingsView;

internal sealed class SettingsViewComponent : BehaviorComponent
{
    private const string ActionBackToMainView = "BackToMainView";
    private const string ActionToggleOption = "ToggleOption";
    private const string ActionNavigateUp = "NavigateUp";
    private const string ActionNavigateDown = "NavigateDown";

    private readonly IWindowingSystem _windowingSystem;
    private readonly IRenderingSystem _renderingSystem;

    private InputComponent _inputComponent = null!;

    private TextRendererComponent _displayModeText = null!;
    private TextRendererComponent _vsyncText = null!;
    private TextRendererComponent _tripleBufferingText = null!;

    private readonly Color _activeColor = Color.White;
    private readonly Color _inactiveColor = Color.Gray;

    private int _selectedOption = 0;

    public SettingsViewComponent(Entity entity, IWindowingSystem windowingSystem, IRenderingSystem renderingSystem) : base(entity)
    {
        _windowingSystem = windowingSystem;
        _renderingSystem = renderingSystem;
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = InputMapping.CreateBuilder()
            .MapAction(ActionBackToMainView, Key.Escape)
            .MapAction(ActionToggleOption, Key.Enter)
            .MapAction(ActionNavigateUp, Key.Up)
            .MapAction(ActionNavigateDown, Key.Down)
            .Build();

        _inputComponent.BindAction(ActionBackToMainView, OnAction_NavigateBackToMainView);
        _inputComponent.BindAction(ActionToggleOption, OnAction_ToggleOption);
        _inputComponent.BindAction(ActionNavigateUp, OnAction_NavigateUp);
        _inputComponent.BindAction(ActionNavigateDown, OnAction_NavigateDown);

        _inputComponent.Enabled = false; // Transition component activates view.

        var containerEntity = Entity.CreateChildEntity();
        containerEntity.CreateComponent<Transform2DComponent>();
        var containerRenderer = containerEntity.CreateComponent<RectangleRendererComponent>();
        containerRenderer.SortingLayerName = GlobalSettings.SortingLayers.Menu;
        containerRenderer.Color = Color.FromArgb(192, 0, 0, 0);
        containerRenderer.Dimensions = new Vector2(220, 100);
        containerRenderer.FillInterior = true;

        _displayModeText = CreateOptionLabel(containerEntity, new Vector2(0, 40));
        _vsyncText = CreateOptionLabel(containerEntity, new Vector2(0, 20));
        _tripleBufferingText = CreateOptionLabel(containerEntity, new Vector2(0, 0));
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
        switch (_selectedOption)
        {
            case 0:
                SettingsService.ToggleDisplayMode(_windowingSystem);
                break;
            case 1:
                _renderingSystem.VSyncEnabled = !_renderingSystem.VSyncEnabled;
                break;
            case 2:
                _renderingSystem.BufferingMode = _renderingSystem.BufferingMode switch
                {
                    BufferingMode.DoubleBuffering => BufferingMode.TripleBuffering,
                    BufferingMode.TripleBuffering => BufferingMode.DoubleBuffering,
                    _ => throw new InvalidOperationException("Unsupported buffering mode.")
                };
                break;
            default:
                throw new InvalidOperationException("Unhandled option.");
        }

        SaveSettings();
        RefreshOptions();
    }

    private void OnAction_NavigateUp()
    {
        _selectedOption = (_selectedOption - 1 + 3) % 3;
        RefreshOptions();
    }

    private void OnAction_NavigateDown()
    {
        _selectedOption = (_selectedOption + 1) % 3;
        RefreshOptions();
    }

    private void SaveSettings()
    {
        var settings = new Settings
        {
            DisplayMode = _windowingSystem.DisplayMode,
            VSyncEnabled = _renderingSystem.VSyncEnabled,
            BufferingMode = _renderingSystem.BufferingMode
        };
        SettingsService.SaveSettings(settings);
    }

    private void RefreshOptions()
    {
        var vsyncOnOff = _renderingSystem.VSyncEnabled ? "ON" : "OFF";
        var tripleBufferingOnOff = _renderingSystem.BufferingMode is BufferingMode.TripleBuffering ? "ON" : "OFF";

        _displayModeText.Text =
            $"Display Mode:       {_windowingSystem.DisplayMode}";
        _vsyncText.Text =
            $"VSync:              {vsyncOnOff}";
        _tripleBufferingText.Text =
            $"Triple Buffering:   {tripleBufferingOnOff}";

        switch (_selectedOption)
        {
            case 0:
                _displayModeText.Color = _activeColor;
                _vsyncText.Color = _inactiveColor;
                _tripleBufferingText.Color = _inactiveColor;
                break;
            case 1:
                _displayModeText.Color = _inactiveColor;
                _vsyncText.Color = _activeColor;
                _tripleBufferingText.Color = _inactiveColor;
                break;
            case 2:
                _displayModeText.Color = _inactiveColor;
                _vsyncText.Color = _inactiveColor;
                _tripleBufferingText.Color = _activeColor;
                break;
            default:
                throw new InvalidOperationException("Unhandled option.");
        }
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
    private readonly IRenderingSystem _renderingSystem;

    public SettingsViewComponentFactory(IWindowingSystem windowingSystem, IRenderingSystem renderingSystem)
    {
        _windowingSystem = windowingSystem;
        _renderingSystem = renderingSystem;
    }

    protected override SettingsViewComponent CreateComponent(Entity entity) => new(entity, _windowingSystem, _renderingSystem);
}