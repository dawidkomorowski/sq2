using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;

namespace SQ2.GamePlay.Player;

internal sealed class CheckPointComponent : Component
{
    public CheckPointComponent(Entity entity) : base(entity)
    {
    }

    public Sprite? ActiveSprite { get; set; }
    public Sprite? InactiveSprite { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CheckPointComponentFactory : ComponentFactory<CheckPointComponent>
{
    protected override CheckPointComponent CreateComponent(Entity entity) => new(entity);
}