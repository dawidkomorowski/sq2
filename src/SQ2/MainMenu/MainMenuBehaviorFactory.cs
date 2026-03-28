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
    private readonly GameSaveService _gameSaveService;

    public MainMenuBehaviorFactory(GameSaveService gameSaveService)
    {
        _gameSaveService = gameSaveService;
    }

    public SceneBehavior Create(Scene scene) => new MainMenuSceneBehavior(scene, _gameSaveService);

    public string BehaviorName => SceneBehaviorName;

    private sealed class MainMenuSceneBehavior : SceneBehavior
    {
        private readonly GameSaveService _gameSaveService;

        public MainMenuSceneBehavior(Scene scene, GameSaveService gameSaveService) : base(scene)
        {
            _gameSaveService = gameSaveService;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _gameSaveService.LoadGame();
            _gameSaveService.SaveGame();

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