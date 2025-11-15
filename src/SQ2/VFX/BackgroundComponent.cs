using System;
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

        const int halfWidthInTiles = 10;
        const int halfHeightInTiles = 6;

        for (var x = -halfWidthInTiles; x <= halfWidthInTiles; x++)
        {
            for (var y = -halfHeightInTiles; y <= halfHeightInTiles; y++)
            {
                if (y == 0)
                {
                    CreateBackgroundTile(x, y, x % 2 == 0 ? MiddleLeftSprite : MiddleRightSprite);
                }
                else
                {
                    if (y > 0)
                    {
                        CreateBackgroundTile(x, y, x % 2 == 0 ? UpperLeftSprite : UpperRightSprite);
                    }
                    else
                    {
                        CreateBackgroundTile(x, y, x % 2 == 0 ? LowerLeftSprite : LowerRightSprite);
                    }
                }
            }
        }
    }

    public override void OnUpdate(GameTime gameTime)
    {
        // Parallax scrolling
        var cameraPosition = _cameraTransform.Translation;
        var backgroundPosition = new Vector2
        (
            Math.Floor(cameraPosition.X / 48) * 48 + (cameraPosition.X * 0.1) % 48,
            cameraPosition.Y + 24
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
        spriteRendererComponent.SortingLayerName = GlobalSettings.SortingLayers.Background;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BackgroundComponentFactory : ComponentFactory<BackgroundComponent>
{
    protected override BackgroundComponent CreateComponent(Entity entity) => new(entity);
}