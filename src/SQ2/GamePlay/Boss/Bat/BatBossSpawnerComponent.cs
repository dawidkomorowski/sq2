using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.Core;

namespace SQ2.GamePlay.Boss.Bat;

internal sealed class BatBossSpawnerComponent : BehaviorComponent
{
    private readonly EntityFactory _entityFactory;
    private int _framesToSpawn = 300;

    public BatBossSpawnerComponent(Entity entity, EntityFactory entityFactory) : base(entity)
    {
        _entityFactory = entityFactory;
    }

    public Vector2 SpawnPosition { get; set; }
    public Vector2 TargetPoint { get; set; }

    public override void OnFixedUpdate()
    {
        if (_framesToSpawn > 0)
        {
            _framesToSpawn--;
            if (_framesToSpawn == 0)
            {
                _entityFactory.CreateBatBoss(Scene, SpawnPosition, TargetPoint);
            }
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BatBossSpawnerComponentFactory : ComponentFactory<BatBossSpawnerComponent>
{
    private readonly EntityFactory _entityFactory;

    public BatBossSpawnerComponentFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    protected override BatBossSpawnerComponent CreateComponent(Entity entity) => new(entity, _entityFactory);
}