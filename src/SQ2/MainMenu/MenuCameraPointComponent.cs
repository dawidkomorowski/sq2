using Geisha.Engine.Core.SceneModel;

namespace SQ2.MainMenu;

internal sealed class MenuCameraPointComponent : Component
{
    public MenuCameraPointComponent(Entity entity) : base(entity)
    {
    }

    public bool IsStart { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MenuCameraPointComponentFactory : ComponentFactory<MenuCameraPointComponent>
{
    protected override MenuCameraPointComponent CreateComponent(Entity entity) => new(entity);
}