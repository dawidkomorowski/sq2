using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.VFX;

namespace SQ2.MainMenu.MainView;

internal sealed class MainViewComponent : BehaviorComponent
{
    private const string ActionNavigateUp = "NavigateUp";
    private const string ActionNavigateDown = "NavigateDown";
    private const string ActionSelect = "Select";
    private InputComponent _inputComponent = null!;

    private const string MenuItemNewGameId = "NewGame";
    private const string MenuItemContinueId = "Continue";
    private const string MenuItemSelectLevelId = "SelectLevel";
    private const string MenuItemExitId = "Exit";
    private readonly List<Entity> _menuItems = new();
    private Entity _selectedMenuItem = null!;

    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;
    private readonly GameStateService _gameStateService;

    public MainViewComponent(Entity entity, IEngineManager engineManager, ISceneManager sceneManager, GameStateService gameStateService) : base(entity)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _gameStateService = gameStateService;
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = new InputMapping
        {
            ActionMappings = ImmutableArray.Create
            (
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
                }
            )
        };

        _inputComponent.BindAction(ActionNavigateUp, OnAction_NavigateUp);
        _inputComponent.BindAction(ActionNavigateDown, OnAction_NavigateDown);
        _inputComponent.BindAction(ActionSelect, OnAction_Select);

        _inputComponent.Enabled = false; // Transition component activates view.

        const double menuStartY = 40;
        const double menuItemSpacing = 20;
        var menuItemsCount = 0;

        if (_gameStateService.IsContinueAvailable)
        {
            _menuItems.Add(CreateMenuItem(MenuItemContinueId, "Continue", new Vector2(0, menuStartY - menuItemSpacing * menuItemsCount++)));
        }

        _menuItems.Add(CreateMenuItem(MenuItemNewGameId, "New Game", new Vector2(0, menuStartY - menuItemSpacing * menuItemsCount++)));
        _menuItems.Add(CreateMenuItem(MenuItemSelectLevelId, "Select Level", new Vector2(0, menuStartY - menuItemSpacing * menuItemsCount++)));
        _menuItems.Add(CreateMenuItem(MenuItemExitId, "Exit", new Vector2(0, menuStartY - menuItemSpacing * menuItemsCount)));

        _selectedMenuItem = _menuItems[0];
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        foreach (var menuItem in _menuItems)
        {
            menuItem.GetComponent<TextRendererComponent>().Color = menuItem == _selectedMenuItem ? Color.White : Color.FromArgb(255, 128, 128, 128);
        }
    }

    public void OnView_Activated()
    {
        _inputComponent.Enabled = true;
    }

    private void OnAction_NavigateUp()
    {
        var currentIndex = _menuItems.IndexOf(_selectedMenuItem);
        var previousIndex = (currentIndex - 1 + _menuItems.Count) % _menuItems.Count;
        _selectedMenuItem = _menuItems[previousIndex];
    }

    private void OnAction_NavigateDown()
    {
        var currentIndex = _menuItems.IndexOf(_selectedMenuItem);
        var nextIndex = (currentIndex + 1) % _menuItems.Count;
        _selectedMenuItem = _menuItems[nextIndex];
    }

    private void OnAction_Select()
    {
        switch (_selectedMenuItem.Name)
        {
            case MenuItemNewGameId:
            {
                _inputComponent.Enabled = false;
                var entity = Scene.CreateEntity();
                var fadeOutComponent = entity.CreateComponent<FadeOutComponent>();
                fadeOutComponent.Duration = TimeSpan.FromMilliseconds(300);
                fadeOutComponent.CompleteDelay = TimeSpan.FromMilliseconds(300);
                fadeOutComponent.OnComplete = () =>
                {
                    _gameStateService.StartNewGame();
                    _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld);
                };
                break;
            }
            case MenuItemContinueId:
            {
                _inputComponent.Enabled = false;
                var entity = Scene.CreateEntity();
                var fadeOutComponent = entity.CreateComponent<FadeOutComponent>();
                fadeOutComponent.Duration = TimeSpan.FromMilliseconds(300);
                fadeOutComponent.CompleteDelay = TimeSpan.FromMilliseconds(300);
                fadeOutComponent.OnComplete = () =>
                {
                    _gameStateService.ContinueGame();
                    _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld);
                };
                break;
            }
            case MenuItemSelectLevelId:
            {
                _inputComponent.Enabled = false;
                ViewTransitionComponent?.ChangeView(ViewTransitionComponent.View.SelectLevelView);

                break;
            }
            case MenuItemExitId:
            {
                _engineManager.ScheduleEngineShutdown();
                break;
            }
        }
    }

    private Entity CreateMenuItem(string id, string text, Vector2 position)
    {
        var menuItemEntity = Entity.CreateChildEntity();
        menuItemEntity.Name = id;
        var transform = menuItemEntity.CreateComponent<Transform2DComponent>();
        transform.Translation = position;
        var textRenderer = menuItemEntity.CreateComponent<TextRendererComponent>();
        textRenderer.Text = text;
        textRenderer.Color = Color.White;
        textRenderer.FontSize = FontSize.FromDips(24);
        textRenderer.MaxWidth = 300;
        textRenderer.TextAlignment = TextAlignment.Center;
        textRenderer.Pivot = new Vector2(150, 0);
        return menuItemEntity;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MainViewComponentFactory : ComponentFactory<MainViewComponent>
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;
    private readonly GameStateService _gameStateService;

    public MainViewComponentFactory(IEngineManager engineManager, ISceneManager sceneManager, GameStateService gameStateService)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _gameStateService = gameStateService;
    }

    protected override MainViewComponent CreateComponent(Entity entity) => new(entity, _engineManager, _sceneManager, _gameStateService);
}