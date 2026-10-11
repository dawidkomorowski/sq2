using Geisha.Engine.Core.SceneModel;
using System;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class PlayerChaseWormBossAI : IWormBossAI
{
    private readonly double _chaseSpeed;
    private readonly AABB2D _trigger;
    private readonly AABB2D _endTrigger;
    private readonly Vector2 _raisePoint;
    private readonly Vector2 _hidePoint;

    private IWormBossController _controller = null!;
    private Transform2DComponent _playerTransform = null!;
    private PlayerComponent _playerComponent = null!;
    private CinematicCameraComponent _cinematicCamera = null!;

    private State _state = State.WaitingForTrigger;

    private enum State
    {
        WaitingForTrigger,
        Raising,
        Chasing,
        Hiding
    }

    public PlayerChaseWormBossAI(double chaseSpeed, AABB2D trigger, AABB2D endTrigger, Vector2 raisePoint, Vector2 hidePoint)
    {
        if (trigger == default)
        {
            throw new ArgumentException("Trigger is invalid.", nameof(trigger));
        }

        _chaseSpeed = chaseSpeed;
        _trigger = trigger;
        _endTrigger = endTrigger;
        _raisePoint = raisePoint;
        _hidePoint = hidePoint;
    }

    public void Init(Entity entity, IWormBossController controller)
    {
        _controller = controller;
        _playerTransform = Query.GetPlayerTransformComponent(entity.Scene);
        _playerComponent = Query.GetPlayerComponent(entity.Scene);
        _cinematicCamera = Query.GetCinematicCameraComponent(entity.Scene);

        _state = State.WaitingForTrigger;
    }

    public void Reset()
    {
        _state = State.WaitingForTrigger;
        _playerComponent.EnableInput();
        _cinematicCamera.Hide();
    }

    public void Update(TimeSpan dt)
    {
        if (_state is State.WaitingForTrigger)
        {
            if (_trigger.Contains(_playerTransform.Translation))
            {
                _playerComponent.DisableInput();
                _cinematicCamera.Show();
                _controller.SetSpeed(100);
                _state = State.Raising;
            }

            return;
        }

        if (_state is State.Raising)
        {
            _controller.MoveTo(_raisePoint);
            if (_controller.HasReachedDestination())
            {
                _playerComponent.EnableInput();
                _cinematicCamera.Hide();
                _controller.SetSpeed(_chaseSpeed);
                _state = State.Chasing;
            }

            return;
        }

        if (_state is State.Chasing)
        {
            if (_endTrigger.Contains(_controller.GetHeadPosition()))
            {
                _state = State.Hiding;
            }

            _controller.MoveTo(_playerTransform.Translation);
            return;
        }

        if (_state is State.Hiding)
        {
            _controller.MoveTo(_hidePoint);
            return;
        }
    }
}