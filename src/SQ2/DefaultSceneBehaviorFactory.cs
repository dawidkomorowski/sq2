using System;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DefaultSceneBehaviorFactory : ISceneBehaviorFactory
{
    private const string DefaultSceneBehaviorName = "Default";
    private readonly IAssetStore _assetStore;

    public DefaultSceneBehaviorFactory(IAssetStore assetStore)
    {
        _assetStore = assetStore;
    }

    public string BehaviorName => DefaultSceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new DefaultSceneBehavior(scene, _assetStore);

    private sealed class DefaultSceneBehavior : SceneBehavior
    {
        private readonly IAssetStore _assetStore;

        public DefaultSceneBehavior(Scene scene, IAssetStore assetStore) : base(scene)
        {
            _assetStore = assetStore;
        }

        public override string Name => DefaultSceneBehaviorName;

        protected override void OnLoaded()
        {
            var e = Scene.CreateEntity();
            e.CreateComponent<Transform2DComponent>();
            var cameraComponent = e.CreateComponent<CameraComponent>();
            cameraComponent.ViewRectangle = new Vector2(1280/4d, 720/4d);

            var entity = Scene.CreateEntity();
            entity.CreateComponent<Transform2DComponent>();
            var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
            spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("c68e1612-a44b-4535-88d0-7a8a223a9546")));
        }
    }
}