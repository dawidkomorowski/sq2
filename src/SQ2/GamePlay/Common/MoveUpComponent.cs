using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Common;

internal sealed class MoveUpComponent : BehaviorComponent
{
    private Transform2DComponent _transform = null!;

    public MoveUpComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform = Entity.GetComponent<Transform2DComponent>();
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        _transform.Translation += new Vector2(0, 10 * timeStep.UnscaledDeltaTimeSeconds);
    }
}

internal sealed class MoveUpComponentFactory : ComponentFactory<MoveUpComponent>
{
    protected override MoveUpComponent CreateComponent(Entity entity) => new(entity);
}