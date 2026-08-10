using System;
using System.Linq;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.GamePlay.PauseMenu;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Common;

internal static class Query
{
    // TODO: Consider caching other queries as well.
    private static readonly PlayerQueryCache PlayerQueryCacheInstance = new();

    public static Transform2DComponent GetPlayerTransformComponent(Scene scene)
    {
        PlayerQueryCacheInstance.RefreshIfStale(scene);
        return PlayerQueryCacheInstance.Transform;
    }

    public static RectangleColliderComponent GetPlayerColliderComponent(Scene scene)
    {
        PlayerQueryCacheInstance.RefreshIfStale(scene);
        return PlayerQueryCacheInstance.Collider;
    }

    public static KinematicRigidBody2DComponent GetPlayerRigidBodyComponent(Scene scene)
    {
        PlayerQueryCacheInstance.RefreshIfStale(scene);
        return PlayerQueryCacheInstance.RigidBody;
    }

    public static PlayerComponent GetPlayerComponent(Scene scene)
    {
        PlayerQueryCacheInstance.RefreshIfStale(scene);
        return PlayerQueryCacheInstance.Player;
    }

    public static CameraMovementComponent GetCameraMovementComponent(Scene scene) =>
        scene.RootEntities.Single(e => e.HasComponent<CameraMovementComponent>()).GetComponent<CameraMovementComponent>();

    public static CinematicCameraComponent GetCinematicCameraComponent(Scene scene) =>
        scene.RootEntities.Single(e => e.HasComponent<CinematicCameraComponent>()).GetComponent<CinematicCameraComponent>();

    public static PauseMenuComponent GetPauseMenuComponent(Scene scene) =>
        scene.AllEntities.Single(e => e.Name == GlobalSettings.SpecialEntities.UIRoot)
            .GetChildrenRecursively().Single(e => e.HasComponent<PauseMenuComponent>()).GetComponent<PauseMenuComponent>();

    public static bool TileHitTest(Scene scene, int tx, int ty)
    {
        var worldPosition = Geometry.GetWorldCoordinates(tx, ty);
        foreach (var entity in scene.RootEntities)
        {
            if (!entity.HasComponent<TileColliderComponent>()) continue;

            var tileCollider = entity.GetComponent<TileColliderComponent>();
            if (tileCollider.BoundingBox.Contains(worldPosition))
            {
                return true;
            }
        }

        return false;
    }

    private class PlayerQueryCache
    {
        private Scene? _cachedScene;
        private Transform2DComponent? _cachedTransform;
        private RectangleColliderComponent? _cachedCollider;
        private KinematicRigidBody2DComponent? _cachedRigidBody;
        private PlayerComponent? _cachedPlayer;

        public Transform2DComponent Transform => _cachedTransform ?? throw new InvalidOperationException("Player transform not found.");
        public RectangleColliderComponent Collider => _cachedCollider ?? throw new InvalidOperationException("Player collider not found.");
        public KinematicRigidBody2DComponent RigidBody => _cachedRigidBody ?? throw new InvalidOperationException("Player rigid body not found.");
        public PlayerComponent Player => _cachedPlayer ?? throw new InvalidOperationException("Player not found.");

        public void RefreshIfStale(Scene scene)
        {
            if (_cachedScene == scene) return;

            _cachedScene = scene;

            var playerEntity = scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>());

            _cachedTransform = playerEntity.GetComponent<Transform2DComponent>();
            _cachedCollider = playerEntity.GetComponent<RectangleColliderComponent>();
            _cachedRigidBody = playerEntity.GetComponent<KinematicRigidBody2DComponent>();
            _cachedPlayer = playerEntity.GetComponent<PlayerComponent>();
        }
    }
}