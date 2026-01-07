using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Player;

internal sealed class CheckPointComponent : Component
{
    public CheckPointComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CheckPointComponentFactory : ComponentFactory<CheckPointComponent>
{
    protected override CheckPointComponent CreateComponent(Entity entity) => new(entity);
}