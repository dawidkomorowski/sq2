using System;
using System.Linq;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.SceneModel;
using Geisha.Extensions.Tiled;

namespace SQ2;

internal sealed class MapLoader
{
    private readonly EntityFactory _entityFactory;

    public MapLoader(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    public void LoadMap(Scene scene, string mapFilePath)
    {
        var tileMap = TileMap.LoadFromFile(mapFilePath);
        var tileLayer = tileMap.TileLayers.Single(tl => tl.Name == "GamePlayTiles");
        for (var w = 0; w < tileLayer.Width; w++)
        {
            for (var h = 0; h < tileLayer.Height; h++)
            {
                var tile = tileLayer.Tiles[w][h];
                if (tile == null)
                {
                    continue;
                }

                var tx = w;
                var ty = -h;

                if (tile.Type == "WorldTile")
                {
                    var assetId = new AssetId(new Guid(tile.Properties["AssetId"].StringValue));
                    _entityFactory.CreateWorldTile(scene, tx, ty, assetId);
                }
            }
        }

        var objectLayer = tileMap.ObjectLayers.Single(ol => ol.Name == "GamePlayObjects");
        foreach (var tileObject in objectLayer.Objects)
        {
            var x = tileObject.X - GlobalSettings.TileSize.Width / 2d;
            var y = -(tileObject.Y - GlobalSettings.TileSize.Height / 2d);

            if (tileObject.Type == "PlayerSpawnPoint")
            {
                _entityFactory.CreatePlayer(scene, x, y);
            }
        }
    }
}