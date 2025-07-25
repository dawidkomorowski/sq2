using Geisha.Engine.Core.SceneModel;

namespace SQ2.Components.GamePlay;

internal sealed class SpikesComponent : Component
{
    public SpikesComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class SpikesComponentFactory : ComponentFactory<SpikesComponent>
{
    protected override SpikesComponent CreateComponent(Entity entity) => new(entity);
}