using Geisha.Engine.Core.SceneModel;

namespace SQ2.Components.GamePlay.Player;

internal sealed class PlayerCheckPointComponent : Component
{
    public PlayerCheckPointComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlayerCheckPointComponentFactory : ComponentFactory<PlayerCheckPointComponent>
{
    protected override PlayerCheckPointComponent CreateComponent(Entity entity) => new(entity);
}