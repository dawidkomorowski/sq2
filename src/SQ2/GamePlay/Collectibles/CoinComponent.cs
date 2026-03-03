using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Collectibles;

internal sealed class CoinComponent : BehaviorComponent, IRespawnable
{
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private Transform2DComponent _playerTransform = null!;

    private bool _isCollected;

    public CoinComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteRendererComponent = Entity.GetComponent<SpriteRendererComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);
    }

    public override void OnFixedUpdate()
    {
        if (_isCollected) return;

        var distanceToPlayer = _transform2DComponent.Translation.Distance(_playerTransform.Translation);
        if (distanceToPlayer < 13)
        {
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
internal sealed class CoinComponentFactory : ComponentFactory<CoinComponent>
{
    protected override CoinComponent CreateComponent(Entity entity) => new(entity);
}