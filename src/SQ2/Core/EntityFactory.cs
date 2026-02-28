using System;
using Geisha.Engine.Animation;
using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.Development;
using SQ2.GamePlay.Boss.Bat;
using SQ2.GamePlay.Boss.Blue;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;
using SQ2.VFX;

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

    public Entity CreateCheckPoint(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        var checkPointComponent = entity.CreateComponent<CheckPointComponent>();
        checkPointComponent.ActiveSprite = _assetStore.GetAsset<Sprite>(assetId);
        checkPointComponent.InactiveSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("677b676e-d1c8-44c0-8c26-6ef0e5fcacbb"));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = checkPointComponent.InactiveSprite;
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
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
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("d7a11945-11f6-41a3-90fb-46dae40c6c56")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation("Walk");

        return entity;
    }

    public Entity CreateGeometry(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        entity.CreateComponent<TileColliderComponent>();

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        spriteTransform2DComponent.Rotation = orientation.GetRotation();
        spriteTransform2DComponent.Scale = orientation.GetScale() * 1.01;

        return entity;
    }

    public Entity CreateDecor(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation, string sortingLayerName, int layerIndex)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.SortingLayerName = sortingLayerName;
        spriteRendererComponent.OrderInLayer = layerIndex * 10; // Multiply by 10 to leave space for other entities in the same layer

        transform2DComponent.Rotation = orientation.GetRotation();
        transform2DComponent.Scale = orientation.GetScale() * 1.01;

        return entity;
    }

    public Entity CreateAnimatedDecor(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation, string sortingLayerName, int layerIndex)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.SortingLayerName = sortingLayerName;
        spriteRendererComponent.OrderInLayer = layerIndex * 10; // Multiply by 10 to leave space for other entities in the same layer
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Animation", _assetStore.GetAsset<SpriteAnimation>(assetId));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlayAnimation("Animation");

        transform2DComponent.Rotation = orientation.GetRotation();
        transform2DComponent.Scale = orientation.GetScale() * 1.01;

        return entity;
    }

    public Entity CreateWaterDeep(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<WaterDeepComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        entity.CreateComponent<TileColliderComponent>();

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Scale = new Vector2(1.01, 1.01);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
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
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        var collisionEntity = entity.CreateChildEntity();
        var collisionTransform2DComponent = collisionEntity.CreateComponent<Transform2DComponent>();
        collisionTransform2DComponent.Translation = new Vector2(0, -GlobalSettings.TileSize.Height / 4d);
        var rectangleColliderComponent = collisionEntity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width - 2, GlobalSettings.TileSize.Height / 2d - 2);

        transform2DComponent.Rotation = orientation.GetRotation();

        if (orientation.Flip is not Flip.None)
        {
            throw new InvalidOperationException("Flipping is not supported for spikes.");
        }

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
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.OrderInLayer = -1;

        return entity;
    }

    public Entity CreateMovingPlatform(Scene scene, double x, double y, double sx, double sy, double ex, double ey, int platformWidth)
    {
        var entity = scene.CreateEntity();
        var movingPlatformComponent = entity.CreateComponent<MovingPlatformComponent>();
        movingPlatformComponent.StartPosition = new Vector2(sx, sy) + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height * 0.75);
        movingPlatformComponent.EndPosition = new Vector2(ex, ey) + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height * 0.75);
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y) + new Vector2(GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height * 0.75);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width * platformWidth, GlobalSettings.TileSize.Height / 2);
        entity.CreateComponent<KinematicRigidBody2DComponent>();

        for (var i = 0; i < platformWidth; i++)
        {
            var spriteEntity = entity.CreateChildEntity();
            var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
            spriteTransform2DComponent.Translation = new Vector2
            (
                i * GlobalSettings.TileSize.Width - (platformWidth - 1) * 0.5 * GlobalSettings.TileSize.Width,
                -GlobalSettings.TileSize.Height / 4 + 0.5
            );
            var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
            spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("e3801ac8-1361-425b-9ca4-48f4fd4b3a4f"));
            spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
            spriteRendererComponent.OrderInLayer = -1;
        }

        return entity;
    }

    public Entity CreateVanishPlatform(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<VanishPlatformComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        entity.CreateComponent<TileColliderComponent>();
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.OrderInLayer = -1;

        return entity;
    }

    public Entity CreateJumpPad(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        var jumpPadComponent = entity.CreateComponent<JumpPadComponent>();
        jumpPadComponent.HighSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("afa34cd1-0643-4551-9a0d-76558521b9e7"));
        jumpPadComponent.LowSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("120bb5b7-2f24-4f72-902c-b516ac2334d7"));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty) + new Vector2(0, -2);
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(GlobalSettings.TileSize.Width, 14);

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, 2);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        return entity;
    }

    public Entity CreateLadder(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation)
    {
        var entity = scene.CreateEntity();
        var ladderComponent = entity.CreateComponent<LadderComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        ladderComponent.HitBox = new AxisAlignedRectangle(transform2DComponent.Translation, new Vector2(9, 9));

        if (orientation.Direction is not Direction.Up)
        {
            throw new InvalidOperationException("Rotation is not supported for ladders.");
        }

        transform2DComponent.Scale = orientation.GetScale() * 1.01;

        return entity;
    }

    public Entity CreateDestructibleWall(Scene scene, double x, double y, int buttonId)
    {
        var entity = scene.CreateEntity();
        var destructibleWallComponent = entity.CreateComponent<DestructibleWallComponent>();
        destructibleWallComponent.ButtonId = buttonId;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("9ba1d62b-7ffa-4732-a2c8-180f044281e4"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateButton(Scene scene, double x, double y, int objectId)
    {
        var entity = scene.CreateEntity();
        var buttonComponent = entity.CreateComponent<ButtonComponent>();
        buttonComponent.ObjectId = objectId;
        buttonComponent.PressedSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("22061613-dde7-4909-88f9-a0897673b9cd"));
        buttonComponent.ReleasedSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("7fa96723-2c98-4597-991d-1bbaf2fa43e5"));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y - 6);
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(15, 5);

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, 6);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        return entity;
    }

    public Entity CreateWallParticleBig(Scene scene, Vector2 position)
    {
        return CreateWallParticle(scene, position, AssetId.Parse("eb3b76a8-8e41-4229-9c4d-1e99849ca44e"));
    }

    public Entity CreateWallParticleSmall(Scene scene, Vector2 position)
    {
        return CreateWallParticle(scene, position, AssetId.Parse("62fb2ae4-b5df-42b3-928c-149bfc85cfc5"));
    }

    private Entity CreateWallParticle(Scene scene, Vector2 position, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<WallParticleComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.OrderInLayer = 1;
        return entity;
    }

    public Entity CreateKey(Scene scene, int tx, int ty, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<KeyComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        return entity;
    }

    public Entity CreateKeyHole(Scene scene, double x, double y, int keysRequired)
    {
        var entity = scene.CreateEntity();
        var keyHoleComponent = entity.CreateComponent<KeyHoleComponent>();
        keyHoleComponent.KeysRequired = keysRequired;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("b1535e09-96d5-4f20-9934-0204cb7a9abc"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateBlueEnemy(Scene scene, Vector2 position, MovementDirection initialMovementDirection, bool requireActivation, int activationGroup)
    {
        var entity = scene.CreateEntity();
        var blueEnemyComponent = entity.CreateComponent<BlueEnemyComponent>();
        blueEnemyComponent.InitialMovementDirection = initialMovementDirection;
        blueEnemyComponent.RequireActivation = requireActivation;
        blueEnemyComponent.ActivationGroup = activationGroup;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(15, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = BlueEnemyComponent.SpriteOffset;
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("be3e253b-a4b2-4662-8624-bf1a5b73ea74")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation("Walk");

        return entity;
    }

    public Entity CreateBlueEnemyDeathAnimation(Scene scene, Vector2 position, Vector2 scale)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<BlueEnemyDeathAnimationComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.Scale = scale;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("31fbfb4d-988a-4382-85f9-f41c63bd4f27"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        return entity;
    }

    public Entity CreateRedEnemy(Scene scene, Vector2 position, MovementDirection initialMovementDirection, bool requireActivation, int activationGroup)
    {
        var entity = scene.CreateEntity();
        var redEnemyComponent = entity.CreateComponent<RedEnemyComponent>();
        redEnemyComponent.InitialMovementDirection = initialMovementDirection;
        redEnemyComponent.RequireActivation = requireActivation;
        redEnemyComponent.ActivationGroup = activationGroup;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(14, 15);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0.5, 4);
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("724f4eb7-fbbd-4ebf-894f-8b354e16b69e")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation("Walk");

        return entity;
    }

    public Entity CreateYellowEnemy(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        var yellowEnemyComponent = entity.CreateComponent<YellowEnemyComponent>();
        yellowEnemyComponent.NeutralSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("410b9b9c-0d87-4118-b711-75a51ada575e"));
        yellowEnemyComponent.AngrySprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("18a00a7d-c552-47a2-9b96-02e44b723a87"));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(18, 18);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = false;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        return entity;
    }

    public Entity CreateFishEnemy(Scene scene, double x, double y, int jumpOffset)
    {
        var entity = scene.CreateEntity();
        var fishEnemyComponent = entity.CreateComponent<FishEnemyComponent>();
        fishEnemyComponent.JumpOffset = jumpOffset;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y) + new Vector2(9, 12);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(11, 15);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = false;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Idle", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("aab93162-61f9-4dac-80cc-780775b08dd6")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 2;
        spriteAnimationComponent.PlayAnimation("Idle");

        return entity;
    }

    public Entity CreateBatEnemy(Scene scene, double x, double y, double sx, double sy, double ex, double ey)
    {
        var entity = scene.CreateEntity();
        var batEnemyComponent = entity.CreateComponent<BatEnemyComponent>();
        batEnemyComponent.StartPosition = new Vector2(sx, sy) + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height / 2);
        batEnemyComponent.EndPosition = new Vector2(ex, ey) + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height / 2);
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y) + new Vector2(9, 12);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(13, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = false;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Fly", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("3a27bb5c-62a2-4f96-be5b-fbb176593312")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 2;
        spriteAnimationComponent.PlayAnimation("Fly");

        return entity;
    }

    public Entity CreateBat2Enemy(Scene scene, double x, double y)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<Bat2EnemyComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y) + new Vector2(9, 12);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(13, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Fly", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("3a27bb5c-62a2-4f96-be5b-fbb176593312")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 2;
        spriteAnimationComponent.PlayAnimation("Fly");

        return entity;
    }

    public Entity CreateBatBossTrigger(Scene scene, double x, double y, double width, double height)
    {
        var entity = scene.CreateEntity();
        var batBossTriggerComponent = entity.CreateComponent<BatBossTriggerComponent>();
        batBossTriggerComponent.TriggerArea = new AxisAlignedRectangle(new Vector2(x, y), new Vector2(width, height));
        return entity;
    }

    public Entity CreateBatBossSpawner
    (
        Scene scene, double x, double y,
        Vector2 targetPoint, double spawnAfterSeconds, double velocity,
        DropType drop, double dropAfterSeconds
    )
    {
        var entity = scene.CreateEntity();
        var batBossSpawnerComponent = entity.CreateComponent<BatBossSpawnerComponent>();
        batBossSpawnerComponent.SpawnPosition = new Vector2(x, y);
        batBossSpawnerComponent.TargetPoint = targetPoint + new Vector2(-GlobalSettings.TileSize.Width / 2, GlobalSettings.TileSize.Height / 2);
        batBossSpawnerComponent.SpawnAfterSeconds = spawnAfterSeconds;
        batBossSpawnerComponent.Velocity = velocity;
        batBossSpawnerComponent.Drop = drop;
        batBossSpawnerComponent.DropAfterSeconds = dropAfterSeconds;

        return entity;
    }

    public Entity CreateBatBoss
    (
        Scene scene, Vector2 spawnPosition, Vector2 targetPoint, double velocity,
        DropType drop, double dropAfterSeconds
    )
    {
        var entity = scene.CreateEntity();
        var batBossComponent = entity.CreateComponent<BatBossComponent>();
        batBossComponent.TargetPoint = targetPoint;
        batBossComponent.Velocity = velocity;
        batBossComponent.Drop = drop;
        batBossComponent.DropAfterSeconds = dropAfterSeconds;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = spawnPosition + new Vector2(9, 12);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(13, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = false;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Fly", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("3a27bb5c-62a2-4f96-be5b-fbb176593312")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 2;
        spriteAnimationComponent.PlayAnimation("Fly");

        var dropPreview = entity.CreateChildEntity();
        var dropPreviewTransform2DComponent = dropPreview.CreateComponent<Transform2DComponent>();
        dropPreviewTransform2DComponent.Translation = new Vector2(0, -9);
        dropPreviewTransform2DComponent.IsInterpolated = true;
        var dropPreviewSpriteRendererComponent = dropPreview.CreateComponent<SpriteRendererComponent>();
        dropPreviewSpriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        dropPreviewSpriteRendererComponent.OrderInLayer = 1;

        if (drop is DropType.BlueEnemy)
        {
            dropPreviewSpriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("31fbfb4d-988a-4382-85f9-f41c63bd4f27"));
        }

        return entity;
    }

    public Entity CreateBlueBoss(Scene scene, int tx, int ty)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<BlueBossComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = BlueBossComponent.ColliderDimensions1;
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = false;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = BlueBossComponent.SpriteOffset1;
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.OrderInLayer = 1;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation(BlueBossComponent.Animations.Walk,
            _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("93ef2825-ca47-4ff3-81e6-ff23be30dbdd")));
        spriteAnimationComponent.AddAnimation(BlueBossComponent.Animations.Shoot,
            _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("c6ace0bd-53db-4614-9743-5a4f117df266")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation(BlueBossComponent.Animations.Walk);

        AddSpike("LeftSpike1", new Vector2(-16, 2), Math.PI / 2);
        AddSpike("LeftSpike2", new Vector2(-16, -6), Math.PI / 2);

        AddSpike("RightSpike1", new Vector2(16, 2), -Math.PI / 2);
        AddSpike("RightSpike2", new Vector2(16, -6), -Math.PI / 2);

        AddSpike("TopLeftSpike", new Vector2(-4, 13), 0);
        AddSpike("TopRightSpike", new Vector2(4, 13), 0);

        AddSpike("DiagonalSpike1", new Vector2(-13.5, 10.5), Math.PI / 4);
        AddSpike("DiagonalSpike2", new Vector2(13.5, 10.5), -Math.PI / 4);

        return entity;

        void AddSpike(string name, Vector2 position, double rotation)
        {
            var spikeEntity = spriteEntity.CreateChildEntity();
            spikeEntity.Name = name;
            var spikeTransform2DComponent = spikeEntity.CreateComponent<Transform2DComponent>();
            spikeTransform2DComponent.Translation = position;
            spikeTransform2DComponent.Rotation = rotation;
            spikeTransform2DComponent.Scale = new Vector2(0.5, 0.5);
            var spikeSpriteRendererComponent = spikeEntity.CreateComponent<SpriteRendererComponent>();
            spikeSpriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("56abfaa2-eb0a-45e1-8831-179ea209155c"));
            spikeSpriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        }
    }

    public Entity CreateBlueBossDeathAnimation(Scene scene, Vector2 position, Vector2 scale)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<BlueEnemyDeathAnimationComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.Scale = scale;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("eb27541a-95e1-4a46-b49d-24c7c967d329"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        return entity;
    }

    public Entity CreateBlueBossProjectile(Scene scene, Vector2 position, double rotation, Vector2 direction)
    {
        var entity = scene.CreateEntity();
        var blueBossProjectileComponent = entity.CreateComponent<BlueBossProjectileComponent>();
        blueBossProjectileComponent.Direction = direction;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.Rotation = rotation;
        transform2DComponent.Scale = new Vector2(0.5, 0.5);
        transform2DComponent.IsInterpolated = true;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.OrderInLayer = 1;
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("56abfaa2-eb0a-45e1-8831-179ea209155c"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        return entity;
    }

    public Entity CreateRaisingWater(Scene scene, double centerX, double minY, double maxY, double width, double height, double velocity, double delay)
    {
        var entity = scene.CreateEntity();
        var raisingWaterComponent = entity.CreateComponent<RaisingWaterComponent>();
        raisingWaterComponent.Velocity = velocity;
        raisingWaterComponent.Delay = delay;
        raisingWaterComponent.Dimensions = new Vector2(width, height);
        raisingWaterComponent.MaxY = maxY - height / 2;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(centerX, minY - height / 2);
        transform2DComponent.IsInterpolated = true;

        for (var x = -width / 2; x < width / 2; x += GlobalSettings.TileSize.Width)
        {
            AddWaterSurface(x + GlobalSettings.TileSize.Width / 2, height / 2 - GlobalSettings.TileSize.Height / 2);
        }

        for (var x = -width / 2; x < width / 2; x += GlobalSettings.TileSize.Width)
        {
            for (var y = height / 2 - GlobalSettings.TileSize.Height; y > -height / 2; y -= GlobalSettings.TileSize.Height)
            {
                AddWaterBody(x + GlobalSettings.TileSize.Width / 2, y - GlobalSettings.TileSize.Height / 2);
            }
        }

        return entity;

        void AddWaterSurface(double x, double y)
        {
            var waterSurfaceEntity = entity.CreateChildEntity();
            var waterSurfaceTransform = waterSurfaceEntity.CreateComponent<Transform2DComponent>();
            waterSurfaceTransform.Translation = new Vector2(x, y);
            waterSurfaceTransform.Scale = new Vector2(1.01, 1.01);
            var spriteRendererComponent = waterSurfaceEntity.CreateComponent<SpriteRendererComponent>();
            spriteRendererComponent.OrderInLayer = 1000;
            spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
            var waterSurfaceAnimation = waterSurfaceEntity.CreateComponent<SpriteAnimationComponent>();
            waterSurfaceAnimation.AddAnimation("WaterSurface",
                _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("22769c0b-b6e0-4595-a2ae-46724a430fab")));
            waterSurfaceAnimation.PlayInLoop = true;
            waterSurfaceAnimation.PlayAnimation("WaterSurface");
        }

        void AddWaterBody(double x, double y)
        {
            var waterBodyEntity = entity.CreateChildEntity();
            var waterBodyTransform = waterBodyEntity.CreateComponent<Transform2DComponent>();
            waterBodyTransform.Translation = new Vector2(x, y);
            waterBodyTransform.Scale = new Vector2(1.01, 1.01);
            var spriteRendererComponent = waterBodyEntity.CreateComponent<SpriteRendererComponent>();
            spriteRendererComponent.OrderInLayer = 1000;
            spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("325dd237-4a19-49d7-ac94-87c13287c4d7"));
            spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        }
    }

    public Entity CreateBackground(Scene scene, Background background)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<Transform2DComponent>();
        var backgroundComponent = entity.CreateComponent<BackgroundComponent>();

        switch (background)
        {
            case Background.Default:
            {
                backgroundComponent.UpperLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("f809d672-5096-4833-8b6a-4bee43a075ee"));
                backgroundComponent.UpperRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("99c8f050-2dd0-4b09-b44e-1688b3753237"));
                backgroundComponent.MiddleLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("c5584c00-090b-4156-a240-2a299ea08583"));
                backgroundComponent.MiddleRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("701b213a-1358-4414-ae20-ff9965ea32f0"));
                backgroundComponent.LowerLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("33605045-173a-46ec-becf-2627704e2b19"));
                backgroundComponent.LowerRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("beae4927-27f3-474e-854c-da15f35f34b2"));
                break;
            }
            case Background.Winter:
                backgroundComponent.UpperLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("1d0a7e73-1e90-422a-9703-0e8700a2de5f"));
                backgroundComponent.UpperRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("ad3edb34-152f-4145-ba70-669f805b4dcf"));
                backgroundComponent.MiddleLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("8fb394fb-e496-43f4-a8de-2d40f0c493ab"));
                backgroundComponent.MiddleRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("15a813ab-d61d-477a-b967-3fe1b8ca31bc"));
                backgroundComponent.LowerLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("5fde8aa8-0c97-4f23-8c2b-353e58a32bb0"));
                backgroundComponent.LowerRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("823769f9-e3b3-45e2-a2fd-f2a9d4a24111"));
                break;
            case Background.Desert:
                backgroundComponent.UpperLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("e7578873-4ce9-4587-a30e-11b8b32debec"));
                backgroundComponent.UpperRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("e22e9b3d-5edf-432c-8265-cceccf9da82f"));
                backgroundComponent.MiddleLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("7d2fd7b8-f340-4e33-90ab-8cbdb972fd22"));
                backgroundComponent.MiddleRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("813da2ea-44ff-4bc1-b385-f69d9655f22a"));
                backgroundComponent.LowerLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("ebe80a7b-6623-41d7-84b7-3293f2f9c37b"));
                backgroundComponent.LowerRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("a02506b8-48b3-44c7-976b-62b9118f4192"));
                break;
            case Background.Forest:
                backgroundComponent.UpperLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("1874af1e-f2b6-47de-8b45-655ca0fb1c99"));
                backgroundComponent.UpperRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("c8affdad-d36b-4657-b95d-9a7e16b17488"));
                backgroundComponent.MiddleLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("dcaf0105-3b16-4563-8344-eaa2803c3726"));
                backgroundComponent.MiddleRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("479b01d4-d48b-4bea-a81e-5cd2bed34dc1"));
                backgroundComponent.LowerLeftSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("881d5acc-b063-473d-9b90-24047f3f6dd0"));
                backgroundComponent.LowerRightSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("7bd94eb9-4c1e-42f4-ae42-5bcd172e3688"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(background), background, null);
        }

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