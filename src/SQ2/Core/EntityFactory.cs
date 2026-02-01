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
using SQ2.GamePlay.Boss.Blue;
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
        checkPointComponent.InactiveSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("677b676e-d1c8-44c0-8c26-6ef0e5fcacbb")));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = checkPointComponent.InactiveSprite;
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
        spriteEntity.CreateComponent<SpriteRendererComponent>();
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("d7a11945-11f6-41a3-90fb-46dae40c6c56"))));
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

        spriteTransform2DComponent.Rotation = orientation.GetRotation();
        spriteTransform2DComponent.Scale = orientation.GetScale();

        return entity;
    }

    public Entity CreateDecor(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation, string sortingLayerName, int layerIndex)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.SortingLayerName = sortingLayerName;
        spriteRendererComponent.OrderInLayer = layerIndex * 10; // Multiply by 10 to leave space for other entities in the same layer

        transform2DComponent.Rotation = orientation.GetRotation();
        transform2DComponent.Scale = orientation.GetScale();

        return entity;
    }

    public Entity CreateAnimatedDecor(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation, string sortingLayerName, int layerIndex)
    {
        var entity = scene.CreateEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.SortingLayerName = sortingLayerName;
        spriteRendererComponent.OrderInLayer = layerIndex * 10; // Multiply by 10 to leave space for other entities in the same layer
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Animation", _assetStore.GetAsset<SpriteAnimation>(assetId));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlayAnimation("Animation");

        transform2DComponent.Rotation = orientation.GetRotation();
        transform2DComponent.Scale = orientation.GetScale();

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
            spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("e3801ac8-1361-425b-9ca4-48f4fd4b3a4f")));
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

    public Entity CreateLadder(Scene scene, int tx, int ty, AssetId assetId, Orientation orientation)
    {
        var entity = scene.CreateEntity();
        var ladderComponent = entity.CreateComponent<LadderComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = Geometry.GetWorldCoordinates(tx, ty);
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);

        ladderComponent.HitBox = new AxisAlignedRectangle(transform2DComponent.Translation, new Vector2(9, 9));

        if (orientation.Direction is not Direction.Up)
        {
            throw new InvalidOperationException("Rotation is not supported for ladders.");
        }

        transform2DComponent.Scale = orientation.GetScale();

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
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("9ba1d62b-7ffa-4732-a2c8-180f044281e4")));
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateButton(Scene scene, double x, double y, int objectId)
    {
        var entity = scene.CreateEntity();
        var buttonComponent = entity.CreateComponent<ButtonComponent>();
        buttonComponent.ObjectId = objectId;
        buttonComponent.PressedSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("22061613-dde7-4909-88f9-a0897673b9cd")));
        buttonComponent.ReleasedSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("7fa96723-2c98-4597-991d-1bbaf2fa43e5")));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y - 6);
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(15, 5);

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = new Vector2(0, 6);
        spriteEntity.CreateComponent<SpriteRendererComponent>();
        return entity;
    }

    public Entity CreateWallParticleBig(Scene scene, Vector2 position)
    {
        return CreateWallParticle(scene, position, new AssetId(new Guid("eb3b76a8-8e41-4229-9c4d-1e99849ca44e")));
    }

    public Entity CreateWallParticleSmall(Scene scene, Vector2 position)
    {
        return CreateWallParticle(scene, position, new AssetId(new Guid("62fb2ae4-b5df-42b3-928c-149bfc85cfc5")));
    }

    private Entity CreateWallParticle(Scene scene, Vector2 position, AssetId assetId)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<WallParticleComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
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
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("b1535e09-96d5-4f20-9934-0204cb7a9abc")));
        entity.CreateComponent<TileColliderComponent>();
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
        spriteEntity.CreateComponent<SpriteRendererComponent>();
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("be3e253b-a4b2-4662-8624-bf1a5b73ea74"))));
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
        spriteEntity.CreateComponent<SpriteRendererComponent>();
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("724f4eb7-fbbd-4ebf-894f-8b354e16b69e"))));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation("Walk");

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
        entity.CreateComponent<SpriteRendererComponent>();
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Idle", _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("aab93162-61f9-4dac-80cc-780775b08dd6"))));
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
        entity.CreateComponent<SpriteRendererComponent>();
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Fly", _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("3a27bb5c-62a2-4f96-be5b-fbb176593312"))));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 2;
        spriteAnimationComponent.PlayAnimation("Fly");

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
        spriteRendererComponent.OrderInLayer = 1;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation(BlueBossComponent.Animations.Walk,
            _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("93ef2825-ca47-4ff3-81e6-ff23be30dbdd"))));
        spriteAnimationComponent.AddAnimation(BlueBossComponent.Animations.Shoot,
            _assetStore.GetAsset<SpriteAnimation>(new AssetId(new Guid("c6ace0bd-53db-4614-9743-5a4f117df266"))));
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
            spikeSpriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("56abfaa2-eb0a-45e1-8831-179ea209155c")));
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
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("eb27541a-95e1-4a46-b49d-24c7c967d329")));
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
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("56abfaa2-eb0a-45e1-8831-179ea209155c")));
        return entity;
    }

    public Entity CreateBackground(Scene scene)
    {
        var entity = scene.CreateEntity();
        var backgroundComponent = entity.CreateComponent<BackgroundComponent>();
        backgroundComponent.UpperLeftSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("f809d672-5096-4833-8b6a-4bee43a075ee")));
        backgroundComponent.UpperRightSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("99c8f050-2dd0-4b09-b44e-1688b3753237")));
        backgroundComponent.MiddleLeftSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("c5584c00-090b-4156-a240-2a299ea08583")));
        backgroundComponent.MiddleRightSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("701b213a-1358-4414-ae20-ff9965ea32f0")));
        backgroundComponent.LowerLeftSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("33605045-173a-46ec-becf-2627704e2b19")));
        backgroundComponent.LowerRightSprite = _assetStore.GetAsset<Sprite>(new AssetId(new Guid("beae4927-27f3-474e-854c-da15f35f34b2")));
        entity.CreateComponent<Transform2DComponent>();
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