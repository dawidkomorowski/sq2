using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.UI;

namespace SQ2.MainMenu.SelectLevelView;

internal sealed class LevelPreviewComponent : BehaviorComponent
{
    private readonly IAssetStore _assetStore;
    private readonly GameStateService _gameStateService;

    public LevelPreviewComponent(Entity entity, IAssetStore assetStore, GameStateService gameStateService) : base(entity)
    {
        _assetStore = assetStore;
        _gameStateService = gameStateService;
    }

    public LevelInfo? LevelInfo { get; set; }

    public override void OnStart()
    {
        var levelNameEntity = Entity.CreateChildEntity();
        var levelNameTransform = levelNameEntity.CreateComponent<Transform2DComponent>();
        levelNameTransform.Translation = new Vector2(0, 95);
        var levelNameTextRenderer = levelNameEntity.CreateComponent<TextRendererComponent>();
        levelNameTextRenderer.Text = LevelInfo?.Name ?? string.Empty;
        levelNameTextRenderer.Color = Color.White;
        levelNameTextRenderer.TextAlignment = TextAlignment.Center;
        levelNameTextRenderer.MaxWidth = 300;
        levelNameTextRenderer.Pivot = new Vector2(150, 0);
        levelNameTextRenderer.FontSize = FontSize.FromDips(12);
        levelNameTextRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;
        levelNameTextRenderer.FontFamilyName = GlobalSettings.Font;

        var levelImageEntity = Entity.CreateChildEntity();
        levelImageEntity.CreateComponent<Transform2DComponent>();
        var levelSpriteRenderer = levelImageEntity.CreateComponent<SpriteRendererComponent>();
        levelSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        levelSpriteRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

        if (LevelInfo is not null)
        {
            levelSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(LevelInfo.PreviewSpriteAssetId);

            CreateCoinInfo(LevelInfo.Index);
            CreateDiamondInfo(LevelInfo.DiamondId);
        }
    }

    private void CreateCoinInfo(int level)
    {
        var coinInfoEntity = Entity.CreateChildEntity();
        var coinInfoTransform = coinInfoEntity.CreateComponent<Transform2DComponent>();
        coinInfoTransform.Translation = new Vector2(-40, -90);

        var collectedNumberEntity = coinInfoEntity.CreateChildEntity();
        var collectedNumberTransform = collectedNumberEntity.CreateComponent<Transform2DComponent>();
        collectedNumberTransform.Translation = new Vector2(-30, 0);
        var collectedNumberRenderer = collectedNumberEntity.CreateComponent<NumberRendererComponent>();
        collectedNumberRenderer.UseDefaultSprites(_assetStore);
        collectedNumberRenderer.Value = _gameStateService.GetCollectedCoins(level);
        collectedNumberRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

        var timesIconEntity = coinInfoEntity.CreateChildEntity();
        var timesIconTransform = timesIconEntity.CreateComponent<Transform2DComponent>();
        timesIconTransform.Translation = new Vector2(0, 0);
        var timesIconSpriteRenderer = timesIconEntity.CreateComponent<SpriteRendererComponent>();
        timesIconSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fa82409e-2840-4492-bc9b-4b48e6030430"));
        timesIconSpriteRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;
        timesIconSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        var coinIconEntity = coinInfoEntity.CreateChildEntity();
        var coinIconTransform = coinIconEntity.CreateComponent<Transform2DComponent>();
        coinIconTransform.Translation = new Vector2(20, 0);
        var coinIconSpriteRenderer = coinIconEntity.CreateComponent<SpriteRendererComponent>();
        coinIconSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("eb06525a-acc2-4593-abc9-506763f077cc"));
        coinIconSpriteRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;
        coinIconSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
    }

    private void CreateDiamondInfo(string diamondId)
    {
        var diamondInfoEntity = Entity.CreateChildEntity();
        var diamondInfoTransform = diamondInfoEntity.CreateComponent<Transform2DComponent>();
        diamondInfoTransform.Translation = new Vector2(40, -90);

        var collectedNumberEntity = diamondInfoEntity.CreateChildEntity();
        var collectedNumberTransform = collectedNumberEntity.CreateComponent<Transform2DComponent>();
        collectedNumberTransform.Translation = new Vector2(-30, 0);
        var collectedNumberRenderer = collectedNumberEntity.CreateComponent<NumberRendererComponent>();
        collectedNumberRenderer.UseDefaultSprites(_assetStore);
        collectedNumberRenderer.Value = _gameStateService.IsDiamondCollected(diamondId) ? 1 : 0;
        collectedNumberRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

        var separatorEntity = diamondInfoEntity.CreateChildEntity();
        var separatorTransform = separatorEntity.CreateComponent<Transform2DComponent>();
        separatorTransform.Translation = new Vector2(-4, 0);
        var separatorSpriteRenderer = separatorEntity.CreateComponent<SpriteRendererComponent>();
        separatorSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        separatorSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("c5624f39-cf75-4177-a673-99d7aa785586"));
        separatorSpriteRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

        var totalNumberEntity = diamondInfoEntity.CreateChildEntity();
        var totalNumberTransform = totalNumberEntity.CreateComponent<Transform2DComponent>();
        totalNumberTransform.Translation = new Vector2(0, 0);
        var totalNumberRenderer = totalNumberEntity.CreateComponent<NumberRendererComponent>();
        totalNumberRenderer.UseDefaultSprites(_assetStore);
        totalNumberRenderer.Value = diamondId != string.Empty ? 1 : 0;
        totalNumberRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

        var diamondEntity = diamondInfoEntity.CreateChildEntity();
        var diamondTransform = diamondEntity.CreateComponent<Transform2DComponent>();
        diamondTransform.Translation = new Vector2(30, 0);
        var diamondSpriteRenderer = diamondEntity.CreateComponent<SpriteRendererComponent>();
        diamondSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        diamondSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fd94100c-a1fd-4c5b-bc5c-a7337ca94f02"));
        diamondSpriteRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LevelPreviewComponentFactory : ComponentFactory<LevelPreviewComponent>
{
    private readonly IAssetStore _assetStore;
    private readonly GameStateService _gameStateService;

    public LevelPreviewComponentFactory(IAssetStore assetStore, GameStateService gameStateService)
    {
        _assetStore = assetStore;
        _gameStateService = gameStateService;
    }

    protected override LevelPreviewComponent CreateComponent(Entity entity) => new(entity, _assetStore, _gameStateService);
}