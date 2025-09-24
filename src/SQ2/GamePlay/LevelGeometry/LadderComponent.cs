using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class LadderComponent : Component
{
    public LadderComponent(Entity entity) : base(entity)
    {
    }

    public AxisAlignedRectangle HitBox { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LadderComponentFactory : ComponentFactory<LadderComponent>
{
    protected override LadderComponent CreateComponent(Entity entity) => new(entity);
}