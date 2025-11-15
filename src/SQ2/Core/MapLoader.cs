using System;
using System.Linq;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.SceneModel;
using Geisha.Extensions.Tiled;
using NLog;

namespace SQ2.Core;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MapLoader
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly EntityFactory _entityFactory;

    public MapLoader(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    public void LoadMap(Scene scene, string mapFilePath)
    {
        Logger.Info("Loading map from file: {mapFilePath}", mapFilePath);

        var tileMap = TileMap.LoadFromFile(mapFilePath);

        var tileLayer = tileMap.TileLayers.Single(tl => tl.Name == "GamePlayTiles");
        LoadTileLayer(scene, tileLayer);

        var objectLayer = tileMap.ObjectLayers.Single(ol => ol.Name == "GamePlayObjects");
        LoadObjectLayer(scene, objectLayer);

        _entityFactory.CreateBackground(scene);

        Logger.Info("Map loading completed.");
    }

    private void LoadTileLayer(Scene scene, TileLayer tileLayer)
    {
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

                switch (tile.Type)
                {
                    case "WorldTile":
                    {
                        var assetId = new AssetId(new Guid(tile.Properties["AssetId"].StringValue));
                        var tileType = tile.Properties["TileType"].StringValue;
                        switch (tileType)
                        {
                            case "Geometry":
                                _entityFactory.CreateGeometry(scene, tx, ty, assetId);
                                break;
                            case "WaterDeep":
                                _entityFactory.CreateWaterDeep(scene, tx, ty, assetId);
                                break;
                            case "WaterSurface":
                                _entityFactory.CreateWaterSurface(scene, tx, ty, assetId);
                                break;
                            case "Spikes":
                                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);
                                _entityFactory.CreateSpikes(scene, tx, ty, assetId, orientation);
                                break;
                            case "CheckPoint":
                                _entityFactory.CreatePlayerCheckPoint(scene, tx, ty, assetId);
                                break;
                            case "DropPlatform":
                                _entityFactory.CreateDropPlatform(scene, tx, ty, assetId);
                                break;
                            case "JumpPad":
                                _entityFactory.CreateJumpPad(scene, tx, ty);
                                break;
                            case "Ladder":
                                _entityFactory.CreateLadder(scene, tx, ty, assetId);
                                break;
                            case "Decor":
                                _entityFactory.CreateDecor(scene, tx, ty, assetId, 0);
                                break;
                        }

                        break;
                    }
                    case "CharacterTile":
                    {
                        var characterType = tile.Properties["CharacterType"].StringValue;
                        switch (characterType)
                        {
                            case "Enemy_Blue_Small":
                                _entityFactory.CreateBlueEnemy(scene, tx, ty);
                                break;
                            case "Enemy_Red":
                                _entityFactory.CreateRedEnemy(scene, tx, ty);
                                break;
                            case "Enemy_Yellow":
                                _entityFactory.CreateYellowEnemy(scene, tx, ty);
                                break;
                        }

                        break;
                    }
                    default:
                        Logger.Error("Unknown tile type: {tile.Type} at position ({w}, {h})", tile.Type, w, h);
                        break;
                }
            }
        }
    }

    private void LoadObjectLayer(Scene scene, ObjectLayer objectLayer)
    {
        foreach (var tiledObject in objectLayer.Objects)
        {
            var x = tiledObject.X - GlobalSettings.TileSize.Width / 2d;
            var y = -(tiledObject.Y - GlobalSettings.TileSize.Height / 2d);

            switch (tiledObject.Type)
            {
                case "PlayerSpawnPoint":
                    _entityFactory.CreatePlayerSpawnPoint(scene, x, y);
                    _entityFactory.CreatePlayer(scene, x, y);
                    break;
                case "MovingPlatform" when tiledObject is TiledObject.Tile:
                {
                    var startObjectId = tiledObject.Properties["StartPosition"].ObjectValue;
                    var endObjectId = tiledObject.Properties["EndPosition"].ObjectValue;
                    var startPositionObject = objectLayer.Objects.Single(o => o.Id == startObjectId);
                    var endPositionObject = objectLayer.Objects.Single(o => o.Id == endObjectId);

                    var sx = startPositionObject.X;
                    var sy = -startPositionObject.Y;
                    var ex = endPositionObject.X;
                    var ey = -endPositionObject.Y;

                    _entityFactory.CreateMovingPlatform(scene, x, y, sx, sy, ex, ey);
                    break;
                }
                case "FishEnemy" when tiledObject is TiledObject.Tile:
                {
                    var jumpOffset = 0;
                    if (tiledObject.Properties.TryGetProperty("JumpOffset", out var prop))
                    {
                        jumpOffset = prop?.IntValue ?? 0;
                    }

                    _entityFactory.CreateFishEnemy(scene, x, y, jumpOffset);
                    break;
                }
                case "DestructibleWall" when tiledObject is TiledObject.Tile:
                {
                    var buttonId = tiledObject.Properties["Button"].ObjectValue;

                    var xx = x + GlobalSettings.TileSize.Width / 2d;
                    var yy = y + GlobalSettings.TileSize.Height / 2d;
                    _entityFactory.CreateDestructibleWall(scene, xx, yy, buttonId);
                    break;
                }
                case "Button" when tiledObject is TiledObject.Tile:
                {
                    var xx = x + GlobalSettings.TileSize.Width / 2d;
                    var yy = y + GlobalSettings.TileSize.Height / 2d;
                    _entityFactory.CreateButton(scene, xx, yy, tiledObject.Id);
                    break;
                }
                default:
                    Logger.Error("Unknown object type: {tiledObject.Type} with id {tiledObject.Id}", tiledObject.Type, tiledObject.Id);
                    break;
            }
        }
    }

    private static Orientation GetOrientationFromGlobalTileId(GlobalTileId globalTileId)
    {
        if (!globalTileId.HasFlippingFlags)
        {
            return Orientation.Up;
        }

        if (globalTileId is { FlippedHorizontally: true, FlippedVertically: true, FlippedDiagonally: false })
        {
            return Orientation.Down;
        }

        if (globalTileId is { FlippedHorizontally: true, FlippedVertically: false, FlippedDiagonally: true })
        {
            return Orientation.Right;
        }

        if (globalTileId is { FlippedHorizontally: false, FlippedVertically: true, FlippedDiagonally: true })
        {
            return Orientation.Left;
        }

        throw new ArgumentOutOfRangeException(nameof(globalTileId), $"Unsupported tile id '{globalTileId}' for orientation.");
    }
}