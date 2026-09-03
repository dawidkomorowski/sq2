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
                MapFileName = "Level01.tmx",
                PreviewSpriteAssetId = AssetId.Parse("ffbca60b-d912-499c-b75b-774aea1b05fc"),
                DiamondId = "f78ccc5b-189b-4036-bf34-9467d5b6a8a7"
            },
            new LevelInfo
            {
                Name = "Level 2",
                MapFileName = "Level02.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "5df3f396-65a3-471c-ada5-50311939ac22"
            },
            new LevelInfo
            {
                Name = "Level 3",
                MapFileName = "Level03.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "f78d0442-936c-4769-914b-3a8e5d257dc7"
            },
            new LevelInfo
            {
                Name = "Level 4",
                MapFileName = "Level04.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "2f94992d-f3d6-4717-8dda-b4dbc33b89c5"
            },
            new LevelInfo
            {
                Name = "Level 5",
                MapFileName = "Level05.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "ec814c35-d639-48d5-a654-50dcc85a1d44"
            },
            new LevelInfo
            {
                Name = "Level 6",
                MapFileName = "Level06.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "f9491c64-518c-41f2-b380-ff4426da66e9"
            },
            new LevelInfo
            {
                Name = "Level 7",
                MapFileName = "Level07.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "3542c956-cd3e-42ab-bad4-b3de466a166f"
            },
            new LevelInfo
            {
                Name = "Level 8",
                MapFileName = "Level08.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "df3b9294-e4ab-44fb-940f-38e1a13c5237"
            },
            new LevelInfo
            {
                Name = "Level 9",
                MapFileName = "Level09.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "bac9e0a6-8b94-4ce3-ba8d-b0647852f1ae"
            },
            new LevelInfo
            {
                Name = "Level 10",
                MapFileName = "Level10.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "3a696e82-84b9-406e-8ad3-edeed4fa3dca"
            },
            new LevelInfo
            {
                Name = "Level 11",
                MapFileName = "Level11.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "4bc25dcd-815a-42e7-bdfb-ac39f34bab8d"
            },
            new LevelInfo
            {
                Name = "Level 12",
                MapFileName = "Level12.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "6cadb092-b15a-4715-9885-12da98c8c41f"
            },
            new LevelInfo
            {
                Name = "Level 13",
                MapFileName = "Level13.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "f370fa29-1a85-4eb0-b7b0-68efd2b44019"
            },
            new LevelInfo
            {
                Name = "Level 14",
                MapFileName = "Level14.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "70dbf128-395e-47df-8138-3c22be8a836e"
            },
            new LevelInfo
            {
                Name = "Level 15",
                MapFileName = "Level15.tmx",
                PreviewSpriteAssetId = AssetId.Parse("2ad44ac3-026a-428c-bb60-29205bc4e697"),
                DiamondId = "8ab7ddba-d490-4935-aa2f-8e8edabebc7f"
            }
        };
    }
}