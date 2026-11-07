using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.GamePlay.Player;
using System;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class FightWormBossAI : IWormBossAI
{
    private readonly AABB2D _fightTrigger;

    private Transform2DComponent _wormTransform = null!;
    private Transform2DComponent _playerTransform = null!;
    private PlayerComponent _playerComponent = null!;
    private CameraMovementComponent _cameraMovementComponent = null!;

    private State _state = State.WaitingForTrigger;
    private readonly WaitingForTriggerState _waitingForTriggerState = new();

    private enum State
    {
        WaitingForTrigger,
        Phase1
    }

    public FightWormBossAI(AABB2D fightTrigger)
    {
        if (fightTrigger == default)
        {
            throw new ArgumentException("Trigger is invalid.", nameof(fightTrigger));
        }

        _fightTrigger = fightTrigger;
    }

    public void Init(Entity entity, IWormBossController controller)
    {
        _wormTransform = entity.GetComponent<Transform2DComponent>();
        _playerTransform = Query.GetPlayerTransformComponent(entity.Scene);
        _playerComponent = Query.GetPlayerComponent(entity.Scene);
        _cameraMovementComponent = Query.GetCameraMovementComponent(entity.Scene);
        _cameraMovementComponent.PointOfInterest = _wormTransform;

        _waitingForTriggerState.Init(controller);
    }

    public void Reset()
    {
        _state = State.WaitingForTrigger;

        _waitingForTriggerState.Reset();
    }

    public void Update(TimeSpan dt)
    {
        if (_state is State.WaitingForTrigger)
        {
            _waitingForTriggerState.Update(dt);

            if (_fightTrigger.Contains(_playerTransform.Translation))
            {
                _state = State.Phase1;
            }
        }
    }

    private sealed class WaitingForTriggerState
    {
        private IWormBossController _controller = null!;

        private TimeSpan _stateTimer = TimeSpan.Zero;
        private Vector2 _position;

        public void Init(IWormBossController controller)
        {
            _controller = controller;
            _position = controller.GetHeadPosition() + new Vector2(0, 70);
        }

        public void Reset()
        {
            _stateTimer = TimeSpan.Zero;
        }

        public void Update(TimeSpan dt)
        {
            _stateTimer += dt;

            const double speed = 2;
            var xPos = Math.Sin(_stateTimer.TotalSeconds * 0.5 * speed) * 30;
            var yPos = Math.Cos(_stateTimer.TotalSeconds * speed) * 10;
            _controller.SetHeadPosition(_position + new Vector2(xPos, yPos));
        }
    }
}