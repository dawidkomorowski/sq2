using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class KeyComponent : BehaviorComponent, IRespawnable
{
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private PlayerComponent _playerComponent = null!;

    private bool _isCollected;

    public KeyComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteRendererComponent = Entity.GetComponent<SpriteRendererComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);
        _playerComponent = Query.GetPlayerComponent(Scene);
    }

    public override void OnFixedUpdate()
    {
        if (_isCollected) return;

        var distanceToPlayer = _transform2DComponent.Translation.Distance(_playerTransform.Translation);
        if (distanceToPlayer < 13)
        {
            _playerComponent.KeysCollected++;
            _isCollected = true;
            _spriteRendererComponent.Visible = false;
        }
    }

    public void Respawn()
    {
        _isCollected = false;
        _spriteRendererComponent.Visible = true;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class KeyComponentFactory : ComponentFactory<KeyComponent>
{
    protected override KeyComponent CreateComponent(Entity entity) => new(entity);
}