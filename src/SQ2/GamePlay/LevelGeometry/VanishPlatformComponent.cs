using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class VanishPlatformComponent : BehaviorComponent, IRespawnable
{
    private TileColliderComponent _tileColliderComponent = null!;
    private bool _isReadyForVanish;

    public VanishPlatformComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _tileColliderComponent = Entity.GetComponent<TileColliderComponent>();
    }

    public override void OnFixedUpdate()
    {
        if (!_tileColliderComponent.Enabled) return;

        var hasLoad = false;

        if (_tileColliderComponent.IsColliding)
        {
            var contacts = _tileColliderComponent.GetContacts();
            foreach (var contact in contacts)
            {
                if (contact.CollisionNormal.Y < 0 && contact.OtherCollider.Entity.HasComponent<PlayerComponent>())
                {
                    hasLoad = true;
                    _isReadyForVanish = true;
                    break;
                }
            }
        }

        if (hasLoad) return;
        if (_isReadyForVanish)
        {
            Vanish();
        }
    }

    private void Vanish()
    {
        _tileColliderComponent.Enabled = false;
        Entity.GetComponent<SpriteRendererComponent>().Visible = false;
    }

    public void Respawn()
    {
        _tileColliderComponent.Enabled = true;
        Entity.GetComponent<SpriteRendererComponent>().Visible = true;
        _isReadyForVanish = false;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class VanishPlatformComponentFactory : ComponentFactory<VanishPlatformComponent>
{
    protected override VanishPlatformComponent CreateComponent(Entity entity) => new(entity);
}