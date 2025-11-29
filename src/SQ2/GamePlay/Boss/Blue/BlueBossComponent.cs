using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Boss.Blue;

internal sealed class BlueBossComponent : BehaviorComponent
{
    internal static readonly Vector2 SpriteOffset = new(0, 0);

    public BlueBossComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueBossComponentFactory : ComponentFactory<BlueBossComponent>
{
    protected override BlueBossComponent CreateComponent(Entity entity) => new(entity);
}