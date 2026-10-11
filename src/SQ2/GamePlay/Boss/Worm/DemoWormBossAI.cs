using Geisha.Engine.Core.SceneModel;
using System;
using Geisha.Engine.Core.Math;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class DemoWormBossAI : IWormBossAI
{
    private AIState _aiState = AIState.Waiting;
    private TimeSpan _stateTimer;
    private Vector2 _position;

    private IWormBossController _controller = null!;

    private enum AIState
    {
        Waiting,
        Raising,
        Waving
    }

    public void Init(Entity entity, IWormBossController controller)
    {
        _controller = controller;
    }

    public void Reset()
    {
        _aiState = AIState.Waiting;
        _stateTimer = TimeSpan.Zero;
    }

    public void Update(TimeSpan dt)
    {
        _stateTimer += dt;

        if (_aiState is AIState.Waiting)
        {
            if (_stateTimer > TimeSpan.FromSeconds(1))
            {
                _controller.MoveTo(_controller.GetHeadPosition() + new Vector2(0, 80));
                _aiState = AIState.Raising;
                _stateTimer = TimeSpan.Zero;
            }

            return;
        }

        if (_aiState is AIState.Raising)
        {
            if (_controller.HasReachedDestination())
            {
                _position = _controller.GetHeadPosition() - new Vector2(0, 10);
                _aiState = AIState.Waving;
                _stateTimer = TimeSpan.Zero;
            }
        }

        if (_aiState is AIState.Waving)
        {
            const double speed = 2;
            var xPos = Math.Sin(_stateTimer.TotalSeconds * 0.5 * speed) * 30;
            var yPos = Math.Cos(_stateTimer.TotalSeconds * speed) * 10;
            _controller.SetHeadPosition(_position + new Vector2(xPos, yPos));
        }
    }
}