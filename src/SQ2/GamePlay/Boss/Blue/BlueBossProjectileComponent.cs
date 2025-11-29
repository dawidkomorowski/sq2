using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Boss.Blue;

internal sealed class BlueBossProjectileComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private TimeSpan _lifetime;

    public BlueBossProjectileComponent(Entity entity) : base(entity)
    {
    }

    public Vector2 Direction { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
    }

    public override void OnFixedUpdate()
    {
        _lifetime += GameTime.FixedDeltaTime;

        if (_lifetime >= TimeSpan.FromSeconds(5))
        {
            Entity.RemoveAfterFixedTimeStep();
            return;
        }

        const double speed = 100.0;
        _transform2DComponent.Translation += Direction * speed * GameTime.FixedDeltaTime.TotalSeconds;
    }
}

internal sealed class BlueBossProjectileComponentFactory : ComponentFactory<BlueBossProjectileComponent>
{
    protected override BlueBossProjectileComponent CreateComponent(Entity entity) => new(entity);
}