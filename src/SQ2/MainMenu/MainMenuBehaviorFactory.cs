using System.IO;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Systems;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.GamePlay.Player;
using SQ2.MainMenu.MainView;
using SQ2.MainMenu.SelectLevelView;
using SQ2.MainMenu.StatsView;

namespace SQ2.MainMenu;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MainMenuBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = GlobalSettings.SceneNames.MainMenu;
    private readonly GameStateService _gameStateService;
    private readonly MapLoader _mapLoader;
    private readonly IPhysicsSystem _physicsSystem;

    public MainMenuBehaviorFactory(GameStateService gameStateService, MapLoader mapLoader, IPhysicsSystem physicsSystem)
    {
        _gameStateService = gameStateService;
        _mapLoader = mapLoader;
        _physicsSystem = physicsSystem;
    }

    public SceneBehavior Create(Scene scene) => new MainMenuSceneBehavior(scene, _gameStateService, _mapLoader, _physicsSystem);

    public string BehaviorName => SceneBehaviorName;

    private sealed class MainMenuSceneBehavior : SceneBehavior
    {
        private readonly GameStateService _gameStateService;
        private readonly MapLoader _mapLoader;
        private readonly IPhysicsSystem _physicsSystem;

        public MainMenuSceneBehavior(Scene scene, GameStateService gameStateService, MapLoader mapLoader, IPhysicsSystem physicsSystem) : base(scene)
        {
            _gameStateService = gameStateService;
            _mapLoader = mapLoader;
            _physicsSystem = physicsSystem;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _gameStateService.InitializeGameSave();

            var cameraEntity = Scene.CreateEntity();
            cameraEntity.CreateComponent<Transform2DComponent>();
            var cameraComponent = cameraEntity.CreateComponent<CameraComponent>();
            cameraComponent.ViewRectangle = GlobalSettings.ViewSize;

            CreateAnimatedBackground(cameraEntity);

            var mainViewEntity = Scene.CreateEntity();
            mainViewEntity.Parent = cameraEntity;
            mainViewEntity.CreateComponent<Transform2DComponent>();
            var mainViewComponent = mainViewEntity.CreateComponent<MainViewComponent>();

            var selectLevelViewEntity = Scene.CreateEntity();
            selectLevelViewEntity.Parent = cameraEntity;
            selectLevelViewEntity.CreateComponent<Transform2DComponent>();
            var selectLevelViewComponent = selectLevelViewEntity.CreateComponent<SelectLevelViewComponent>();

            var statsViewEntity = Scene.CreateEntity();
            statsViewEntity.Parent = cameraEntity;
            statsViewEntity.CreateComponent<Transform2DComponent>();
            var statsViewComponent = statsViewEntity.CreateComponent<StatsViewComponent>();

            var viewTransitionEntity = Scene.CreateEntity();
            var viewTransitionComponent = viewTransitionEntity.CreateComponent<ViewTransitionComponent>();
            viewTransitionComponent.MainViewComponent = mainViewComponent;
            viewTransitionComponent.SelectLevelViewComponent = selectLevelViewComponent;
            viewTransitionComponent.StatsViewComponent = statsViewComponent;

            mainViewComponent.ViewTransitionComponent = viewTransitionComponent;
            selectLevelViewComponent.ViewTransitionComponent = viewTransitionComponent;
            statsViewComponent.ViewTransitionComponent = viewTransitionComponent;
        }

        private void CreateAnimatedBackground(Entity cameraEntity)
        {
            cameraEntity.CreateComponent<AnimatedBackgroundComponent>();

            var menuBackground = Path.Combine("Assets", "Maps", "menu.tmx");
            _mapLoader.LoadMap(Scene, menuBackground);

            foreach (var entity in Scene.AllEntities)
            {
                if (entity.HasComponent<PlayerComponent>())
                {
                    cameraEntity.GetComponent<Transform2DComponent>().Translation = entity.GetComponent<Transform2DComponent>().Translation;
                    entity.RemoveComponent(entity.GetComponent<PlayerComponent>());

                    foreach (var child in entity.Children)
                    {
                        if (child.HasComponent<SpriteRendererComponent>())
                        {
                            child.GetComponent<SpriteRendererComponent>().Visible = false;
                        }
                    }
                }
            }

            _physicsSystem.SynchronizePhysicsState();
        }
    }
}