using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class WormBossComponent : BehaviorComponent
{
    public WormBossComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WormBossComponentFactory : ComponentFactory<WormBossComponent>
{
    protected override WormBossComponent CreateComponent(Entity entity) => new(entity);
}