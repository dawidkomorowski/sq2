using System;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Development;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;

namespace SQ2.Core;

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

    public Entity CreatePlayerCheckPoint(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<PlayerCheckPointComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        return entity;
    }

    public Entity CreatePlayer(Scene scene, double x, double y)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<PlayerComponent>();
        entity.CreateComponent<InputComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(14, 22);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, 1);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("ff2c22e2-d8b9-4e7e-b6fa-e1926e98465b")));

        return entity;
    }

    public Entity CreateGeometry(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateWaterDeep(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<WaterDeepComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.OrderInLayer = 1;
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateWaterSurface(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.OrderInLayer = 1;
        return entity;
    }

    public Entity CreateSpikes(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<SpikesComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);

        var collisionEntity = entity.CreateChildEntity();
        var collisionTransform2DComponent = collisionEntity.CreateComponent<Transform2DComponent>();
        collisionTransform2DComponent.Translation = new Vector2(0, -GlobalSettings.TileSize.Height / 4d);
        var rectangleColliderComponent = collisionEntity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width - 2, GlobalSettings.TileSize.Height / 2d - 2);

        transform2DComponent.Rotation = orientation switch
        {
            Orientation.Up => 0,
            Orientation.Down => Math.PI,
            Orientation.Left => Math.PI / 2,
            Orientation.Right => -Math.PI / 2,
            _ => transform2DComponent.Rotation
        };

        return entity;
    }

    public Entity CreateDropPlatform(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<DropPlatformComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty) + new Vector2(0, GlobalSettings.TileSize.Height / 4);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width, GlobalSettings.TileSize.Height / 2);
        entity.CreateComponent<KinematicRigidBody2DComponent>();

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, -GlobalSettings.TileSize.Height / 4 + 0.5);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.OrderInLayer = -1;

        return entity;
    }

    public Entity CreateMovingPlatform(Scene scene, double x, double y, double sx, double sy, double ex, double ey)
    {
        var entity = scene.CreateEntity();
        var movingPlatformComponent = entity.CreateComponent<MovingPlatformComponent>();
        movingPlatformComponent.StartPosition = new Vector2(sx, sy) + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height * 0.75);
        movingPlatformComponent.EndPosition = new Vector2(ex, ey) + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height * 0.75);
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y) + new Vector2(GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height * 0.75);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width, GlobalSettings.TileSize.Height / 2);
        entity.CreateComponent<KinematicRigidBody2DComponent>();

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, -GlobalSettings.TileSize.Height / 4 + 0.5);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("e3801ac8-1361-425b-9ca4-48f4fd4b3a4f")));
        spriteRendererComponent.OrderInLayer = -1;

        return entity;
    }

    public Entity CreateJumpPad(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        var jumpPadComponent = entity.CreateComponent<JumpPadComponent>();
        jumpPadComponent.HighSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("afa34cd1-0643-4551-9a0d-76558521b9e7")));
        jumpPadComponent.LowSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("120bb5b7-2f24-4f72-902c-b516ac2334d7")));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty) + new Vector2(0, -2);
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width, 14);

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, 2);
        spriteEntity.CreateComponent<SpriteRendererComponent>();

        return entity;
    }

    public Entity CreateLadder(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        var ladderComponent = entity.CreateComponent<LadderComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);

        ladderComponent.HitBox = new AxisAlignedRectangle(transform2DComponent.Translation, new Vector2(9, 9));

        return entity;
    }

    public Entity CreateBlueEnemy(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<BlueEnemyComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(15, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = BlueEnemyComponent.SpriteOffset;
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("31fbfb4d-988a-4382-85f9-f41c63bd4f27")));

        return entity;
    }

    public Entity CreateBlueEnemyDeathAnimation(Scene scene, Vector2 position)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<BlueEnemyDeathAnimationComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("31fbfb4d-988a-4382-85f9-f41c63bd4f27")));
        return entity;
    }

    public Entity CreateRedEnemy(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<RedEnemyComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(14, 15);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0.5, 4);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("457ab657-bfcd-4dfd-9542-018f5be53e03")));

        return entity;
    }

    public Entity CreateYellowEnemy(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        var yellowEnemyComponent = entity.CreateComponent<YellowEnemyComponent>();
        yellowEnemyComponent.NeutralSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("410b9b9c-0d87-4118-b711-75a51ada575e")));
        yellowEnemyComponent.AngrySprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("18a00a7d-c552-47a2-9b96-02e44b723a87")));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(18, 18);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = false;
        entity.CreateComponent<SpriteRendererComponent>();

        return entity;
    }

    public Entity CreateCamera(Scene scene)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<Transform2DComponent>();
        var cameraComponent = entity.CreateComponent<CameraComponent>();
        cameraComponent.ViewRectangle = GlobalSettings.ViewSize;
        entity.CreateComponent<CameraMovementComponent>();
        return entity;
    }
}