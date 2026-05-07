using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Systems;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.PauseMenu;

namespace SQ2.GamePlay;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameWorldBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = GlobalSettings.SceneNames.GameWorld;
    private readonly EntityFactory _entityFactory;
    private readonly MapLoader _mapLoader;
    private readonly RespawnService _respawnService;
    private readonly ProximityActivationService _proximityActivationService;
    private readonly GameStateService _gameStateService;
    private readonly IPhysicsSystem _physicsSystem;

    public GameWorldBehaviorFactory
    (
        EntityFactory entityFactory,
        MapLoader mapLoader,
        RespawnService respawnService,
        ProximityActivationService proximityActivationService,
        GameStateService gameStateService,
        IPhysicsSystem physicsSystem
    )
    {
        _entityFactory = entityFactory;
        _mapLoader = mapLoader;
        _respawnService = respawnService;
        _proximityActivationService = proximityActivationService;
        _gameStateService = gameStateService;
        _physicsSystem = physicsSystem;
    }

    public string BehaviorName => SceneBehaviorName;

    public SceneBehavior Create(Scene scene) =>
        new GameWorldSceneBehavior(scene, _entityFactory, _mapLoader, _respawnService, _proximityActivationService, _gameStateService, _physicsSystem);

    private sealed class GameWorldSceneBehavior : SceneBehavior
    {
        private readonly EntityFactory _entityFactory;
        private readonly MapLoader _mapLoader;
        private readonly RespawnService _respawnService;
        private readonly ProximityActivationService _proximityActivationService;
        private readonly GameStateService _gameStateService;
        private readonly IPhysicsSystem _physicsSystem;

        public GameWorldSceneBehavior
        (
            Scene scene,
            EntityFactory entityFactory,
            MapLoader mapLoader,
            RespawnService respawnService,
            ProximityActivationService proximityActivationService,
            GameStateService gameStateService,
            IPhysicsSystem physicsSystem
        ) : base(scene)
        {
            _entityFactory = entityFactory;
            _mapLoader = mapLoader;
            _respawnService = respawnService;
            _proximityActivationService = proximityActivationService;
            _gameStateService = gameStateService;
            _physicsSystem = physicsSystem;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _gameStateService.InitializeGameSave();

            _proximityActivationService.Reset();
            _respawnService.Reset();

            _entityFactory.CreateDevControls(Scene);
            var camera = _entityFactory.CreateCamera(Scene);

            var uiRoot = Scene.CreateEntity();
            uiRoot.Name = GlobalSettings.SpecialEntities.UIRoot;
            uiRoot.Parent = camera;

            _entityFactory.CreateUI_CoinCounter(uiRoot, 160, 110);

            var pauseMenu = uiRoot.CreateChildEntity();
            pauseMenu.CreateComponent<PauseMenuComponent>();

            var tmxPath = DevConfig.MapFile ?? _gameStateService.GetMapFile();
            _mapLoader.LoadMap(Scene, tmxPath);

            _physicsSystem.SynchronizePhysicsState();
        }
    }
}