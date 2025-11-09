using System;
using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Core.Systems;

namespace SQ2.GamePlay.Common;

internal interface IRespawnable
{
    void Respawn();
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class RespawnService
{
    private readonly List<Action> _oneTimeRespawnActions = new();

    public void RequestRespawn()
    {
        RespawnRequested = true;
    }

    public void AddOneTimeRespawnAction(Action respawnAction)
    {
        _oneTimeRespawnActions.Add(respawnAction);
    }

    internal bool RespawnRequested { get; private set; }

    internal void Reset()
    {
        _oneTimeRespawnActions.Clear();
        RespawnRequested = false;
    }

    internal void HandleRespawn(Scene scene)
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

        foreach (var action in _oneTimeRespawnActions)
        {
            action();
        }

        _oneTimeRespawnActions.Clear();

        RespawnRequested = false;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class RespawnSystem : ICustomSystem
{
    private readonly ISceneManager _sceneManager;
    private readonly RespawnService _respawnService;

    public RespawnSystem(ISceneManager sceneManager, RespawnService respawnService)
    {
        _sceneManager = sceneManager;
        _respawnService = respawnService;
    }

    public string Name => "RespawnSystem";

    public void ProcessFixedUpdate()
    {
        if (_respawnService.RespawnRequested)
        {
            _respawnService.HandleRespawn(_sceneManager.CurrentScene);
        }
    }

    public void ProcessUpdate(GameTime gameTime)
    {
    }

    public void OnEntityCreated(Entity entity)
    {
    }

    public void OnEntityRemoved(Entity entity)
    {
    }

    public void OnEntityParentChanged(Entity entity, Entity? oldParent, Entity? newParent)
    {
    }

    public void OnComponentCreated(Component component)
    {
    }

    public void OnComponentRemoved(Component component)
    {
    }
}