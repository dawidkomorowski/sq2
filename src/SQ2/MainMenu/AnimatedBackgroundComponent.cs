using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.MainMenu;

internal sealed class AnimatedBackgroundComponent : BehaviorComponent
{
    private Transform2DComponent _transform = null!;

    public AnimatedBackgroundComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform = Entity.GetComponent<Transform2DComponent>();
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        _transform.Translation += Vector2.UnitX * 25 * timeStep.UnscaledDeltaTimeSeconds;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class AnimatedBackgroundComponentFactory : ComponentFactory<AnimatedBackgroundComponent>
{
    protected override AnimatedBackgroundComponent CreateComponent(Entity entity) => new(entity);
}