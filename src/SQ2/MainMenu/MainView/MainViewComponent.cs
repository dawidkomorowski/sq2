using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.MainMenu.MainView;

internal sealed class MainViewComponent : BehaviorComponent
{
    private const string ActionNavigateUp = "NavigateUp";
    private const string ActionNavigateDown = "NavigateDown";
    private const string ActionSelect = "Select";
    private InputComponent _inputComponent = null!;

    private const string MenuItemStartGameId = "StartGame";
    private const string MenuItemExitId = "Exit";
    private readonly List<Entity> _menuItems = new();
    private Entity _selectedMenuItem = null!;

    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;

    public MainViewComponent(Entity entity, IEngineManager engineManager, ISceneManager sceneManager) : base(entity)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
    }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = new InputMapping
        {
            ActionMappings =
            {
                new ActionMapping
                {
                    ActionName = ActionNavigateUp,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Up)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = ActionNavigateDown,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Down)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = ActionSelect,
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Enter)
                        }
                    }
                }
            }
        };

        _inputComponent.BindAction(ActionNavigateUp, OnActionNavigateUp);
        _inputComponent.BindAction(ActionNavigateDown, OnActionNavigateDown);
        _inputComponent.BindAction(ActionSelect, OnActionSelect);

        CreateBackground();

        _menuItems.Add(CreateMenuItem(MenuItemStartGameId, "Start Game", new Vector2(0, 10)));
        _menuItems.Add(CreateMenuItem(MenuItemExitId, "Exit", new Vector2(0, -10)));

        _selectedMenuItem = _menuItems[0];
    }

    public override void OnUpdate(GameTime gameTime)
    {
        foreach (var menuItem in _menuItems)
        {
            if (menuItem == _selectedMenuItem)
            {
                menuItem.GetComponent<TextRendererComponent>().Color = Color.White;
            }
            else
            {
                menuItem.GetComponent<TextRendererComponent>().Color = Color.FromArgb(255, 128, 128, 128);
            }
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
            case MenuItemStartGameId:
                _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld);
                break;
            case MenuItemExitId:
                _engineManager.ScheduleEngineShutdown();
                break;
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
        return menuItemEntity;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MainViewComponentFactory : ComponentFactory<MainViewComponent>
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;

    public MainViewComponentFactory(IEngineManager engineManager, ISceneManager sceneManager)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
    }

    protected override MainViewComponent CreateComponent(Entity entity) => new(entity, _engineManager, _sceneManager);
}