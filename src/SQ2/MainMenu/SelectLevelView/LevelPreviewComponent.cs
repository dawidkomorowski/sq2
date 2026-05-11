using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;

namespace SQ2.MainMenu.SelectLevelView;

internal sealed class LevelPreviewComponent : BehaviorComponent
{
    private readonly IAssetStore _assetStore;

    public LevelPreviewComponent(Entity entity, IAssetStore assetStore) : base(entity)
    {
        _assetStore = assetStore;
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
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LevelPreviewComponentFactory : ComponentFactory<LevelPreviewComponent>
{
    private readonly IAssetStore _assetStore;

    public LevelPreviewComponentFactory(IAssetStore assetStore)
    {
        _assetStore = assetStore;
    }

    protected override LevelPreviewComponent CreateComponent(Entity entity) => new(entity, _assetStore);
}