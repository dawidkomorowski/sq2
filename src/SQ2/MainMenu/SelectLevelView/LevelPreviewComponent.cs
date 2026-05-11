using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;

namespace SQ2.MainMenu.SelectLevelView;

internal sealed class LevelPreviewComponent : BehaviorComponent
{
    public LevelPreviewComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        var rectangleRenderer = Entity.CreateComponent<RectangleRendererComponent>();
        rectangleRenderer.Color = Color.Red;
        rectangleRenderer.FillInterior = true;
        rectangleRenderer.Dimensions = new Vector2(100, 100);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LevelPreviewComponentFactory : ComponentFactory<LevelPreviewComponent>
{
    protected override LevelPreviewComponent CreateComponent(Entity entity) => new(entity);
}