using System.Linq;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.VFX;

internal sealed class BackgroundComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private Transform2DComponent _cameraTransform = null!;

    public BackgroundComponent(Entity entity) : base(entity)
    {
    }

    public Sprite? UpperLeftSprite { get; set; }
    public Sprite? UpperRightSprite { get; set; }
    public Sprite? MiddleLeftSprite { get; set; }
    public Sprite? MiddleRightSprite { get; set; }
    public Sprite? LowerLeftSprite { get; set; }
    public Sprite? LowerRightSprite { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _cameraTransform = Entity.Scene.RootEntities.Single(e => e.HasComponent<CameraComponent>()).GetComponent<Transform2DComponent>();

        const int widthInTiles = 2;
        const int heightInTiles = 2;

        //for (int x = 0; x < widthInTiles; x++)
        //{
        //    for (int y = 0; y < heightInTiles; y++)
        //    {
        //        CreateBackgroundTile(x, y, null);
        //    }
        //}

        CreateBackgroundTile(-1, 1, UpperLeftSprite);
        CreateBackgroundTile(0, 1, UpperRightSprite);
        CreateBackgroundTile(-1, 0, MiddleLeftSprite);
        CreateBackgroundTile(0, 0, MiddleRightSprite);
        CreateBackgroundTile(-1, -1, LowerLeftSprite);
        CreateBackgroundTile(0, -1, LowerRightSprite);
    }

    public override void OnUpdate(GameTime gameTime)
    {
        var cameraPosition = _cameraTransform.Translation;
        var backgroundPosition = new Vector2
        (
            cameraPosition.X * 1.0,
            cameraPosition.Y * 1.0 + 24
        );
        _transform2DComponent.Translation = backgroundPosition;
    }

    private void CreateBackgroundTile(int tx, int ty, Sprite? sprite)
    {
        var tileEntity = Entity.CreateChildEntity();
        var transform2DComponent = tileEntity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = new Vector2(tx * 24, ty * 24);
        var spriteRendererComponent = tileEntity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.Sprite = sprite;
        spriteRendererComponent.SortingLayerName = GlobalSettings.BackgroundSortingLayer;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BackgroundComponentFactory : ComponentFactory<BackgroundComponent>
{
    protected override BackgroundComponent CreateComponent(Entity entity) => new(entity);
}