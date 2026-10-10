using System;
using System.Collections.Generic;
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

    public List<Entity> Tail { get; } = new();

    public override void OnStart()
    {
        _headTransform = Entity.GetComponent<Transform2DComponent>();
        _moveToPosition = _headTransform.Translation;
    }

    public override void OnFixedUpdate()
    {
        ProcessAI();
        ProcessHeadMovement();
        ProcessTailMovement();
    }

    private void ProcessAI()
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

    private void ProcessHeadMovement()
    {
        var distance = _headTransform.Translation.Distance(_moveToPosition);
        if (distance > 10)
        {
            const double velocity = 10;
            var direction = (_moveToPosition - _headTransform.Translation).Unit;
            _headTransform.Translation += direction * velocity * TimeStep.FixedDeltaTimeSeconds;
        }
    }

    private void ProcessTailMovement()
    {
        const double maxDistance = 10;

        var segment1 = Entity;

        for (var i = 0; i < Tail.Count; i++)
        {
            var segment2 = Tail[i];

            var transform1 = segment1.GetComponent<Transform2DComponent>();
            var transform2 = segment2.GetComponent<Transform2DComponent>();

            var distance = transform1.Translation.Distance(transform2.Translation);
            if (distance > maxDistance)
            {
                var translationFrom1To2 = transform2.Translation - transform1.Translation;
                transform2.Translation = transform1.Translation + translationFrom1To2.OfLength(maxDistance);
            }

            // Chain segment pairs.
            segment1 = segment2;
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WormBossComponentFactory : ComponentFactory<WormBossComponent>
{
    protected override WormBossComponent CreateComponent(Entity entity) => new(entity);
}