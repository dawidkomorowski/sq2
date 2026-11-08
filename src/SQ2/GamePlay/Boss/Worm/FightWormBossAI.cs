using System;
using System.Collections.Generic;
using System.Linq;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class FightWormBossAI : IWormBossAI
{
    private readonly AABB2D _fightTrigger;
    private readonly WormPathPoint[] _attackSequence1;
    private readonly List<DestructibleWallComponent> _destructibleWalls = new();

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

    public FightWormBossAI(AABB2D fightTrigger, WormPathPoint[] attackSequence1)
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

        _destructibleWalls.AddRange(
            entity.Scene.AllEntities
                .Where(e => e.HasComponent<DestructibleWallComponent>())
                .Select(e => e.GetComponent<DestructibleWallComponent>())
        );

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
        private int _sequenceIndex = -1;

        public void Init(FightWormBossAI wormBossAI)
        {
            _wormBossAI = wormBossAI;
            _controller = wormBossAI._controller;
        }

        public void Reset()
        {
            _sequenceIndex = -1;
        }

        public void Update(TimeSpan dt)
        {
            if (_controller.HasReachedDestination())
            {
                _sequenceIndex++;
                if (_sequenceIndex >= _wormBossAI._attackSequence1.Length)
                {
                    _sequenceIndex = -1;
                    _wormBossAI._state = State.ToDo;
                    return;
                }
            }

            var wormPathPoint = _wormBossAI._attackSequence1[_sequenceIndex];
            _controller.MoveTo(wormPathPoint.Position);
            _controller.SetSpeed(wormPathPoint.Speed);

            if (wormPathPoint.ButtonIdForDestructibleWalls != 0)
            {
                foreach (var destructibleWallComponent in _wormBossAI._destructibleWalls)
                {
                    if (destructibleWallComponent.ButtonId == wormPathPoint.ButtonIdForDestructibleWalls)
                    {
                        destructibleWallComponent.Destroy();
                    }
                }
            }
        }
    }
}