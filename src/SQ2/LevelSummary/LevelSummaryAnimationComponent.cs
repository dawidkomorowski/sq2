using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.LevelSummary;

internal sealed class LevelSummaryAnimationComponent : BehaviorComponent
{
    private TimeSpan _timer = TimeSpan.Zero;

    public LevelSummaryAnimationComponent(Entity entity) : base(entity)
    {
    }

    public Entity? PlayerCharacter { get; set; }
    public Entity? CoinInfo { get; set; }
    public Entity? DiamondInfo { get; set; }

    public override void OnStart()
    {
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        _timer += timeStep.DeltaTime;

        var duration = TimeSpan.FromSeconds(5);
        var alpha = _timer / duration;

        if (alpha >= 1)
        {
            return;
        }

        if (PlayerCharacter is null)
        {
            return;
        }

        var playerStart = new Vector2(-250, 25);
        var playerEnd = new Vector2(250, 25);
        var playerTransform = PlayerCharacter.GetComponent<Transform2DComponent>();
        playerTransform.Translation = Vector2.Lerp(playerStart, playerEnd, alpha);
        playerTransform.Scale = new Vector2(-1, 1);
    }
}

internal sealed class LevelSummaryAnimationComponentFactory : ComponentFactory<LevelSummaryAnimationComponent>
{
    protected override LevelSummaryAnimationComponent CreateComponent(Entity entity) => new(entity);
}