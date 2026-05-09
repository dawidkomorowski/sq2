using System.Collections.Immutable;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Input;
using Geisha.Engine.Input.Components;
using Geisha.Engine.Input.Mapping;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.MainMenu.SelectLevelView;

internal sealed class SelectLevelViewComponent : BehaviorComponent
{
    private const string ActionNavigateBackToMainView = "NavigateUp";
    private InputComponent _inputComponent = null!;

    public SelectLevelViewComponent(Entity entity) : base(entity)
    {
    }

    public ViewTransitionComponent? ViewTransitionComponent { get; set; }

    public override void OnStart()
    {
        _inputComponent = Entity.CreateComponent<InputComponent>();
        _inputComponent.InputMapping = new InputMapping
        {
            ActionMappings = ImmutableArray.Create
            (
                new ActionMapping
                {
                    ActionName = ActionNavigateBackToMainView,
                    HardwareActions = ImmutableArray.Create
                    (
                        new HardwareAction
                        {
                            HardwareInputVariant = HardwareInputVariant.CreateKeyboardVariant(Key.Escape)
                        }
                    )
                }
            )
        };

        _inputComponent.BindAction(ActionNavigateBackToMainView, OnAction_NavigateBackToMainView);

        _inputComponent.Enabled = false; // Transition component activates view.

        var textEntity = Entity.CreateChildEntity();
        textEntity.CreateComponent<Transform2DComponent>();
        var textRenderer = textEntity.CreateComponent<TextRendererComponent>();
        textRenderer.Text = "Select level";
        textRenderer.Color = Color.White;
        textRenderer.FontSize = FontSize.FromDips(24);
        textRenderer.MaxWidth = 300;
        textRenderer.TextAlignment = TextAlignment.Center;
        textRenderer.Pivot = new Vector2(150, 0);
    }

    public void OnView_Activated()
    {
        _inputComponent.Enabled = true;
    }

    private void OnAction_NavigateBackToMainView()
    {
        _inputComponent.Enabled = false;
        ViewTransitionComponent?.ChangeView(ViewTransitionComponent.View.MainView);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class SelectLevelViewComponentFactory : ComponentFactory<SelectLevelViewComponent>
{
    protected override SelectLevelViewComponent CreateComponent(Entity entity) => new(entity);
}