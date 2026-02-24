using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Physics.Systems;
using SQ2.GamePlay.Common;

namespace SQ2.Development;

internal sealed class DevControlsComponent : BehaviorComponent
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;
    private readonly IPhysicsSystem _physicsSystem;

    public DevControlsComponent(Entity entity, IEngineManager engineManager, ISceneManager sceneManager, IPhysicsSystem physicsSystem) : base(entity)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _physicsSystem = physicsSystem;
    }

    public override void OnStart()
    {
        var inputComponent = Entity.CreateComponent<InputComponent>();
        inputComponent.InputMapping = new InputMapping
        {
            ActionMappings =
            {
                new ActionMapping
                {
                    ActionName = "Exit",
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Escape)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = "Reload",
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F5)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = "Respawn",
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F8)
                        }
                    }
                },
                new ActionMapping
                {
                    ActionName = "ToggleDebugPhysics",
                    HardwareActions =
                    {
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.F12)
                        }
                    }
                }
            }
        };

        inputComponent.BindAction("Exit", _engineManager.ScheduleEngineShutdown);
        inputComponent.BindAction("Reload", () => { _sceneManager.LoadEmptyScene("GameWorld", SceneLoadMode.PreserveAssets); });
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

    public DevControlsComponentFactory(IEngineManager engineManager, ISceneManager sceneManager, IPhysicsSystem physicsSystem)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
        _physicsSystem = physicsSystem;
    }

    protected override DevControlsComponent CreateComponent(Entity entity) => new(entity, _engineManager, _sceneManager, _physicsSystem);
}