using System.IO;
using Geisha.Engine.Core.SceneModel;

namespace SQ2;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameWorldBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = "GameWorld";
    private readonly EntityFactory _entityFactory;
    private readonly MapLoader _mapLoader;

    public GameWorldBehaviorFactory(EntityFactory entityFactory, MapLoader mapLoader)
    {
        _entityFactory = entityFactory;
        _mapLoader = mapLoader;
    }

    public string BehaviorName => SceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new GameWorldSceneBehavior(scene, _entityFactory, _mapLoader);

    private sealed class GameWorldSceneBehavior : SceneBehavior
    {
        private readonly EntityFactory _entityFactory;
        private readonly MapLoader _mapLoader;

        public GameWorldSceneBehavior(Scene scene, EntityFactory entityFactory, MapLoader mapLoader) : base(scene)
        {
            _entityFactory = entityFactory;
            _mapLoader = mapLoader;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _entityFactory.CreateDevControls(Scene);
            _entityFactory.CreateCamera(Scene);

            var tmxPath = DevConfig.MapFile ?? Path.Combine("Assets", "Maps", "level_03.tmx");
            _mapLoader.LoadMap(Scene, tmxPath);
        }
    }
}