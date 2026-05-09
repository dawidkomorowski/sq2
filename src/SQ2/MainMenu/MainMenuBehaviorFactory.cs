using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.MainMenu.MainView;
using SQ2.MainMenu.SelectLevelView;

namespace SQ2.MainMenu;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MainMenuBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = GlobalSettings.SceneNames.MainMenu;
    private readonly GameStateService _gameStateService;

    public MainMenuBehaviorFactory(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    public SceneBehavior Create(Scene scene) => new MainMenuSceneBehavior(scene, _gameStateService);

    public string BehaviorName => SceneBehaviorName;

    private sealed class MainMenuSceneBehavior : SceneBehavior
    {
        private readonly GameStateService _gameStateService;

        public MainMenuSceneBehavior(Scene scene, GameStateService gameStateService) : base(scene)
        {
            _gameStateService = gameStateService;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _gameStateService.InitializeGameSave();

            var cameraEntity = Scene.CreateEntity();
            cameraEntity.CreateComponent<Transform2DComponent>();
            var cameraComponent = cameraEntity.CreateComponent<CameraComponent>();
            cameraComponent.ViewRectangle = GlobalSettings.ViewSize;

            CreateBackground();

            var mainViewEntity = Scene.CreateEntity();
            mainViewEntity.CreateComponent<Transform2DComponent>();
            var mainViewComponent = mainViewEntity.CreateComponent<MainViewComponent>();

            var selectLevelViewEntity = Scene.CreateEntity();
            selectLevelViewEntity.CreateComponent<Transform2DComponent>();
            var selectLevelViewComponent = selectLevelViewEntity.CreateComponent<SelectLevelViewComponent>();

            var viewTransitionEntity = Scene.CreateEntity();
            var viewTransitionComponent = viewTransitionEntity.CreateComponent<ViewTransitionComponent>();
            viewTransitionComponent.MainViewComponent = mainViewComponent;
            viewTransitionComponent.SelectLevelViewComponent = selectLevelViewComponent;

            mainViewComponent.ViewTransitionComponent = viewTransitionComponent;
            selectLevelViewComponent.ViewTransitionComponent = viewTransitionComponent;
        }

        private void CreateBackground()
        {
            var backgroundEntity = Scene.CreateEntity();
            backgroundEntity.CreateComponent<Transform2DComponent>();
            var rectangleRendererComponent = backgroundEntity.CreateComponent<RectangleRendererComponent>();
            rectangleRendererComponent.Dimensions = GlobalSettings.ViewSize * 2;
            rectangleRendererComponent.Color = Color.Black;
            rectangleRendererComponent.FillInterior = true;
        }
    }
}