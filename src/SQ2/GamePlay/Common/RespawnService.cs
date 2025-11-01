using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Common;

internal interface IRespawnable
{
    void Respawn();
}

internal static class RespawnService
{
    public static void RespawnAll(Scene scene)
    {
        foreach (var entity in scene.AllEntities)
        {
            foreach (var component in entity.Components)
            {
                if (component is IRespawnable respawnable)
                {
                    respawnable.Respawn();
                }
            }
        }
    }
}