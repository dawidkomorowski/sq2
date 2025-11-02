using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Enemies;

internal sealed class BlueEnemyDeathAnimationComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;

    public BlueEnemyDeathAnimationComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
    }

    public override void OnUpdate(GameTime gameTime)
    {
        const double shrinkSpeed = 1.5;
        var currentScale = _transform2DComponent.Scale;
        var newScale = currentScale - new Vector2(-2, 4) * gameTime.DeltaTimeSeconds * shrinkSpeed;
        if (newScale.Y <= 0)
        {
            Entity.RemoveAfterFullFrame();
            return;
        }

        _transform2DComponent.Scale = newScale;
        _transform2DComponent.Translation -= new Vector2(0, 45) * gameTime.DeltaTimeSeconds * shrinkSpeed;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueEnemyDeathAnimationComponentFactory : ComponentFactory<BlueEnemyDeathAnimationComponent>
{
    protected override BlueEnemyDeathAnimationComponent CreateComponent(Entity entity) => new(entity);
}