using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.MainMenu.MainView;

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

            var mainViewEntity = Scene.CreateEntity();
            mainViewEntity.CreateComponent<Transform2DComponent>();
            mainViewEntity.CreateComponent<MainViewComponent>();
        }
    }
}