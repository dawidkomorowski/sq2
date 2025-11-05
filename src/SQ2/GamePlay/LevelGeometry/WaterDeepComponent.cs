using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class WaterDeepComponent : Component
{
    public WaterDeepComponent(Entity entity) : base(entity)
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WaterDeepComponentFactory : ComponentFactory<WaterDeepComponent>
{
    protected override WaterDeepComponent CreateComponent(Entity entity) => new(entity);
}