using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Enemies;

/// <summary>
/// Used to animate the death of a blue enemy and blue boss.
/// </summary>
internal sealed class BlueEnemyDeathAnimationComponent : BehaviorComponent
{
    private Transform2DComponent _transform2DComponent = null!;
    private Transform2D _initialTransform;
    private Transform2D _intermediateTransform;
    private Transform2D _targetTransform;
    private double _elapsedTime;

    public BlueEnemyDeathAnimationComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();

        _initialTransform = _transform2DComponent.Transform;
        _intermediateTransform = new Transform2D(
            _initialTransform.Translation + new Vector2(0, -5.5),
            _initialTransform.Rotation,
            new Vector2(_initialTransform.Scale.X * 1.1, 0.5)
        );
        _targetTransform = new Transform2D(
            _initialTransform.Translation + new Vector2(0, -11),
            _initialTransform.Rotation,
            new Vector2(_initialTransform.Scale.X * 2, 0)
        );
    }

    public override void OnUpdate(GameTime gameTime)
    {
        const double animationDuration = 0.2;
        _elapsedTime += gameTime.DeltaTimeSeconds;

        if (_elapsedTime > animationDuration)
        {
            Entity.RemoveAfterFullFrame();
            return;
        }

        var progress = _elapsedTime / animationDuration;
        if (progress < 0.5)
        {
            var intermediateProgress = progress / 0.5;
            _transform2DComponent.Transform = Transform2D.Lerp(_initialTransform, _intermediateTransform, intermediateProgress);
            return;
        }

        progress = (progress - 0.5) / 0.5;
        _transform2DComponent.Transform = Transform2D.Lerp(_intermediateTransform, _targetTransform, progress);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BlueEnemyDeathAnimationComponentFactory : ComponentFactory<BlueEnemyDeathAnimationComponent>
{
    protected override BlueEnemyDeathAnimationComponent CreateComponent(Entity entity) => new(entity);
}