using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class WormBossComponent : BehaviorComponent
{
    private Transform2DComponent _headTransform = null!;

    private Vector2 _moveToPosition;

    private bool _decided;
    private TimeSpan _timer;

    public WormBossComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _headTransform = Entity.GetComponent<Transform2DComponent>();
        _moveToPosition = _headTransform.Translation;
    }

    public override void OnFixedUpdate()
    {
        ProcessDecision();
        ProcessMovement();
    }

    private void ProcessDecision()
    {
        if (_decided)
        {
            return;
        }

        _timer += TimeStep.FixedDeltaTime;

        if (_timer > TimeSpan.FromSeconds(1))
        {
            MoveTo(_headTransform.Translation + new Vector2(0, 100));
            _decided = true;
        }
    }

    private void MoveTo(Vector2 position)
    {
        _moveToPosition = position;
    }

    private void ProcessMovement()
    {
        var distance = _headTransform.Translation.Distance(_moveToPosition);
        if (distance > 10)
        {
            const double velocity = 10;
            var direction = (_moveToPosition - _headTransform.Translation).Unit;
            _headTransform.Translation += direction * velocity * TimeStep.FixedDeltaTimeSeconds;
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WormBossComponentFactory : ComponentFactory<WormBossComponent>
{
    protected override WormBossComponent CreateComponent(Entity entity) => new(entity);
}