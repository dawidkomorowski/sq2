using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using SQ2.Core;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Player;

internal sealed class PlaytimeTrackingComponent : BehaviorComponent, IRespawnable
{
    private readonly GameStateService _gameStateService;
    private TimeSpan _playTime;

    public PlaytimeTrackingComponent(Entity entity, GameStateService gameStateService) : base(entity)
    {
        _gameStateService = gameStateService;
    }

    public override void OnStart()
    {
        _playTime = TimeSpan.Zero;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        _playTime += timeStep.UnscaledDeltaTime;
    }

    public override void OnRemove()
    {
        RegisterPlaytime();
    }

    public void Respawn()
    {
        RegisterPlaytime();
    }

    public void RegisterPlaytime()
    {
        _gameStateService.RegisterAdditionalPlaytime(_playTime);
        _playTime = TimeSpan.Zero;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlaytimeTrackingComponentFactory : ComponentFactory<PlaytimeTrackingComponent>
{
    private readonly GameStateService _gameStateService;

    public PlaytimeTrackingComponentFactory(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    protected override PlaytimeTrackingComponent CreateComponent(Entity entity) => new(entity, _gameStateService);
}