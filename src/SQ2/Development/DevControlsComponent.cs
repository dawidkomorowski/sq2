using System.Collections.Immutable;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Physics.Systems;
using SQ2.Core;
using SQ2.GamePlay.Common;

namespace SQ2.Development;

internal sealed class DevControlsComponent : BehaviorComponent
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;
    private readonly IPhysicsSystem _physicsSystem;
    private readonly GameSaveService _gameSaveService;

    public DevControlsComponent(Entity entity, IEngineManager engineManager, ISceneManager sceneManager, IPhysicsSystem physicsSystem,
        GameSaveService gameSaveService) : base(entity)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _physicsSystem = physicsSystem;
        _gameSaveService = gameSaveService;
    }

    public override void OnStart()
    {
        var inputComponent = Entity.CreateComponent<InputComponent>();
        inputComponent.InputMapping = new InputMapping
        {
            ActionMappings = ImmutableArray.Create
            (
                new ActionMapping
                {
                    ActionName = "Exit",
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Escape)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = "Reload",
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F5)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = "ClearSave",
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F6)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = "Respawn",
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F8)
                        }
                    )
                },
                new ActionMapping
                {
                    ActionName = "ToggleDebugPhysics",
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F12)
                        }
                    )
                }
            )
        };

        inputComponent.BindAction("Exit", _engineManager.ScheduleEngineShutdown);
        inputComponent.BindAction("Reload", () => { _sceneManager.LoadEmptyScene("GameWorld"); });
        inputComponent.BindAction("ClearSave", _gameSaveService.ClearSave);
        inputComponent.BindAction("Respawn", Respawn);
        inputComponent.BindAction("ToggleDebugPhysics", () => { _physicsSystem.EnableDebugRendering = !_physicsSystem.EnableDebugRendering; });
    }

    private void Respawn()
    {
        var playerComponent = Query.GetPlayerComponent(Scene);
        playerComponent.KillPlayer();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DevControlsComponentFactory : ComponentFactory<DevControlsComponent>
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;
    private readonly IPhysicsSystem _physicsSystem;
    private readonly GameSaveService _gameSaveService;

    public DevControlsComponentFactory(IEngineManager engineManager, ISceneManager sceneManager, IPhysicsSystem physicsSystem, GameSaveService gameSaveService)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _physicsSystem = physicsSystem;
        _gameSaveService = gameSaveService;
    }

    protected override DevControlsComponent CreateComponent(Entity entity) => new(entity, _engineManager, _sceneManager, _physicsSystem, _gameSaveService);
}