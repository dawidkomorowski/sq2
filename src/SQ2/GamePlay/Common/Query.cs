using System.Linq;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Common;

internal static class Query
{
    public static Transform2DComponent GetPlayerTransform2DComponent(Scene scene)
    {
        return scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<Transform2DComponent>();
    }

    public static RectangleColliderComponent GetPlayerRectangleColliderComponent(Scene scene)
    {
        return scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<RectangleColliderComponent>();
    }

    public static KinematicRigidBody2DComponent GetPlayerKinematicRigidBody2DComponent(Scene scene)
    {
        return scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<KinematicRigidBody2DComponent>();
    }

    public static PlayerComponent GetPlayerComponent(Scene scene)
    {
        return scene.RootEntities.Single(e => e.HasComponent<PlayerComponent>()).GetComponent<PlayerComponent>();
    }

    public static bool TileHitTest(Scene scene, int tx, int ty)
    {
        var worldPosition = Geometry.GetWorldCoordinates(tx, ty);
        foreach (var entity in scene.RootEntities)
        {
            if (!entity.HasComponent<TileColliderComponent>() || !entity.HasComponent<Transform2DComponent>()) continue;
            var transform2DComponent = entity.GetComponent<Transform2DComponent>();
            var aabb = new AxisAlignedRectangle(transform2DComponent.Translation, new Vector2(GlobalSettings.TileSize.Width, GlobalSettings.TileSize.Height));
            if (aabb.Contains(worldPosition))
            {
                return true;
            }
        }

        return false;
    }
}