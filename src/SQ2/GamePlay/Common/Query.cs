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
    private static Scene? _cachedScene;
    private static Transform2DComponent? _cachedPlayerTransform2DComponent;

    public static Transform2DComponent GetPlayerTransform2DComponentCached(Scene scene)
    {
        if (_cachedScene != scene)
        {
            _cachedScene = scene;
            _cachedPlayerTransform2DComponent = GetPlayerTransform2DComponent(scene);
        }

        return _cachedPlayerTransform2DComponent ?? throw new InvalidOperationException("Player transform not found.");
    }

    public static Transform2DComponent GetPlayerTransform2DComponent(Scene scene) =>
        scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<Transform2DComponent>();

    public static RectangleColliderComponent GetPlayerRectangleColliderComponent(Scene scene) =>
        scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<RectangleColliderComponent>();

    public static KinematicRigidBody2DComponent GetPlayerKinematicRigidBody2DComponent(Scene scene) =>
        scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<KinematicRigidBody2DComponent>();

    public static PlayerComponent GetPlayerComponent(Scene scene) =>
        scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<PlayerComponent>();

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
}