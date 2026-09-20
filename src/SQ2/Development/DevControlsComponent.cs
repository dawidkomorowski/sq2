using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Physics.Systems;
using Geisha.Engine.Windowing;
using SQ2.Core;
using SQ2.GamePlay.Common;

namespace SQ2.Development;

internal sealed class DevControlsComponent : BehaviorComponent
{
    private readonly ISceneManager _sceneManager;
    private readonly IPhysicsSystem _physicsSystem;
    private readonly IWindowingSystem _windowingSystem;
    private readonly GameSaveService _gameSaveService;

    public DevControlsComponent(Entity entity, ISceneManager sceneManager, IPhysicsSystem physicsSystem,
        IWindowingSystem windowingSystem, GameSaveService gameSaveService) : base(entity)
    {
        _sceneManager = sceneManager;
        _physicsSystem = physicsSystem;
        _windowingSystem = windowingSystem;
        _gameSaveService = gameSaveService;
    }

    public override void OnStart()
    {
        var inputComponent = Entity.CreateComponent<InputComponent>();
        inputComponent.InputMapping = InputMapping.CreateBuilder()
            .MapAction("Reload", Key.F5)
            .MapAction("ClearSave", Key.F6)
            .MapAction("Respawn", Key.F8)
            .MapAction("ToggleFullscreen", Key.F11)
            .MapAction("ToggleDebugPhysics", Key.F12)
            .Build();

        inputComponent.BindAction("Reload", () => { _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld); });
        inputComponent.BindAction("ClearSave", _gameSaveService.ClearSave);
        inputComponent.BindAction("Respawn", Respawn);
        inputComponent.BindAction("ToggleFullscreen", ToggleFullscreen);
        inputComponent.BindAction("ToggleDebugPhysics", () => { _physicsSystem.EnableDebugRendering = !_physicsSystem.EnableDebugRendering; });
    }

    private void Respawn()
    {
        var playerComponent = Query.GetPlayerComponent(Scene);
        playerComponent.KillPlayer();
    }

    private void ToggleFullscreen()
    {
        _windowingSystem.DisplayMode = _windowingSystem.DisplayMode switch
        {
            DisplayMode.Windowed => DisplayMode.Fullscreen,
            DisplayMode.Fullscreen => DisplayMode.Windowed,
            _ => throw new InvalidOperationException("Invalid display mode.")
        };
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DevControlsComponentFactory : ComponentFactory<DevControlsComponent>
{
    private readonly ISceneManager _sceneManager;
    private readonly IPhysicsSystem _physicsSystem;
    private readonly IWindowingSystem _windowingSystem;
    private readonly GameSaveService _gameSaveService;

    public DevControlsComponentFactory(ISceneManager sceneManager, IPhysicsSystem physicsSystem, IWindowingSystem windowingSystem,
        GameSaveService gameSaveService)
    {
        _sceneManager = sceneManager;
        _physicsSystem = physicsSystem;
        _windowingSystem = windowingSystem;
        _gameSaveService = gameSaveService;
    }

    protected override DevControlsComponent CreateComponent(Entity entity) => new(entity, _sceneManager, _physicsSystem, _windowingSystem, _gameSaveService);
}