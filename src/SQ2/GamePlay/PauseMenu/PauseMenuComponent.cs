using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using System.Collections.Immutable;
using Geisha.Engine.Core;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.PauseMenu;

internal sealed class PauseMenuComponent : BehaviorComponent
{
    private const string ActionPause = "Pause";
    private InputComponent _inputComponent = null!;

    private readonly ITimeSystem _timeSystem;

    public PauseMenuComponent(Entity entity, ITimeSystem timeSystem) : base(entity)
    {
        _timeSystem = timeSystem;
    }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = new InputMapping
        {
            ActionMappings = ImmutableArray.Create
            (
                new ActionMapping
                {
                    ActionName = ActionPause,
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.P)
                        }
                    )
                }
            )
        };

        _inputComponent.BindAction(ActionPause, OnActionPause);
    }

    private void OnActionPause()
    {
        _timeSystem.TimeScale = _timeSystem.TimeScale == 0 ? 1 : 0;

        var playerComponent = Query.GetPlayerComponent(Scene);
        if (_timeSystem.TimeScale == 0)
        {
            playerComponent.DisableInput();
        }
        else
        {
            playerComponent.EnableInput();
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PauseMenuComponentFactory : ComponentFactory<PauseMenuComponent>
{
    private readonly ITimeSystem _timeSystem;

    public PauseMenuComponentFactory(ITimeSystem timeSystem)
    {
        _timeSystem = timeSystem;
    }

    protected override PauseMenuComponent CreateComponent(Entity entity) => new(entity, _timeSystem);
}