using System;
using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.PauseMenu;
using SQ2.GamePlay.Player;
using SQ2.VFX;

namespace SQ2.GamePlay.Boss;

internal sealed class KeyComponent : BehaviorComponent, IRespawnable
{
    private readonly ISceneManager _sceneManager;
    private readonly GameStateService _gameStateService;

    private Transform2DComponent _transform2DComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private KinematicRigidBody2DComponent _kinematicComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private PlayerComponent _playerComponent = null!;
    private CinematicCameraComponent _cinematicCameraComponent = null!;
    private PauseMenuComponent _pauseMenuComponent = null!;

    private readonly List<Contact2D> _contacts = new();
    private bool _isFullyDropped;
    private bool _isCollected;
    private TimeSpan _timer;
    private Entity? _fadeOutEntity;

    public KeyComponent(Entity entity, ISceneManager sceneManager, GameStateService gameStateService) : base(entity)
    {
        _sceneManager = sceneManager;
        _gameStateService = gameStateService;
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteRendererComponent = Entity.GetComponent<SpriteRendererComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _kinematicComponent = Entity.GetComponent<KinematicRigidBody2DComponent>();
        _playerTransform = Query.GetPlayerTransformComponent(Scene);
        _playerComponent = Query.GetPlayerComponent(Scene);
        _cinematicCameraComponent = Query.GetCinematicCameraComponent(Scene);
        _pauseMenuComponent = Query.GetPauseMenuComponent(Scene);
    }

    public override void OnFixedUpdate()
    {
        if (_isCollected)
        {
            _timer += TimeStep.FixedDeltaTime;

            if (_timer >= TimeSpan.FromSeconds(1.5) && _fadeOutEntity is null)
            {
                _fadeOutEntity = _cinematicCameraComponent.Entity.CreateChildEntity();
                _fadeOutEntity.CreateComponent<FadeInOutComponent>();
            }

            if (_timer >= TimeSpan.FromSeconds(3))
            {
                _gameStateService.CompleteLevel(_playerComponent.CoinsCollected);
                _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.LevelSummary);
            }

            return;
        }

        if (_isFullyDropped)
        {
            var distanceToPlayer = _transform2DComponent.Translation.Distance(_playerTransform.Translation);
            if (distanceToPlayer < 5)
            {
                _isCollected = true;

                Entity.RemoveComponent(_rectangleColliderComponent);
                Entity.RemoveComponent(_kinematicComponent);

                Entity.Parent = _playerComponent.Entity;
                _spriteRendererComponent.OrderInLayer = 1;

                _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
                {
                    Translation = new Vector2(-12, -5),
                    Scale = new Vector2(-1, 1)
                });

                _cinematicCameraComponent.Show();
                _playerComponent.DisableInput();
                _pauseMenuComponent.Disable();
            }
        }
        else
        {
            Movement.ApplyGravity(_kinematicComponent);

            var contacts = _rectangleColliderComponent.GetContactsAsSpan(_contacts);
            foreach (var contact2D in contacts)
            {
                if (contact2D.CollisionNormal.Y > 0)
                {
                    _isFullyDropped = true;
                    _kinematicComponent.EnableCollisionResponse = false;
                    _kinematicComponent.LinearVelocity = Vector2.Zero;
                }
            }
        }
    }

    public void Respawn()
    {
        Entity.RemoveAfterFixedTimeStep();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class KeyComponentFactory : ComponentFactory<KeyComponent>
{
    private readonly ISceneManager _sceneManager;
    private readonly GameStateService _gameStateService;

    public KeyComponentFactory(ISceneManager sceneManager, GameStateService gameStateService)
    {
        _sceneManager = sceneManager;
        _gameStateService = gameStateService;
    }

    protected override KeyComponent CreateComponent(Entity entity) => new(entity, _sceneManager, _gameStateService);
}