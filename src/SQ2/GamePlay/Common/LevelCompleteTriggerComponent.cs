using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Diagnostics;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using SQ2.Development;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Common;

internal sealed class LevelCompleteTriggerComponent : BehaviorComponent
{
    private readonly bool _enableDebugDraw = DevConfig.DebugDraw.LevelCompleteTrigger;
    private readonly IDebugRenderer _debugRenderer;
    private readonly ISceneManager _sceneManager;
    private PlayerComponent _playerComponent = null!;
    private Transform2DComponent _playerTransform2DComponent = null!;
    private RectangleColliderComponent _playerRectangleColliderComponent = null!;
    private CameraMovementComponent _cameraMovementComponent = null!;
    private TimeSpan _timer;

    public LevelCompleteTriggerComponent(Entity entity, IDebugRenderer debugRenderer, ISceneManager sceneManager) : base(entity)
    {
        _debugRenderer = debugRenderer;
        _sceneManager = sceneManager;
    }

    public AxisAlignedRectangle TriggerArea { get; set; }

    public override void OnStart()
    {
        _playerComponent = Query.GetPlayerComponent(Scene);
        _playerTransform2DComponent = Query.GetPlayerTransform2DComponent(Scene);
        _playerRectangleColliderComponent = Query.GetPlayerRectangleColliderComponent(Scene);
        _cameraMovementComponent = Query.GetCameraMovementComponent(Scene);

        _timer = TimeSpan.Zero;
    }

    public override void OnFixedUpdate()
    {
        var playerPosition = _playerTransform2DComponent.Translation;
        var playerDimensions = _playerRectangleColliderComponent.Dimensions;
        var playerHitBox = new AxisAlignedRectangle(playerPosition, playerDimensions);

        if (TriggerArea.Overlaps(playerHitBox))
        {
            _timer += GameTime.FixedDeltaTime;

            _playerComponent.DisableInput();
            _playerComponent.ForceMoveRight = true;
            _cameraMovementComponent.EnableFollow = false;

            if (_timer >= TimeSpan.FromSeconds(3))
            {
                _sceneManager.LoadEmptyScene("GameWorld");
            }
        }
    }

    public override void OnUpdate(GameTime gameTime)
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

    public LevelCompleteTriggerComponentFactory(IDebugRenderer debugRenderer, ISceneManager sceneManager)
    {
        _debugRenderer = debugRenderer;
        _sceneManager = sceneManager;
    }

    protected override LevelCompleteTriggerComponent CreateComponent(Entity entity) => new(entity, _debugRenderer, _sceneManager);
}