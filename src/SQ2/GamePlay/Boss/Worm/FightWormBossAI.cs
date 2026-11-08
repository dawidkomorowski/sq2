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
    private readonly Vector2[] _attackSequence1;

    private IWormBossController _controller = null!;
    private Transform2DComponent _wormTransform = null!;
    private Transform2DComponent _playerTransform = null!;
    private PlayerComponent _playerComponent = null!;
    private CameraMovementComponent _cameraMovementComponent = null!;

    private State _state = State.WaitingForTrigger;
    private readonly WaitingForTriggerStateAI _waitingForTriggerStateAI = new();
    private readonly AttackSequenceStateAI _attackSequenceStateAI = new();

    private enum State
    {
        WaitingForTrigger,
        AttackSequence1,
        ToDo
    }

    public FightWormBossAI(AABB2D fightTrigger, Vector2[] attackSequence1)
    {
        if (fightTrigger == default)
        {
            throw new ArgumentException("Trigger is invalid.", nameof(fightTrigger));
        }

        _fightTrigger = fightTrigger;
        _attackSequence1 = attackSequence1;
    }

    public void Init(Entity entity, IWormBossController controller)
    {
        _controller = controller;
        _wormTransform = entity.GetComponent<Transform2DComponent>();
        _playerTransform = Query.GetPlayerTransformComponent(entity.Scene);
        _playerComponent = Query.GetPlayerComponent(entity.Scene);
        _cameraMovementComponent = Query.GetCameraMovementComponent(entity.Scene);
        _cameraMovementComponent.PointOfInterest = _wormTransform;

        _waitingForTriggerStateAI.Init(this);
        _attackSequenceStateAI.Init(this);
    }

    public void Reset()
    {
        _state = State.WaitingForTrigger;

        _waitingForTriggerStateAI.Reset();
        _attackSequenceStateAI.Reset();
    }

    public void Update(TimeSpan dt)
    {
        switch (_state)
        {
            case State.WaitingForTrigger:
                _waitingForTriggerStateAI.Update(dt);
                break;
            case State.AttackSequence1:
                _attackSequenceStateAI.Update(dt);
                break;
            case State.ToDo:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private sealed class WaitingForTriggerStateAI
    {
        private FightWormBossAI _wormBossAI = null!;
        private IWormBossController _controller = null!;

        private TimeSpan _stateTimer = TimeSpan.Zero;
        private Vector2 _position;
        private TimeSpan _triggeredTimer = TimeSpan.Zero;

        public void Init(FightWormBossAI wormBossAI)
        {
            _wormBossAI = wormBossAI;
            _controller = wormBossAI._controller;
            _position = _controller.GetHeadPosition() + new Vector2(0, 70);
        }

        public void Reset()
        {
            _stateTimer = TimeSpan.Zero;
            _triggeredTimer = TimeSpan.Zero;
        }

        public void Update(TimeSpan dt)
        {
            _stateTimer += dt;

            const double speed = 2;
            var xPos = Math.Sin(_stateTimer.TotalSeconds * 0.5 * speed) * 30;
            var yPos = Math.Cos(_stateTimer.TotalSeconds * speed) * 10;
            _controller.SetHeadPosition(_position + new Vector2(xPos, yPos));

            if (_wormBossAI._fightTrigger.Contains(_wormBossAI._playerTransform.Translation))
            {
                _triggeredTimer += dt;
                if (_triggeredTimer > TimeSpan.FromSeconds(2))
                {
                    _wormBossAI._state = State.AttackSequence1;
                }
            }
        }
    }

    private sealed class AttackSequenceStateAI
    {
        private FightWormBossAI _wormBossAI = null!;
        private IWormBossController _controller = null!;
        private int _sequenceIndex = 0;

        public void Init(FightWormBossAI wormBossAI)
        {
            _wormBossAI = wormBossAI;
            _controller = wormBossAI._controller;
        }

        public void Reset()
        {
            _sequenceIndex = 0;
        }

        public void Update(TimeSpan dt)
        {
            var target = _wormBossAI._attackSequence1[_sequenceIndex];
            _controller.MoveTo(target);
            _controller.SetSpeed(100);

            if (_controller.HasReachedDestination())
            {
                _sequenceIndex++;
                if (_sequenceIndex >= _wormBossAI._attackSequence1.Length)
                {
                    _sequenceIndex = 0;
                    _wormBossAI._state = State.ToDo;
                }
            }
        }
    }
}