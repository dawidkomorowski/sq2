using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;

namespace SQ2;

internal sealed class DevControlsComponent : BehaviorComponent
{
    private readonly IEngineManager _engineManager;

    public DevControlsComponent(Entity entity, IEngineManager engineManager) : base(entity)
    {
        _engineManager = engineManager;
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
                }
            }
        };

        inputComponent.BindAction("Exit", _engineManager.ScheduleEngineShutdown);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DevControlsComponentFactory : ComponentFactory<DevControlsComponent>
{
    private readonly IEngineManager _engineManager;

    public DevControlsComponentFactory(IEngineManager engineManager)
    {
        _engineManager = engineManager;
    }

    protected override DevControlsComponent CreateComponent(Entity entity) => new(entity, _engineManager);
}