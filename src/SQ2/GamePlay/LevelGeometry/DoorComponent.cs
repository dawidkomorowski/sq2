using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class DoorComponent : BehaviorComponent
{
    public DoorComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DoorComponentFactory : ComponentFactory<DoorComponent>
{
    protected override DoorComponent CreateComponent(Entity entity) => new(entity);
}