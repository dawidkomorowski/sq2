using Geisha.Engine.Core.Assets;

namespace SQ2.Core;

internal sealed class LevelInfo
{
    public string Name { get; private init; } = string.Empty;
    public string MapFileName { get; private init; } = string.Empty;
    public AssetId PreviewSpriteAssetId { get; private init; }
    public string DiamondId { get; private init; } = string.Empty;

    public static LevelInfo[] Levels { get; } = DefineLevels();

    private static LevelInfo[] DefineLevels()
    {
        return new[]
        {
            new LevelInfo
            {
                Name = "Level 1",
                MapFileName = "level_01.tmx",
                PreviewSpriteAssetId = AssetId.Parse("ffbca60b-d912-499c-b75b-774aea1b05fc"),
                DiamondId = string.Empty
            },
            new LevelInfo
            {
                Name = "Level 2",
                MapFileName = "level_02.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "f9491c64-518c-41f2-b380-ff4426da66e9"
            }
        };
    }
}