using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Player;

internal sealed class PlayerSpawnPointComponent : Component
{
    public PlayerSpawnPointComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PlayerSpawnPointComponentFactory : ComponentFactory<PlayerSpawnPointComponent>
{
    protected override PlayerSpawnPointComponent CreateComponent(Entity entity) => new(entity);
}