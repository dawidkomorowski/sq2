using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;

namespace SQ2.Development;

internal sealed class DevControlsComponent : BehaviorComponent
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;

    public DevControlsComponent(Entity entity, IEngineManager engineManager, ISceneManager sceneManager) : base(entity)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
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
                }
            }
        };

        inputComponent.BindAction("Exit", _engineManager.ScheduleEngineShutdown);
        inputComponent.BindAction("Reload", () => { _sceneManager.LoadEmptyScene("GameWorld", SceneLoadMode.PreserveAssets); });
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DevControlsComponentFactory : ComponentFactory<DevControlsComponent>
{
    private readonly IEngineManager _engineManager;
    private readonly ISceneManager _sceneManager;

    public DevControlsComponentFactory(IEngineManager engineManager, ISceneManager sceneManager)
    {
        _engineManager = engineManager;
        _sceneManager = sceneManager;
    }

    protected override DevControlsComponent CreateComponent(Entity entity) => new(entity, _engineManager, _sceneManager);
}