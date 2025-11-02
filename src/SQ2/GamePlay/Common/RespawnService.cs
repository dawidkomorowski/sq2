using System;
using System.Collections.Generic;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Common;

internal interface IRespawnable
{
    void Respawn();
}

internal static class RespawnService
{
    private static readonly List<Action> OneTimeRespawnActions = new();

    public static void Reset()
    {
        OneTimeRespawnActions.Clear();
    }

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

        foreach (var action in OneTimeRespawnActions)
        {
            action();
        }

        OneTimeRespawnActions.Clear();
    }

    public static void AddOneTimeRespawnAction(Action respawnAction)
    {
        OneTimeRespawnActions.Add(respawnAction);
    }
}