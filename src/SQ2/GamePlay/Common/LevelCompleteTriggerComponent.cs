using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.PauseMenu;
using SQ2.GamePlay.Player;
using SQ2.VFX;

namespace SQ2.GamePlay.Common;

internal sealed class LevelCompleteTriggerComponent : BehaviorComponent
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.LevelCompleteTrigger;
    private readonly IDebugRenderer _debugRenderer;
    private readonly ISceneManager _sceneManager;
    private readonly GameStateService _gameStateService;
    private PlayerComponent _playerComponent = null!;
    private Transform2DComponent _playerTransform2DComponent = null!;
    private RectangleColliderComponent _playerRectangleColliderComponent = null!;
    private CameraMovementComponent _cameraMovementComponent = null!;
    private CinematicCameraComponent _cinematicCameraComponent = null!;
    private PauseMenuComponent _pauseMenuComponent = null!;
    private bool _triggered;
    private TimeSpan _timer;
    private Entity? _fadeOutEntity;

    public LevelCompleteTriggerComponent(Entity entity, IDebugRenderer debugRenderer, ISceneManager sceneManager, GameStateService gameStateService) :
        base(entity)
    {
        _debugRenderer = debugRenderer;
        _sceneManager = sceneManager;
        _gameStateService = gameStateService;
    }

    public AxisAlignedRectangle TriggerArea { get; set; }
    public LevelCompleteDirection LevelCompleteDirection { get; set; }

    public override void OnStart()
    {
        _playerComponent = Query.GetPlayerComponent(Scene);
        _playerTransform2DComponent = Query.GetPlayerTransform2DComponent(Scene);
        _playerRectangleColliderComponent = Query.GetPlayerRectangleColliderComponent(Scene);
        _cameraMovementComponent = Query.GetCameraMovementComponent(Scene);
        _cinematicCameraComponent = Query.GetCinematicCameraComponent(Scene);
        _pauseMenuComponent = Query.GetPauseMenuComponent(Scene);

        _timer = TimeSpan.Zero;
    }

    public override void OnFixedUpdate()
    {
        if (_triggered)
        {
            _timer += TimeStep.FixedDeltaTime;

            if (_timer >= TimeSpan.FromSeconds(0.5))
            {
                _cameraMovementComponent.EnableFollow = false;
            }

            if (_timer >= TimeSpan.FromSeconds(3) && _fadeOutEntity is null)
            {
                _fadeOutEntity = _cinematicCameraComponent.Entity.CreateChildEntity();
                _fadeOutEntity.CreateComponent<FadeOutComponent>();
            }

            if (_timer >= TimeSpan.FromSeconds(4.5))
            {
                _gameStateService.CompleteLevel();
                _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld);
            }

            return;
        }

        var playerPosition = _playerTransform2DComponent.Translation;
        var playerDimensions = _playerRectangleColliderComponent.Dimensions;
        var playerHitBox = new AxisAlignedRectangle(playerPosition, playerDimensions);

        if (TriggerArea.Overlaps(playerHitBox))
        {
            _triggered = true;

            _cinematicCameraComponent.Show();
            _playerComponent.DisableInput();
            _pauseMenuComponent.Disable();

            switch (LevelCompleteDirection)
            {
                case LevelCompleteDirection.Left:
                    _playerComponent.ForceMoveLeft = true;
                    break;
                case LevelCompleteDirection.Right:
                    _playerComponent.ForceMoveRight = true;
                    break;
                case LevelCompleteDirection.Down:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        if (_enableDebugDraw)
        {
            _debugRenderer.DrawRectangle(TriggerArea, Color.Red, Matrix3x3.Identity);
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LevelCompleteTriggerComponentFactory : ComponentFactory<LevelCompleteTriggerComponent>
{
    private readonly IDebugRenderer _debugRenderer;
    private readonly ISceneManager _sceneManager;
    private readonly GameStateService _gameStateService;

    public LevelCompleteTriggerComponentFactory(IDebugRenderer debugRenderer, ISceneManager sceneManager, GameStateService gameStateService)
    {
        _debugRenderer = debugRenderer;
        _sceneManager = sceneManager;
        _gameStateService = gameStateService;
    }

    protected override LevelCompleteTriggerComponent CreateComponent(Entity entity) => new(entity, _debugRenderer, _sceneManager, _gameStateService);
}