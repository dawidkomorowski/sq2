using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.VFX;

internal sealed class WallParticleComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private TimeSpan _lifeTime = TimeSpan.FromSeconds(10);
    private Vector2 _velocity = Vector2.Zero;
    private double _angularVelocity = 0;

    public WallParticleComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _velocity = new Vector2(Random.Shared.NextDouble() * 200 - 100, Random.Shared.NextDouble() * 100 + 50);
        _angularVelocity = (Random.Shared.NextDouble() * 2 - 1) * 10;
    }

    public override void OnUpdate(GameTime gameTime)
    {
        _velocity += new Vector2(0, -300) * gameTime.DeltaTimeSeconds; // gravity

        var translation = _transform2DComponent.Translation;
        translation += _velocity * gameTime.DeltaTimeSeconds;
        _transform2DComponent.Translation = translation;

        _transform2DComponent.Rotation += _angularVelocity * gameTime.DeltaTimeSeconds;

        if (_lifeTime < TimeSpan.Zero)
        {
            Entity.RemoveAfterFullFrame();
        }

        _lifeTime -= gameTime.DeltaTime;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WallParticleComponentFactory : ComponentFactory<WallParticleComponent>
{
    protected override WallParticleComponent CreateComponent(Entity entity) => new(entity);
}