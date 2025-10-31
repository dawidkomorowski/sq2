﻿using System;
using System.Linq;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.SceneModel;
using Geisha.Extensions.Tiled;

namespace SQ2.Core;

// ReSharper disable once ClassNeverInstantiated.Global
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
                    var tileType = tile.Properties["TileType"].StringValue;
                    switch (tileType)
                    {
                        case "Geometry":
                            _entityFactory.CreateGeometry(scene, tx, ty, assetId);
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
                    }
                }

                if (tile.Type == "CharacterTile")
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
                _entityFactory.CreatePlayerSpawnPoint(scene, x, y);
                _entityFactory.CreatePlayer(scene, x, y);
            }

            if (tileObject.Type == "MovingPlatform" && tileObject is TiledObject.Tile tile)
            {
                var startObjectId = tile.Properties["StartPosition"].ObjectValue;
                var endObjectId = tile.Properties["EndPosition"].ObjectValue;
                var startPositionObject = objectLayer.Objects.Single(o => o.Id == startObjectId);
                var endPositionObject = objectLayer.Objects.Single(o => o.Id == endObjectId);

                var sx = startPositionObject.X;
                var sy = -startPositionObject.Y;
                var ex = endPositionObject.X;
                var ey = -endPositionObject.Y;

                _entityFactory.CreateMovingPlatform(scene, x, y, sx, sy, ex, ey);
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