using Geisha.Engine.Animation;
using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.UI;

namespace SQ2.LevelSummary;

internal sealed class LevelSummaryBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = GlobalSettings.SceneNames.LevelSummary;
    private readonly IAssetStore _assetStore;

    public LevelSummaryBehaviorFactory(IAssetStore assetStore)
    {
        _assetStore = assetStore;
    }

    public string BehaviorName => SceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new LevelSummaryBehavior(scene, _assetStore);

    private sealed class LevelSummaryBehavior : SceneBehavior
    {
        private readonly IAssetStore _assetStore;

        public LevelSummaryBehavior(Scene scene, IAssetStore assetStore) : base(scene)
        {
            _assetStore = assetStore;
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            var cameraEntity = Scene.CreateEntity();
            cameraEntity.CreateComponent<Transform2DComponent>();
            var cameraComponent = cameraEntity.CreateComponent<CameraComponent>();
            cameraComponent.ViewRectangle = GlobalSettings.ViewSize;
            cameraComponent.AspectRatioBehavior = AspectRatioBehavior.Underscan;

            var backgroundEntity = Scene.CreateEntity();
            backgroundEntity.CreateComponent<Transform2DComponent>();
            var backgroundRenderer = backgroundEntity.CreateComponent<RectangleRendererComponent>();
            backgroundRenderer.Dimensions = GlobalSettings.ViewSize * 2;
            backgroundRenderer.OrderInLayer = 0;
            backgroundRenderer.FillInterior = true;
            backgroundRenderer.Color = Color.Black;

            var playerCharacterEntity = Scene.CreateEntity();
            playerCharacterEntity.CreateComponent<Transform2DComponent>();
            var spriteRendererComponent = playerCharacterEntity.CreateComponent<SpriteRendererComponent>();
            spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
            var spriteAnimationComponent = playerCharacterEntity.CreateComponent<SpriteAnimationComponent>();
            spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("d7a11945-11f6-41a3-90fb-46dae40c6c56")));
            spriteAnimationComponent.PlayInLoop = true;
            spriteAnimationComponent.PlaybackSpeed = 3;
            spriteAnimationComponent.PlayAnimation("Walk");

            var coinInfoEntity = CreateCoinInfo(0);
            var diamondInfoEntity = CreateDiamondInfo(false, true);

            var animationEntity = Scene.CreateEntity();
            var levelSummaryAnimationComponent = animationEntity.CreateComponent<LevelSummaryAnimationComponent>();
            levelSummaryAnimationComponent.PlayerCharacter = playerCharacterEntity;
            levelSummaryAnimationComponent.CoinInfo = coinInfoEntity;
            levelSummaryAnimationComponent.DiamondInfo = diamondInfoEntity;
        }

        private Entity CreateCoinInfo(int collectedCoins)
        {
            var coinInfoEntity = Scene.CreateEntity();
            var coinInfoTransform = coinInfoEntity.CreateComponent<Transform2DComponent>();
            coinInfoTransform.Translation = new Vector2(0, -50);

            var collectedNumberEntity = coinInfoEntity.CreateChildEntity();
            var collectedNumberTransform = collectedNumberEntity.CreateComponent<Transform2DComponent>();
            collectedNumberTransform.Translation = new Vector2(-30, 0);
            var collectedNumberRenderer = collectedNumberEntity.CreateComponent<NumberRendererComponent>();
            collectedNumberRenderer.UseDefaultSprites(_assetStore);
            collectedNumberRenderer.Value = collectedCoins;

            var timesIconEntity = coinInfoEntity.CreateChildEntity();
            var timesIconTransform = timesIconEntity.CreateComponent<Transform2DComponent>();
            timesIconTransform.Translation = new Vector2(0, 0);
            var timesIconSpriteRenderer = timesIconEntity.CreateComponent<SpriteRendererComponent>();
            timesIconSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fa82409e-2840-4492-bc9b-4b48e6030430"));
            timesIconSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

            var coinIconEntity = coinInfoEntity.CreateChildEntity();
            var coinIconTransform = coinIconEntity.CreateComponent<Transform2DComponent>();
            coinIconTransform.Translation = new Vector2(20, 0);
            var coinIconSpriteRenderer = coinIconEntity.CreateComponent<SpriteRendererComponent>();
            coinIconSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("eb06525a-acc2-4593-abc9-506763f077cc"));
            coinIconSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

            return coinIconEntity;
        }

        private Entity CreateDiamondInfo(bool isDiamondCollected, bool hasDiamond)
        {
            var diamondInfoEntity = Scene.CreateEntity();
            var diamondInfoTransform = diamondInfoEntity.CreateComponent<Transform2DComponent>();
            diamondInfoTransform.Translation = new Vector2(0, -90);

            var collectedNumberEntity = diamondInfoEntity.CreateChildEntity();
            var collectedNumberTransform = collectedNumberEntity.CreateComponent<Transform2DComponent>();
            collectedNumberTransform.Translation = new Vector2(-30, 0);
            var collectedNumberRenderer = collectedNumberEntity.CreateComponent<NumberRendererComponent>();
            collectedNumberRenderer.UseDefaultSprites(_assetStore);
            collectedNumberRenderer.Value = isDiamondCollected ? 1 : 0;
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
            totalNumberRenderer.Value = hasDiamond ? 1 : 0;
            totalNumberRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

            var diamondEntity = diamondInfoEntity.CreateChildEntity();
            var diamondTransform = diamondEntity.CreateComponent<Transform2DComponent>();
            diamondTransform.Translation = new Vector2(30, 0);
            var diamondSpriteRenderer = diamondEntity.CreateComponent<SpriteRendererComponent>();
            diamondSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
            diamondSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fd94100c-a1fd-4c5b-bc5c-a7337ca94f02"));
            diamondSpriteRenderer.SortingLayerName = GlobalSettings.SortingLayers.MenuForeground;

            return diamondInfoEntity;
        }
    }
}