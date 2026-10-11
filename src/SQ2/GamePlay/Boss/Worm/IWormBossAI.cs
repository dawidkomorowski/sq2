using System;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.GamePlay.Boss.Worm;

internal interface IWormBossAI
{
    void Init(Entity entity, IWormBossController controller);
    void Reset();
    void Update(TimeSpan dt);
}

internal interface IWormBossController
{
    void MoveTo(Vector2 position);
    bool HasReachedDestination();
    Vector2 GetHeadPosition();
    void SetHeadPosition(Vector2 position);
}