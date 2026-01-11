using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class KeyHoleComponent : BehaviorComponent, IRespawnable
{
    private TileColliderComponent? _tileColliderComponent;
    private SpriteRendererComponent _spriteRendererComponent = null!;

    public KeyHoleComponent(Entity entity) : base(entity)
    {
    }

    public int KeysRequired { get; set; }

    public override void OnStart()
    {
        _tileColliderComponent = Entity.GetComponent<TileColliderComponent>();
        _spriteRendererComponent = Entity.GetComponent<SpriteRendererComponent>();
    }

    public override void OnFixedUpdate()
    {
        if (_tileColliderComponent is null) return;

        var contacts = _tileColliderComponent.GetContacts();
        foreach (var contact in contacts)
        {
            if (contact.OtherCollider.Entity.HasComponent<PlayerComponent>() &&
                contact.OtherCollider.Entity.GetComponent<PlayerComponent>().KeysCollected >= KeysRequired)
            {
                Entity.RemoveComponent(_tileColliderComponent);
                _tileColliderComponent = null;
                _spriteRendererComponent.Visible = false;
                break;
            }
        }
    }

    public void Respawn()
    {
        if (!Entity.HasComponent<TileColliderComponent>())
        {
            _tileColliderComponent = Entity.CreateComponent<TileColliderComponent>();
        }

        _spriteRendererComponent.Visible = true;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class KeyHoleComponentFactory : ComponentFactory<KeyHoleComponent>
{
    protected override KeyHoleComponent CreateComponent(Entity entity) => new(entity);
}