using Geisha.Engine.Core.SceneModel;

namespace SQ2;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DefaultSceneBehaviorFactory : ISceneBehaviorFactory
{
    private const string DefaultSceneBehaviorName = "Default";
    private readonly EntityFactory _entityFactory;

    public DefaultSceneBehaviorFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    public string BehaviorName => DefaultSceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new DefaultSceneBehavior(scene, _entityFactory);

    private sealed class DefaultSceneBehavior : SceneBehavior
    {
        private readonly EntityFactory _entityFactory;

        public DefaultSceneBehavior(Scene scene, EntityFactory entityFactory) : base(scene)
        {
            _entityFactory = entityFactory;
        }

        public override string Name => DefaultSceneBehaviorName;

        protected override void OnLoaded()
        {
            _entityFactory.CreateDevControls(Scene);

            _entityFactory.CreateCamera(Scene);

            _entityFactory.CreatePlayer(Scene, 0, 21);

            _entityFactory.CreateLevelTile(Scene, -2, -3);

            const int bx1 = -2;
            const int by1 = -4;
            _entityFactory.CreateLevelTile(Scene, bx1, by1);
            _entityFactory.CreateLevelTile(Scene, bx1 + 1, by1);
            _entityFactory.CreateLevelTile(Scene, bx1 + 2, by1);
            _entityFactory.CreateLevelTile(Scene, bx1 + 3, by1);
            _entityFactory.CreateLevelTile(Scene, bx1 + 4, by1);

            const int bx2 = 3;
            const int by2 = -1;
            _entityFactory.CreateLevelTile(Scene, bx2, by2);
            _entityFactory.CreateLevelTile(Scene, bx2 + 1, by2);
            _entityFactory.CreateLevelTile(Scene, bx2 + 2, by2);
            _entityFactory.CreateLevelTile(Scene, bx2 + 3, by2);
            _entityFactory.CreateLevelTile(Scene, bx2 + 4, by2);
        }
    }
}