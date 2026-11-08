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
    private readonly WaitingForTriggerStateAI _waitingForTriggerStateAI = new();
    private readonly StraightAttackStateAI _straightAttackStateAI = new();

    private enum State
    {
        WaitingForTrigger,
        StraightAttack,
        ToDo
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

        _waitingForTriggerStateAI.Init(controller);
        _straightAttackStateAI.Init(controller, _playerTransform);
    }

    public void Reset()
    {
        _state = State.WaitingForTrigger;

        _waitingForTriggerStateAI.Reset();
        _straightAttackStateAI.Reset();
    }

    public void Update(TimeSpan dt)
    {
        if (_state is State.WaitingForTrigger)
        {
            _waitingForTriggerStateAI.Update(dt);

            if (_fightTrigger.Contains(_playerTransform.Translation))
            {
                _state = State.StraightAttack;
            }
        }

        if (_state is State.StraightAttack)
        {
            _straightAttackStateAI.Update(dt);
        }
    }

    private sealed class WaitingForTriggerStateAI
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

    private sealed class StraightAttackStateAI
    {
        private IWormBossController _controller = null!;
        private Transform2DComponent _playerTransform = null!;
        private Vector2 _targetPosition;
        private bool _targetAcquired = false;

        public void Init(IWormBossController controller, Transform2DComponent playerTransform)
        {
            _controller = controller;
            _playerTransform = playerTransform;
        }

        public void Reset()
        {
            _targetAcquired = false;
        }

        public void Update(TimeSpan dt)
        {
            if (!_targetAcquired)
            {
                _targetPosition = _playerTransform.Translation;
                _targetAcquired = true;
                _controller.SetSpeed(150);
                return;
            }

            _controller.MoveTo(_targetPosition);
        }
    }
}