using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.PauseMenu;

internal sealed class PauseMenuComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private bool _isPaused;

    // Input
    private InputComponent _inputComponent = null!;
    private const string ActionPause = "Pause";
    private const string ActionSelect = "Select";
    private const string ActionNavigateUp = "NavigateUp";

    private const string ActionNavigateDown = "NavigateDown";

    // Services
    private readonly ITimeSystem _timeSystem;
    private readonly IEngineManager _engineManager;

    // Menu
    private sealed record MenuItem(string Text, Action Action)
    {
        public TextRendererComponent? TextRendererComponent { get; set; }
    }

    private readonly List<MenuItem> _menuItems = new();
    private MenuItem _selectedMenuItem = null!;

    // UI
    private readonly List<Renderer2DComponent> _uiElements = new();

    public PauseMenuComponent(Entity entity, ITimeSystem timeSystem, IEngineManager engineManager) : base(entity)
    {
        _timeSystem = timeSystem;
        _engineManager = engineManager;
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.CreateComponent<Transform2DComponent>();
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = new InputMapping
        {
            ActionMappings = ImmutableArray.Create
            (
                new ActionMapping
                {
                    ActionName = ActionPause,
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Escape)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = ActionSelect,
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Enter)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = ActionNavigateUp,
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Up)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = ActionNavigateDown,
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Down)
                        }
                    )
                }
            )
        };

        _inputComponent.BindAction(ActionPause, OnActionPause);
        _inputComponent.BindAction(ActionSelect, OnActionSelect);
        _inputComponent.BindAction(ActionNavigateUp, OnActionNavigateUp);
        _inputComponent.BindAction(ActionNavigateDown, OnActionNavigateDown);

        CreateMenuItems();
        BuildUI();
        SetVisible(false);
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        if (_isPaused)
        {
            foreach (var menuItem in _menuItems)
            {
                Debug.Assert(menuItem.TextRendererComponent != null, "TextRendererComponent should not be null");

                menuItem.TextRendererComponent.Color = menuItem == _selectedMenuItem ? Color.White : Color.FromArgb(255, 192, 192, 192);
            }
        }
    }

    private void CreateMenuItems()
    {
        _menuItems.Add(new MenuItem("Resume", OnActionPause));
        _menuItems.Add(new MenuItem("Exit Game", _engineManager.ScheduleEngineShutdown));

        _selectedMenuItem = _menuItems[0];
    }

    private void OnActionPause()
    {
        _isPaused = !_isPaused;
        _timeSystem.TimeScale = _isPaused ? 0 : 1;

        SetVisible(_isPaused);

        var playerComponent = Query.GetPlayerComponent(Scene);
        if (_isPaused)
        {
            playerComponent.DisableInput();
        }
        else
        {
            playerComponent.EnableInput();
        }
    }

    private void OnActionSelect()
    {
        if (_isPaused)
        {
            _selectedMenuItem.Action.Invoke();
        }
    }

    private void OnActionNavigateUp()
    {
        if (_isPaused)
        {
            var currentIndex = _menuItems.IndexOf(_selectedMenuItem);
            var previousIndex = (currentIndex - 1 + _menuItems.Count) % _menuItems.Count;
            _selectedMenuItem = _menuItems[previousIndex];
        }
    }

    private void OnActionNavigateDown()
    {
        if (_isPaused)
        {
            var currentIndex = _menuItems.IndexOf(_selectedMenuItem);
            var nextIndex = (currentIndex + 1) % _menuItems.Count;
            _selectedMenuItem = _menuItems[nextIndex];
        }
    }

    private void SetVisible(bool visible)
    {
        foreach (var uiElement in _uiElements)
        {
            uiElement.Visible = visible;
        }
    }

    // ReSharper disable once InconsistentNaming
    private void BuildUI()
    {
        var pauseMenuSize = GlobalSettings.ViewSize / 2d;

        var background = Entity.CreateChildEntity();
        background.CreateComponent<Transform2DComponent>();
        var backgroundRenderer = background.CreateComponent<RectangleRendererComponent>();
        backgroundRenderer.SortingLayerName = GlobalSettings.SortingLayers.UI;
        backgroundRenderer.Color = Color.FromArgb(128, 0, 0, 0);
        backgroundRenderer.Dimensions = pauseMenuSize;
        backgroundRenderer.FillInterior = true;
        _uiElements.Add(backgroundRenderer);


        var verticalOffset = 0;
        foreach (var menuItem in _menuItems)
        {
            var textEntity = Entity.CreateChildEntity();
            var textTransform = textEntity.CreateComponent<Transform2DComponent>();
            textTransform.Translation = new Vector2(-pauseMenuSize.X / 2, pauseMenuSize.Y / 2 - verticalOffset);
            var textRenderer = textEntity.CreateComponent<TextRendererComponent>();
            textRenderer.SortingLayerName = GlobalSettings.SortingLayers.UI;
            textRenderer.OrderInLayer = 1;
            textRenderer.Color = Color.White;
            textRenderer.Text = menuItem.Text;
            textRenderer.MaxWidth = pauseMenuSize.X;
            textRenderer.TextAlignment = TextAlignment.Center;
            textRenderer.FontSize = FontSize.FromDips(24);
            _uiElements.Add(textRenderer);
            menuItem.TextRendererComponent = textRenderer;

            verticalOffset += 30; // Adjust this value based on your desired spacing
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PauseMenuComponentFactory : ComponentFactory<PauseMenuComponent>
{
    private readonly ITimeSystem _timeSystem;
    private readonly IEngineManager _engineManager;

    public PauseMenuComponentFactory(ITimeSystem timeSystem, IEngineManager engineManager)
    {
        _timeSystem = timeSystem;
        _engineManager = engineManager;
    }

    protected override PauseMenuComponent CreateComponent(Entity entity) => new(entity, _timeSystem, _engineManager);
}