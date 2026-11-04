using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.Core;

namespace SQ2.LevelSummary;

internal sealed class LevelSummaryAnimationComponent : BehaviorComponent
{
    private TimeSpan _timer = TimeSpan.Zero;
    private readonly ISceneManager _sceneManager;

    public LevelSummaryAnimationComponent(Entity entity, ISceneManager sceneManager) : base(entity)
    {
        _sceneManager = sceneManager;
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
            _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.LoadingScreen);
            return;
        }

        AnimatePlayerCharacter(alpha);
        AnimateCoinInfo(alpha);
        AnimateDiamondInfo(alpha);
    }

    private void AnimatePlayerCharacter(double alpha)
    {
        if (PlayerCharacter is null)
        {
            return;
        }

        var start = new Vector2(-250, 25);
        var end = new Vector2(250, 25);

        var transform = PlayerCharacter.GetComponent<Transform2DComponent>();
        transform.Translation = Vector2.Lerp(start, end, alpha);
        transform.Scale = new Vector2(-1, 1);
    }

    private void AnimateCoinInfo(double alpha)
    {
        if (CoinInfo is null)
        {
            return;
        }

        var start = new Vector2(-250, -10);
        var center = new Vector2(0, -10);
        var end = new Vector2(250, -10);

        var transform = CoinInfo.GetComponent<Transform2DComponent>();

        switch (alpha)
        {
            case < 0.10:
                transform.Translation = start;
                return;
            case >= 0.10 and <= 0.20:
                alpha = (alpha - 0.10) * 10;
                alpha = Ease.InOutSine(alpha);
                transform.Translation = Vector2.Lerp(start, center, alpha);
                return;
            case >= 0.70 and <= 0.80:
                alpha = (alpha - 0.70) * 10;
                alpha = Ease.InOutSine(alpha);
                transform.Translation = Vector2.Lerp(center, end, alpha);
                return;
            case > 0.80:
                transform.Translation = end;
                return;
        }
    }

    private void AnimateDiamondInfo(double alpha)
    {
        if (DiamondInfo is null)
        {
            return;
        }

        var start = new Vector2(-250, -40);
        var center = new Vector2(-5, -40);
        var end = new Vector2(250, -40);

        var transform = DiamondInfo.GetComponent<Transform2DComponent>();

        switch (alpha)
        {
            case < 0.20:
                transform.Translation = start;
                return;
            case >= 0.20 and <= 0.30:
                alpha = (alpha - 0.20) * 10;
                alpha = Ease.InOutSine(alpha);
                transform.Translation = Vector2.Lerp(start, center, alpha);
                return;
            case >= 0.80 and <= 0.90:
                alpha = (alpha - 0.80) * 10;
                alpha = Ease.InOutSine(alpha);
                transform.Translation = Vector2.Lerp(center, end, alpha);
                return;
            case > 0.90:
                transform.Translation = end;
                return;
        }
    }
}

internal sealed class LevelSummaryAnimationComponentFactory : ComponentFactory<LevelSummaryAnimationComponent>
{
    private readonly ISceneManager _sceneManager;

    public LevelSummaryAnimationComponentFactory(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    protected override LevelSummaryAnimationComponent CreateComponent(Entity entity) => new(entity, _sceneManager);
}