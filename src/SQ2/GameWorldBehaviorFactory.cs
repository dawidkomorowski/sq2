using System.IO;
using Geisha.Engine.Core.SceneModel;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Common;

namespace SQ2;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameWorldBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = "GameWorld";
    private readonly EntityFactory _entityFactory;
    private readonly MapLoader _mapLoader;
    private readonly RespawnService _respawnService;
    private readonly ProximityActivationService _proximityActivationService;

    public GameWorldBehaviorFactory(EntityFactory entityFactory, MapLoader mapLoader, RespawnService respawnService,
        ProximityActivationService proximityActivationService)
    {
        _entityFactory = entityFactory;
        _mapLoader = mapLoader;
        _respawnService = respawnService;
        _proximityActivationService = proximityActivationService;
    }

    public string BehaviorName => SceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new GameWorldSceneBehavior(scene, _entityFactory, _mapLoader, _respawnService, _proximityActivationService);

    private sealed class GameWorldSceneBehavior : SceneBehavior
    {
        private readonly EntityFactory _entityFactory;
        private readonly MapLoader _mapLoader;
        private readonly RespawnService _respawnService;
        private readonly ProximityActivationService _proximityActivationService;

        public GameWorldSceneBehavior
        (
            Scene scene,
            EntityFactory entityFactory,
            MapLoader mapLoader,
            RespawnService respawnService,
            ProximityActivationService proximityActivationService
        ) : base(scene)
        {
            _entityFactory = entityFactory;
            _mapLoader = mapLoader;
            _respawnService = respawnService;
            _proximityActivationService = proximityActivationService;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _proximityActivationService.Reset();
            _respawnService.Reset();

            _entityFactory.CreateDevControls(Scene);
            _entityFactory.CreateCamera(Scene);

            var tmxPath = DevConfig.MapFile ?? Path.Combine("Assets", "Maps", "level_05.tmx");
            _mapLoader.LoadMap(Scene, tmxPath);
        }
    }
}