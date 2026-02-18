using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.Core;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Boss.Bat;

internal sealed class BatBossSpawnerComponent : BehaviorComponent, IRespawnable
{
    private readonly EntityFactory _entityFactory;
    private double _secondsTimer;
    private bool _hasSpawned;

    public BatBossSpawnerComponent(Entity entity, EntityFactory entityFactory) : base(entity)
    {
        _entityFactory = entityFactory;
    }

    public Vector2 SpawnPosition { get; set; }
    public Vector2 TargetPoint { get; set; }
    public double SpawnAfterSeconds { get; set; }
    public double Velocity { get; set; }
    public bool Active { get; set; }

    public override void OnFixedUpdate()
    {
        if (!Active) return;

        _secondsTimer += GameTime.FixedDeltaTimeSeconds;

        if (_secondsTimer >= SpawnAfterSeconds && !_hasSpawned)
        {
            _entityFactory.CreateBatBoss(Scene, SpawnPosition, TargetPoint, Velocity);
            _hasSpawned = true;
        }
    }

    public void Respawn()
    {
        Active = false;
        _secondsTimer = 0;
        _hasSpawned = false;
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