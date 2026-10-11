using Geisha.Engine.Core.SceneModel;
using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class PlayerChaseWormBossAI : IWormBossAI
{
    private readonly double _chaseSpeed;
    private readonly AABB2D _trigger;
    private readonly Vector2 _raisePoint;

    private IWormBossController _controller = null!;
    private Transform2DComponent _playerTransform = null!;

    private State _state = State.WaitingForTrigger;

    private enum State
    {
        WaitingForTrigger,
        Raising,
        Chasing
    }

    public PlayerChaseWormBossAI(double chaseSpeed, AABB2D trigger, Vector2 raisePoint)
    {
        if (trigger == default)
        {
            throw new ArgumentException("Trigger is invalid.", nameof(trigger));
        }

        _chaseSpeed = chaseSpeed;
        _trigger = trigger;
        _raisePoint = raisePoint;
    }

    public void Init(Entity entity, IWormBossController controller)
    {
        _controller = controller;
        _playerTransform = Query.GetPlayerTransformComponent(entity.Scene);

        _state = State.WaitingForTrigger;
    }

    public void Reset()
    {
        _state = State.WaitingForTrigger;
    }

    public void Update(TimeSpan dt)
    {
        if (_state is State.WaitingForTrigger)
        {
            if (_trigger.Contains(_playerTransform.Translation))
            {
                _controller.SetSpeed(100);
                _controller.SetHeadPosition(_raisePoint + new Vector2(0, -150));
                _state = State.Raising;
            }

            return;
        }

        if (_state is State.Raising)
        {
            _controller.MoveTo(_raisePoint);
            if (_controller.HasReachedDestination())
            {
                _controller.SetSpeed(_chaseSpeed);
                _state = State.Chasing;
            }

            return;
        }

        if (_state is State.Chasing)
        {
            _controller.MoveTo(_playerTransform.Translation);
            return;
        }
    }
}