using System;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Components.Development;
using SQ2.Components.GamePlay;

namespace SQ2;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EntityFactory
{
    private readonly IAssetStore _assetStore;

    public EntityFactory(IAssetStore assetStore)
    {
        _assetStore = assetStore;
    }

    public Entity CreateDevControls(Scene scene)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<DevControlsComponent>();
        return entity;
    }

    public Entity CreatePlayerSpawnPoint(Scene scene, double x, double y)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<PlayerSpawnPointComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y);
        return entity;
    }

    public Entity CreatePlayer(Scene scene, double x, double y)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<PlayerComponent>();
        entity.CreateComponent<InputComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("ff2c22e2-d8b9-4e7e-b6fa-e1926e98465b")));
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(14, 22);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;
        return entity;
    }

    public Entity CreateGeometry(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(tx * GlobalSettings.TileSize.Width, ty * GlobalSettings.TileSize.Height);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateSpikes(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<SpikesComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(tx * GlobalSettings.TileSize.Width, ty * GlobalSettings.TileSize.Height);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);

        var collisionEntity = entity.CreateChildEntity();
        var collisionTransform2DComponent = collisionEntity.CreateComponent<Transform2DComponent>();
        collisionTransform2DComponent.Translation = new Vector2(0, -GlobalSettings.TileSize.Height / 4d);
        var rectangleColliderComponent = collisionEntity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width - 2, (GlobalSettings.TileSize.Height / 2d) - 2);
        return entity;
    }

    public Entity CreateCamera(Scene scene)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<Transform2DComponent>();
        var cameraComponent = entity.CreateComponent<CameraComponent>();
        cameraComponent.ViewRectangle = GlobalSettings.ViewSize;
        return entity;
    }
}