using System;
using System.Linq;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
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

        var count = tileMap.TileLayers.Count(tl => tl.Name == "GamePlayTiles");
        if (count != 1)
        {
            throw new InvalidOperationException($"Tile map must contain exactly one 'GamePlayTiles' tile layer, but found {count}.");
        }

        var tileLayerIndex = 0;
        var gameplayTilesLayerIndex = int.MaxValue;
        foreach (var tileLayer in tileMap.TileLayers)
        {
            if (tileLayer.Name == "GamePlayTiles")
            {
                gameplayTilesLayerIndex = tileLayerIndex;
                LoadGamePlayTileLayer(scene, tileLayer);
            }
            else
            {
                var sortingLayerName = tileLayerIndex > gameplayTilesLayerIndex
                    ? GlobalSettings.SortingLayers.DecorForeground
                    : GlobalSettings.SortingLayers.DecorBackground;

                LoadDecorTileLayer(scene, tileLayer, sortingLayerName, tileLayerIndex);
            }

            tileLayerIndex++;
        }

        var objectLayer = tileMap.ObjectLayers.Single(ol => ol.Name == "GamePlayObjects");
        LoadObjectLayer(scene, objectLayer);

        _entityFactory.CreateBackground(scene);

        Logger.Info("Map loading completed.");
    }

    private void LoadDecorTileLayer(Scene scene, TileLayer tileLayer, string sortingLayerName, int layerIndex)
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

                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);

                switch (tile.Type)
                {
                    case "WorldTile":
                    {
                        var assetId = new AssetId(new Guid(tile.Properties["AssetId"].StringValue));
                        var tileType = tile.Properties["TileType"].StringValue;
                        switch (tileType)
                        {
                            case "Geometry":
                            case "WaterDeep":
                            case "Decor":
                                _entityFactory.CreateDecor(scene, tx, ty, assetId, orientation, sortingLayerName, layerIndex);
                                break;
                            case "AnimatedDecor":
                                _entityFactory.CreateAnimatedDecor(scene, tx, ty, assetId, orientation, sortingLayerName, layerIndex);
                                break;
                            default:
                                Logger.Error("Unknown WorldTile: {TileType} at position ({w}, {h}) in tile layer {tileLayer.Name}", tileType, w, h,
                                    tileLayer.Name);
                                break;
                        }

                        break;
                    }
                    default:
                        Logger.Error("Unknown tile type: {tile.Type} at position ({w}, {h}) in tile layer {tileLayer.Name}", tile.Type, w, h, tileLayer.Name);
                        break;
                }
            }
        }
    }

    private void LoadGamePlayTileLayer(Scene scene, TileLayer tileLayer)
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
                            {
                                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);
                                _entityFactory.CreateGeometry(scene, tx, ty, assetId, orientation);
                                break;
                            }
                            case "WaterDeep":
                                AssertNoFlippingFlags(tileLayer, tile, w, h);
                                _entityFactory.CreateWaterDeep(scene, tx, ty, assetId);
                                break;
                            case "Spikes":
                            {
                                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);
                                _entityFactory.CreateSpikes(scene, tx, ty, assetId, orientation);
                                break;
                            }
                            case "CheckPoint":
                                AssertNoFlippingFlags(tileLayer, tile, w, h);
                                _entityFactory.CreateCheckPoint(scene, tx, ty, assetId);
                                break;
                            case "DropPlatform":
                                AssertNoFlippingFlags(tileLayer, tile, w, h);
                                _entityFactory.CreateDropPlatform(scene, tx, ty, assetId);
                                break;
                            case "VanishPlatform":
                                AssertNoFlippingFlags(tileLayer, tile, w, h);
                                _entityFactory.CreateVanishPlatform(scene, tx, ty, assetId);
                                break;
                            case "JumpPad":
                                AssertNoFlippingFlags(tileLayer, tile, w, h);
                                _entityFactory.CreateJumpPad(scene, tx, ty);
                                break;
                            case "Ladder":
                            {
                                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);
                                _entityFactory.CreateLadder(scene, tx, ty, assetId, orientation);
                                break;
                            }
                            case "Decor":
                            {
                                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);
                                _entityFactory.CreateDecor(scene, tx, ty, assetId, orientation, RenderingConfiguration.DefaultSortingLayerName, 0);
                                break;
                            }
                            case "AnimatedDecor":
                            {
                                var orientation = GetOrientationFromGlobalTileId(tile.GlobalTileId);
                                _entityFactory.CreateAnimatedDecor(scene, tx, ty, assetId, orientation, RenderingConfiguration.DefaultSortingLayerName, 0);
                                break;
                            }
                            default:
                                Logger.Error("Unknown WorldTile: {TileType} at position ({w}, {h}) in tile layer {tileLayer.Name}", tileType, w, h,
                                    tileLayer.Name);
                                break;
                        }

                        break;
                    }
                    case "CharacterTile":
                    {
                        AssertNoFlippingFlags(tileLayer, tile, w, h);

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
                            case "Boss_Blue":
                                _entityFactory.CreateBlueBoss(scene, tx, ty);
                                break;
                            default:
                                Logger.Error("Unknown CharacterTile: {CharacterType} at position ({w}, {h}) in tile layer {tileLayer.Name}", characterType, w,
                                    h, tileLayer.Name);
                                break;
                        }

                        break;
                    }
                    default:
                        Logger.Error("Unknown tile type: {tile.Type} at position ({w}, {h}) in tile layer {tileLayer.Name}", tile.Type, w, h, tileLayer.Name);
                        break;
                }
            }
        }
    }

    private void LoadObjectLayer(Scene scene, ObjectLayer objectLayer)
    {
        var playerSpawnPointCount = objectLayer.Objects.Count(o => o.Type == "PlayerSpawnPoint");
        switch (playerSpawnPointCount)
        {
            case 0:
                throw new InvalidOperationException("Map must contain exactly one PlayerSpawnPoint object, but none found.");
            case 1:
                break;
            default:
                throw new InvalidOperationException($"Map must contain exactly one PlayerSpawnPoint object, but found {playerSpawnPointCount}.");
        }

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
                case "Metadata":
                    // Ignore metadata objects
                    break;
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
            return new Orientation(Direction.Up, Flip.None);
        }

        if (globalTileId is { FlippedHorizontally: true, FlippedVertically: true, FlippedDiagonally: false })
        {
            return new Orientation(Direction.Down, Flip.None);
        }

        if (globalTileId is { FlippedHorizontally: true, FlippedVertically: false, FlippedDiagonally: true })
        {
            return new Orientation(Direction.Right, Flip.None);
        }

        if (globalTileId is { FlippedHorizontally: false, FlippedVertically: true, FlippedDiagonally: true })
        {
            return new Orientation(Direction.Left, Flip.None);
        }

        if (globalTileId is { FlippedHorizontally: false, FlippedVertically: true, FlippedDiagonally: false })
        {
            return new Orientation(Direction.Up, Flip.Vertical);
        }

        if (globalTileId is { FlippedHorizontally: true, FlippedVertically: false, FlippedDiagonally: false })
        {
            return new Orientation(Direction.Up, Flip.Horizontal);
        }

        if (globalTileId is { FlippedHorizontally: false, FlippedVertically: false, FlippedDiagonally: true })
        {
            return new Orientation(Direction.Left, Flip.Horizontal);
        }

        if (globalTileId is { FlippedHorizontally: true, FlippedVertically: true, FlippedDiagonally: true })
        {
            return new Orientation(Direction.Right, Flip.Horizontal);
        }

        throw new ArgumentOutOfRangeException(nameof(globalTileId), $"Unsupported tile id '{globalTileId}' for orientation.");
    }

    private static void AssertNoFlippingFlags(TileLayer tileLayer, Tile tile, int w, int h)
    {
        if (tile.GlobalTileId.HasFlippingFlags)
        {
            Logger.Error("Tile at position ({w}, {h}) in tile layer {tileLayer.Name} has flipping flags set. Flipping is not supported for this tile.", w, h,
                tileLayer.Name);
        }
    }
}