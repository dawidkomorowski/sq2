using System;
using System.IO;
using System.Linq;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.SceneModel;
using Geisha.Extensions.Tiled;

namespace SQ2;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameWorldBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = "GameWorld";
    private readonly EntityFactory _entityFactory;

    public GameWorldBehaviorFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    public string BehaviorName => SceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new GameWorldSceneBehavior(scene, _entityFactory);

    private sealed class GameWorldSceneBehavior : SceneBehavior
    {
        private readonly EntityFactory _entityFactory;

        public GameWorldSceneBehavior(Scene scene, EntityFactory entityFactory) : base(scene)
        {
            _entityFactory = entityFactory;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            _entityFactory.CreateDevControls(Scene);

            _entityFactory.CreateCamera(Scene);

            var tmxPath = Path.Combine("Assets", "Maps", "test.tmx");
            var tileMap = TileMap.LoadFromFile(tmxPath);
            var tileLayer = tileMap.TileLayers.Single(tl => tl.Name == "GamePlayTiles");
            for (var tx = 0; tx < tileLayer.Width; tx++)
            {
                for (var ty = 0; ty < tileLayer.Height; ty++)
                {
                    var tile = tileLayer.Tiles[tx][ty];
                    if (tile == null)
                    {
                        continue;
                    }

                    if (tile.Type == "WorldTile")
                    {
                        var assetId = new AssetId(new Guid(tile.Properties["AssetId"].StringValue));
                        _entityFactory.CreateWorldTile(Scene, tx, -ty, assetId);
                    }
                }
            }

            var objectLayer = tileMap.ObjectLayers.Single(ol => ol.Name == "GamePlayObjects");
            foreach (var tileObject in objectLayer.Objects)
            {
                if (tileObject.Type == "PlayerSpawnPoint")
                {
                    _entityFactory.CreatePlayer(Scene, tileObject.X - GlobalSettings.TileSize.Width / 2d,
                        -(tileObject.Y - GlobalSettings.TileSize.Height / 2d));
                }
            }
        }
    }
}