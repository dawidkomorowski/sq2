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
using SQ2.GamePlay.Collectibles;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;
using SQ2.UI;
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

    public Entity CreatePlayerSpawnPoint(Scene scene, Vector2 position)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<PlayerSpawnPointComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreatePlayer(Scene scene, Vector2 position)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<PlayerComponent>();
        entity.CreateComponent<InputComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreateMovingPlatform(Scene scene, Vector2 position, Vector2 startPosition, Vector2 endPosition, int platformWidth)
    {
        var entity = scene.CreateEntity();
        var movingPlatformComponent = entity.CreateComponent<MovingPlatformComponent>();
        movingPlatformComponent.StartPosition = startPosition;
        movingPlatformComponent.EndPosition = endPosition;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreateDestructibleWall(Scene scene, Vector2 position, int buttonId)
    {
        var entity = scene.CreateEntity();
        var destructibleWallComponent = entity.CreateComponent<DestructibleWallComponent>();
        destructibleWallComponent.ButtonId = buttonId;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("9ba1d62b-7ffa-4732-a2c8-180f044281e4"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateButton(Scene scene, Vector2 position, int objectId)
    {
        var entity = scene.CreateEntity();
        var buttonComponent = entity.CreateComponent<ButtonComponent>();
        buttonComponent.ObjectId = objectId;
        buttonComponent.PressedSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("22061613-dde7-4909-88f9-a0897673b9cd"));
        buttonComponent.ReleasedSprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("7fa96723-2c98-4597-991d-1bbaf2fa43e5"));
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreateKeyHole(Scene scene, Vector2 position, int keysRequired)
    {
        var entity = scene.CreateEntity();
        var keyHoleComponent = entity.CreateComponent<KeyHoleComponent>();
        keyHoleComponent.KeysRequired = keysRequired;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("b1535e09-96d5-4f20-9934-0204cb7a9abc"));
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        entity.CreateComponent<TileColliderComponent>();
        return entity;
    }

    public Entity CreateDoor(Scene scene, Vector2 position, AssetId assetId, int objectId, int exitObjectId, bool updateCameraPosition)
    {
        var entity = scene.CreateEntity();
        var doorComponent = entity.CreateComponent<DoorComponent>();
        doorComponent.ObjectId = objectId;
        doorComponent.ExitObjectId = exitObjectId;
        doorComponent.UpdateCameraPosition = updateCameraPosition;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        var childEntity = entity.CreateChildEntity();
        var childTransform2DComponent = childEntity.CreateComponent<Transform2DComponent>();
        childTransform2DComponent.Translation = new Vector2(0, GlobalSettings.TileSize.Height);
        childTransform2DComponent.Scale = new Vector2(1, 1.02);
        var childSpriteRendererComponent = childEntity.CreateComponent<SpriteRendererComponent>();
        childSpriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("d71a0d14-7955-4e95-8a68-efe124ee8cbc"));
        childSpriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        return entity;
    }

    public Entity CreateCoin(Scene scene, Vector2 position)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<CoinComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Spin", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("0ea11568-b514-468d-8ada-2b7bd9bad1fb")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 2;
        spriteAnimationComponent.PlayAnimation("Spin");
        return entity;
    }

    public Entity CreateDiamond(Scene scene, Vector2 position)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<DiamondComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fd94100c-a1fd-4c5b-bc5c-a7337ca94f02"));
        return entity;
    }

    public Entity CreateBlueEnemy(Scene scene, Vector2 position, MovementDirection initialMovementDirection, bool requireActivation, int activationGroup)
    {
        var entity = scene.CreateEntity();
        var walkingEnemyComponent = entity.CreateComponent<WalkingEnemyComponent>();
        walkingEnemyComponent.InitialMovementDirection = initialMovementDirection;
        walkingEnemyComponent.RequireActivation = requireActivation;
        walkingEnemyComponent.ActivationGroup = activationGroup;
        walkingEnemyComponent.Type = WalkingEnemyComponent.WalkingEnemyType.Blue;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(15, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = WalkingEnemyComponent.SpriteOffset;
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("be3e253b-a4b2-4662-8624-bf1a5b73ea74")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation("Walk");

        return entity;
    }

    public Entity CreateGreenEnemy(Scene scene, Vector2 position, MovementDirection initialMovementDirection, bool requireActivation, int activationGroup,
        double minX, double maxX)
    {
        var entity = scene.CreateEntity();
        var walkingEnemyComponent = entity.CreateComponent<WalkingEnemyComponent>();
        walkingEnemyComponent.InitialMovementDirection = initialMovementDirection;
        walkingEnemyComponent.RequireActivation = requireActivation;
        walkingEnemyComponent.ActivationGroup = activationGroup;
        walkingEnemyComponent.Type = WalkingEnemyComponent.WalkingEnemyType.Green;
        walkingEnemyComponent.MinX = minX;
        walkingEnemyComponent.MaxX = maxX;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.IsInterpolated = true;
        var rectangleColliderComponent = entity.CreateComponent<RectangleColliderComponent>();
        rectangleColliderComponent.Dimensions = new Vector2(15, 13);
        var kinematicRigidBody2DComponent = entity.CreateComponent<KinematicRigidBody2DComponent>();
        kinematicRigidBody2DComponent.EnableCollisionResponse = true;

        var spriteEntity = entity.CreateChildEntity();
        var spriteTransform2DComponent = spriteEntity.CreateComponent<Transform2DComponent>();
        spriteTransform2DComponent.Translation = WalkingEnemyComponent.SpriteOffset;
        var spriteRendererComponent = spriteEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = spriteEntity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("0c56aef6-8858-4710-a4bb-687143593673")));
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 3;
        spriteAnimationComponent.PlayAnimation("Walk");

        return entity;
    }

    public Entity CreateWalkingEnemyDeathAnimation(Scene scene, Vector2 position, Vector2 scale, WalkingEnemyComponent.WalkingEnemyType type)
    {
        var assetId = type switch
        {
            WalkingEnemyComponent.WalkingEnemyType.Blue => AssetId.Parse("31fbfb4d-988a-4382-85f9-f41c63bd4f27"),
            WalkingEnemyComponent.WalkingEnemyType.Green => AssetId.Parse("7fe4fd22-c9b2-4fe8-86c2-6949df3eb5f8"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var entity = scene.CreateEntity();
        entity.CreateComponent<WalkingEnemyDeathAnimationComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        transform2DComponent.Scale = scale;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = _assetStore.GetAsset<Sprite>(assetId);
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        return entity;
    }

    public Entity CreateRedEnemy(Scene scene, Vector2 position, MovementDirection initialMovementDirection, bool requireActivation, int activationGroup)
    {
        var entity = scene.CreateEntity();
        var spikeEnemyComponent = entity.CreateComponent<WalkingSpikeEnemyComponent>();
        spikeEnemyComponent.InitialMovementDirection = initialMovementDirection;
        spikeEnemyComponent.RequireActivation = requireActivation;
        spikeEnemyComponent.ActivationGroup = activationGroup;
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

    public Entity CreateYellowWalkingEnemy(Scene scene, Vector2 position, MovementDirection initialMovementDirection, bool requireActivation,
        int activationGroup, double minX, double maxX)
    {
        var entity = scene.CreateEntity();
        var spikeEnemyComponent = entity.CreateComponent<WalkingSpikeEnemyComponent>();
        spikeEnemyComponent.InitialMovementDirection = initialMovementDirection;
        spikeEnemyComponent.RequireActivation = requireActivation;
        spikeEnemyComponent.ActivationGroup = activationGroup;
        spikeEnemyComponent.MinX = minX;
        spikeEnemyComponent.MaxX = maxX;
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
        spriteAnimationComponent.AddAnimation("Walk", _assetStore.GetAsset<SpriteAnimation>(AssetId.Parse("f2cbe445-e292-4169-ab8d-2aa9384d42e0")));
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

    public Entity CreateFishEnemy(Scene scene, Vector2 position, int jumpOffset)
    {
        var entity = scene.CreateEntity();
        var fishEnemyComponent = entity.CreateComponent<FishEnemyComponent>();
        fishEnemyComponent.JumpOffset = jumpOffset;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreateBatEnemy(Scene scene, Vector2 position, Vector2 startPosition, Vector2 endPosition)
    {
        var entity = scene.CreateEntity();
        var batEnemyComponent = entity.CreateComponent<BatEnemyComponent>();
        batEnemyComponent.StartPosition = startPosition;
        batEnemyComponent.EndPosition = endPosition;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreateBat2Enemy(Scene scene, Vector2 position)
    {
        var entity = scene.CreateEntity();
        entity.CreateComponent<Bat2EnemyComponent>();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
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

    public Entity CreateBatBossTrigger(Scene scene, Vector2 center, SizeD size, double timerStartValue)
    {
        var entity = scene.CreateEntity();
        var batBossTriggerComponent = entity.CreateComponent<BatBossTriggerComponent>();
        batBossTriggerComponent.TriggerArea = new AxisAlignedRectangle(center, size);
        batBossTriggerComponent.TimerStartValue = timerStartValue;
        return entity;
    }

    public Entity CreateBatBossSpawner
    (
        Scene scene, Vector2 spawnPosition,
        Vector2 targetPoint, double spawnAfterSeconds, double velocity,
        DropType drop, double dropAfterSeconds
    )
    {
        var entity = scene.CreateEntity();
        var batBossSpawnerComponent = entity.CreateComponent<BatBossSpawnerComponent>();
        batBossSpawnerComponent.SpawnPosition = spawnPosition;
        batBossSpawnerComponent.TargetPoint = targetPoint;
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
        transform2DComponent.Translation = spawnPosition;
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
        entity.CreateComponent<WalkingEnemyDeathAnimationComponent>();
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
        var halfWidth = width / 2;
        var halfHeight = height / 2;

        var entity = scene.CreateEntity();
        var raisingWaterComponent = entity.CreateComponent<RaisingWaterComponent>();
        raisingWaterComponent.Velocity = velocity;
        raisingWaterComponent.Delay = delay;
        raisingWaterComponent.Dimensions = new Vector2(width, height);
        raisingWaterComponent.MaxY = maxY - halfHeight;
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(centerX, minY - halfHeight);
        transform2DComponent.IsInterpolated = true;

        for (var x = -halfWidth; x < halfWidth; x += GlobalSettings.TileSize.Width)
        {
            AddWaterSurface(x + GlobalSettings.TileSize.Width / 2, halfHeight - GlobalSettings.TileSize.Height / 2);
        }

        for (var x = -halfWidth; x < width / 2; x += GlobalSettings.TileSize.Width)
        {
            for (var y = halfHeight - GlobalSettings.TileSize.Height; y > -halfHeight; y -= GlobalSettings.TileSize.Height)
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

    public Entity CreateLevelCompleteTrigger(Scene scene, Vector2 center, SizeD size, LevelCompleteDirection direction)
    {
        var entity = scene.CreateEntity();
        var levelCompleteTriggerComponent = entity.CreateComponent<LevelCompleteTriggerComponent>();
        levelCompleteTriggerComponent.TriggerArea = new AxisAlignedRectangle(center, size);
        levelCompleteTriggerComponent.LevelCompleteDirection = direction;
        return entity;
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
        entity.CreateComponent<CinematicCameraComponent>();
        return entity;
    }

    public Entity CreateUI_CoinCounter(Entity parent, double x, double y)
    {
        var entity = parent.CreateChildEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(x, y);

        var numberEntity = entity.CreateChildEntity();
        var numberTransform = numberEntity.CreateComponent<Transform2DComponent>();
        numberTransform.Translation = new Vector2(-10, 0);
        var numberRendererComponent = numberEntity.CreateComponent<NumberRendererComponent>();
        numberRendererComponent.Digit0Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("7b973a46-3c17-4d6c-a20a-97214db7086a"));
        numberRendererComponent.Digit1Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("19398cc8-0a86-4aee-9e82-0a95bb4317c7"));
        numberRendererComponent.Digit2Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("71fa41ca-2162-47de-a7b1-de7462bd1cb0"));
        numberRendererComponent.Digit3Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("aacd2f7c-5145-4b8c-b7f6-1342e3a7d8ad"));
        numberRendererComponent.Digit4Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("72126ad3-6cd7-4c39-b3b7-266bb614686d"));
        numberRendererComponent.Digit5Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("9eeb80fa-485b-4f21-8db9-0a3412796863"));
        numberRendererComponent.Digit6Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("55a98703-d4e5-4f7e-b15e-b2a169cb1b93"));
        numberRendererComponent.Digit7Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("6fd29513-cf28-4678-aeb6-40d2a93c8817"));
        numberRendererComponent.Digit8Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("2db0e507-4e38-4d61-8c65-aca6aba26a20"));
        numberRendererComponent.Digit9Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("74fe02bb-b7fc-4f9d-882f-fe584de17c26"));

        var timesIconEntity = entity.CreateChildEntity();
        var timesIconTransform = timesIconEntity.CreateComponent<Transform2DComponent>();
        timesIconTransform.Translation = new Vector2(20, 0);
        var timesIconSpriteRenderer = timesIconEntity.CreateComponent<SpriteRendererComponent>();
        timesIconSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("fa82409e-2840-4492-bc9b-4b48e6030430"));
        timesIconSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        var coinIconEntity = entity.CreateChildEntity();
        var coinIconTransform = coinIconEntity.CreateComponent<Transform2DComponent>();
        coinIconTransform.Translation = new Vector2(40, 0);
        var coinIconSpriteRenderer = coinIconEntity.CreateComponent<SpriteRendererComponent>();
        coinIconSpriteRenderer.Sprite = _assetStore.GetAsset<Sprite>(AssetId.Parse("eb06525a-acc2-4593-abc9-506763f077cc"));
        coinIconSpriteRenderer.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;

        return entity;
    }
}