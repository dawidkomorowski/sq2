using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class KeyHoleComponent : BehaviorComponent, IRespawnable
{
    private TileColliderComponent _tileColliderComponent = null!;
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
        var contacts = _tileColliderComponent.GetContacts();
        foreach (var contact in contacts)
        {
            if (contact.OtherCollider.Entity.HasComponent<PlayerComponent>() &&
                contact.OtherCollider.Entity.GetComponent<PlayerComponent>().KeysCollected >= KeysRequired)
            {
                _tileColliderComponent.Enabled = false;
                _spriteRendererComponent.Visible = false;
                break;
            }
        }
    }

    public void Respawn()
    {
        _tileColliderComponent.Enabled = true;
        _spriteRendererComponent.Visible = true;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class KeyHoleComponentFactory : ComponentFactory<KeyHoleComponent>
{
    protected override KeyHoleComponent CreateComponent(Entity entity) => new(entity);
}