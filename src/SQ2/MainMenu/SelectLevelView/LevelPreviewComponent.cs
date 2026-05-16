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
        levelNameTransform.Translation = new Vector2(0, 100);
        var levelNameTextRenderer = levelNameEntity.CreateComponent<TextRendererComponent>();
        levelNameTextRenderer.Text = LevelInfo?.Name ?? string.Empty;
        levelNameTextRenderer.Color = Color.White;
        levelNameTextRenderer.TextAlignment = TextAlignment.Center;
        levelNameTextRenderer.MaxWidth = 300;
        levelNameTextRenderer.Pivot = new Vector2(150, 0);
        levelNameTextRenderer.FontSize = FontSize.FromDips(24);

        var levelImageEntity = Entity.CreateChildEntity();
        levelImageEntity.CreateComponent<Transform2DComponent>();
        var levelSpriteRenderer = levelImageEntity.CreateComponent<SpriteRendererComponent>();
        levelSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        if (LevelInfo is not null)
        {
            levelSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(LevelInfo.PreviewSpriteAssetId);

            CreateDiamondInfo(LevelInfo.DiamondId);
        }
    }

    private void CreateDiamondInfo(string diamondId)
    {
        var diamondInfoEntity = Entity.CreateChildEntity();
        var diamondInfoTransform = diamondInfoEntity.CreateComponent<Transform2DComponent>();
        diamondInfoTransform.Translation = new Vector2(0, -90);

        var collectedNumberEntity = diamondInfoEntity.CreateChildEntity();
        var collectedNumberTransform = collectedNumberEntity.CreateComponent<Transform2DComponent>();
        collectedNumberTransform.Translation = new Vector2(-30, 0);
        var collectedNumberRenderer = collectedNumberEntity.CreateComponent<NumberRendererComponent>();
        collectedNumberRenderer.UseDefaultSprites(_assetStore);
        collectedNumberRenderer.Value = _gameStateService.IsDiamondCollected(diamondId) ? 1 : 0;

        var totalNumberEntity = diamondInfoEntity.CreateChildEntity();
        var totalNumberTransform = totalNumberEntity.CreateComponent<Transform2DComponent>();
        totalNumberTransform.Translation = new Vector2(0, 0);
        var totalNumberRenderer = totalNumberEntity.CreateComponent<NumberRendererComponent>();
        totalNumberRenderer.UseDefaultSprites(_assetStore);
        totalNumberRenderer.Value = diamondId != string.Empty ? 1 : 0;

        var diamondEntity = diamondInfoEntity.CreateChildEntity();
        var diamondTransform = diamondEntity.CreateComponent<Transform2DComponent>();
        diamondTransform.Translation = new Vector2(30, 0);
        var diamondSpriteRenderer = diamondEntity.CreateComponent<SpriteRendererComponent>();
        diamondSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        diamondSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fd94100c-a1fd-4c5b-bc5c-a7337ca94f02"));
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