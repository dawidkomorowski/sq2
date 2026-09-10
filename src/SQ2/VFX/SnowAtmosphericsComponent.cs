using Geisha.Engine.Animation;
using Geisha.Engine.Animation.Components;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Assets;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using System;
using System.Linq;
using Geisha.Engine.Rendering;

namespace SQ2.VFX;

internal sealed class SnowAtmosphericsComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private Transform2DComponent _cameraTransform = null!;

    public SnowAtmosphericsComponent(Entity entity) : base(entity)
    {
    }

    public SpriteAnimation? Animation { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _cameraTransform = Entity.Scene.RootEntities.Single(e => e.HasComponent<CameraComponent>()).GetComponent<Transform2DComponent>();

        for (var i = -4; i <= 4; i++)
        {
            for (var j = -3; j <= 3; j++)
            {
                var x = i * 64;
                var y = j * 64;

                CreateAnimationTile(new Vector2(x, y));
            }
        }
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        // Keep animation tiles in camera view
        var cameraPosition = _cameraTransform.Translation;
        var animationTilesPosition = new Vector2
        (
            Math.Round(cameraPosition.X / 64) * 64,
            Math.Round(cameraPosition.Y / 64) * 64
        );
        _transform2DComponent.Translation = animationTilesPosition;
    }

    private void CreateAnimationTile(Vector2 position)
    {
        if (Animation is null) return;

        var entity = Entity.CreateChildEntity();
        var transform2DComponent = entity.CreateComponent<Transform2DComponent>();
        transform2DComponent.Translation = position;
        var spriteRendererComponent = entity.CreateComponent<SpriteRendererComponent>();
        spriteRendererComponent.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        var spriteAnimationComponent = entity.CreateComponent<SpriteAnimationComponent>();
        spriteAnimationComponent.AddAnimation("Animation", Animation);
        spriteAnimationComponent.PlayInLoop = true;
        spriteAnimationComponent.PlaybackSpeed = 0.75;
        spriteAnimationComponent.PlayAnimation("Animation");
    }
}

internal sealed class SnowAtmosphericsComponentFactory : ComponentFactory<SnowAtmosphericsComponent>
{
    protected override SnowAtmosphericsComponent CreateComponent(Entity entity) => new(entity);
}