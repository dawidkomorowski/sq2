using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Core.Systems;
using System.Collections.Generic;
using Geisha.Engine.Rendering.Components;

namespace SQ2.GamePlay.Player;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CheckPointSystem : ICustomSystem
{
    private readonly List<CheckPointComponent> _checkPoints = new();
    private PlayerComponent? _playerComponent;
    private Transform2DComponent? _playerTransform;

    public string Name => "CheckPointSystem";

    public void ProcessFixedUpdate()
    {
        if (_playerComponent is not null && _playerTransform is not null)
        {
            foreach (var checkPoint in _checkPoints)
            {
                var checkPointTransform = checkPoint.Entity.GetComponent<Transform2DComponent>();
                if (_playerTransform.Translation.Distance(checkPointTransform.Translation) < 10)
                {
                    _playerComponent.ActiveCheckPoint = checkPoint;

                    foreach (var cp in _checkPoints)
                    {
                        var spriteRenderer = cp.Entity.GetComponent<SpriteRendererComponent>();
                        spriteRenderer.Sprite = cp == checkPoint ? cp.ActiveSprite : cp.InactiveSprite;
                    }
                }
            }
        }
    }

    public void ProcessUpdate(GameTime gameTime)
    {
    }

    public void OnEntityCreated(Entity entity)
    {
    }

    public void OnEntityRemoved(Entity entity)
    {
    }

    public void OnEntityParentChanged(Entity entity, Entity? oldParent, Entity? newParent)
    {
    }

    public void OnComponentCreated(Component component)
    {
        if (component is CheckPointComponent checkPointComponent)
        {
            _checkPoints.Add(checkPointComponent);
        }

        if (component.Entity.HasComponent<PlayerComponent>() && component.Entity.HasComponent<Transform2DComponent>())
        {
            _playerComponent = component.Entity.GetComponent<PlayerComponent>();
            _playerTransform = component.Entity.GetComponent<Transform2DComponent>();
        }
    }

    public void OnComponentRemoved(Component component)
    {
        if (component is CheckPointComponent checkPointComponent)
        {
            _checkPoints.Remove(checkPointComponent);
        }

        if (component is PlayerComponent)
        {
            _playerComponent = null!;
            _playerTransform = null!;
        }
    }
}