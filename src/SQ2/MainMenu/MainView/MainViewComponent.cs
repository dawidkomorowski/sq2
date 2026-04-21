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
    private const string MenuItemExitId = "Exit";
    private readonly List<Entity> _menuItems = new();
    private Entity _selectedMenuItem = null!;

    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;
    private readonly GameSaveService _gameSaveService;

    public MainViewComponent(Entity entity, IEngineManager engineManager, ISceneManager sceneManager, GameSaveService gameSaveService) : base(entity)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _gameSaveService = gameSaveService;
    }

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

        _inputComponent.BindAction(ActionNavigateUp, OnActionNavigateUp);
        _inputComponent.BindAction(ActionNavigateDown, OnActionNavigateDown);
        _inputComponent.BindAction(ActionSelect, OnActionSelect);

        CreateBackground();

        const double menuStartY = 20;
        const double menuItemSpacing = 20;
        var menuItemsCount = 0;

        _menuItems.Add(CreateMenuItem(MenuItemNewGameId, "New Game", new Vector2(0, menuStartY - menuItemSpacing * menuItemsCount++)));

        if (_gameSaveService.GameSave.NewGameStarted)
        {
            _menuItems.Add(CreateMenuItem(MenuItemContinueId, "Continue", new Vector2(0, menuStartY - menuItemSpacing * menuItemsCount++)));
        }

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

    private void OnActionNavigateUp()
    {
        var currentIndex = _menuItems.IndexOf(_selectedMenuItem);
        var previousIndex = (currentIndex - 1 + _menuItems.Count) % _menuItems.Count;
        _selectedMenuItem = _menuItems[previousIndex];
    }

    private void OnActionNavigateDown()
    {
        var currentIndex = _menuItems.IndexOf(_selectedMenuItem);
        var nextIndex = (currentIndex + 1) % _menuItems.Count;
        _selectedMenuItem = _menuItems[nextIndex];
    }

    private void OnActionSelect()
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
                    _gameSaveService.GameSave.NewGameStarted = true;
                    _gameSaveService.SaveGame();
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
                fadeOutComponent.OnComplete = () => { _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld); };
                break;
            }
            case MenuItemExitId:
            {
                _engineManager.ScheduleEngineShutdown();
                break;
            }
        }
    }

    private void CreateBackground()
    {
        var backgroundEntity = Entity.CreateChildEntity();
        backgroundEntity.CreateComponent<Transform2DComponent>();
        var rectangleRendererComponent = backgroundEntity.CreateComponent<RectangleRendererComponent>();
        rectangleRendererComponent.Dimensions = GlobalSettings.ViewSize * 2;
        rectangleRendererComponent.Color = Color.Black;
        rectangleRendererComponent.FillInterior = true;
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
    private readonly GameSaveService _gameSaveService;

    public MainViewComponentFactory(IEngineManager engineManager, ISceneManager sceneManager, GameSaveService gameSaveService)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _gameSaveService = gameSaveService;
    }

    protected override MainViewComponent CreateComponent(Entity entity) => new(entity, _engineManager, _sceneManager, _gameSaveService);
}