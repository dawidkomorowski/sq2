using Geisha.Engine.Core;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Core.Systems;
using SQ2.Development;
using System.Collections.Generic;

namespace SQ2.GamePlay.Common;

internal interface IProximityActivatable
{
    Vector2 Position { get; }
    int ActivationGroup { get; }
    bool Active { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ProximityActivationService
{
    private const double ProximityActivationDistance = 250.0;
    private readonly List<IProximityActivatable> _proximityActivatableList = new();
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.ProximityActivation;
    private readonly IDebugRenderer _debugRenderer;

    public ProximityActivationService(IDebugRenderer debugRenderer)
    {
        _debugRenderer = debugRenderer;
    }

    public void Register(IProximityActivatable proximityActivatable)
    {
        _proximityActivatableList.Add(proximityActivatable);
    }

    public void Unregister(IProximityActivatable proximityActivatable)
    {
        _proximityActivatableList.Remove(proximityActivatable);
    }

    public void ResetActivations()
    {
        foreach (var proximityActivatable in _proximityActivatableList)
        {
            proximityActivatable.Active = false;
        }
    }

    internal void UpdateActivations(Vector2 playerPosition)
    {
        foreach (var proximityActivatable in _proximityActivatableList)
        {
            if (proximityActivatable.Active) continue;

            var distanceToPlayer = proximityActivatable.Position.Distance(playerPosition);
            if (distanceToPlayer < ProximityActivationDistance)
            {
                proximityActivatable.Active = true;

                if (proximityActivatable.ActivationGroup != 0)
                {
                    foreach (var activatable in _proximityActivatableList)
                    {
                        if (activatable.ActivationGroup == proximityActivatable.ActivationGroup)
                        {
                            activatable.Active = true;
                        }
                    }
                }
            }
        }
    }

    internal void DebugDraw()
    {
        if (!_enableDebugDraw) return;
        foreach (var proximityActivatable in _proximityActivatableList)
        {
            _debugRenderer.DrawCircle(new Circle(proximityActivatable.Position, ProximityActivationDistance), Color.Green);
        }
    }

    internal void Reset()
    {
        _proximityActivatableList.Clear();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ProximityActivationSystem : ICustomSystem
{
    private readonly ISceneManager _sceneManager;
    private readonly ProximityActivationService _proximityActivationService;

    public ProximityActivationSystem(ISceneManager sceneManager, ProximityActivationService proximityActivationService)
    {
        _sceneManager = sceneManager;
        _proximityActivationService = proximityActivationService;
    }

    public string Name => "ProximityActivationSystem";

    public void ProcessFixedUpdate()
    {
        var playerPosition = Query.GetPlayerTransform2DComponent(_sceneManager.CurrentScene).Translation;
        _proximityActivationService.UpdateActivations(playerPosition);
    }

    public void ProcessUpdate(GameTime gameTime)
    {
        _proximityActivationService.DebugDraw();
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