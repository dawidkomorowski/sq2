using Geisha.Engine.Core.SceneModel;
using System;
using Geisha.Engine.Core.Components;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class PlayerChaseWormBossAI : IWormBossAI
{
    private IWormBossController _controller = null!;
    private Transform2DComponent _playerTransform = null!;

    public void Init(Entity entity, IWormBossController controller)
    {
        _controller = controller;
        _playerTransform = Query.GetPlayerTransformComponent(entity.Scene);
    }

    public void Reset()
    {
    }

    public void Update(TimeSpan dt)
    {
        _controller.MoveTo(_playerTransform.Translation);
    }
}