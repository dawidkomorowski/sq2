using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class DestructibleWallComponent : BehaviorComponent, IRespawnable
{
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private bool _isDestroyed;

    public DestructibleWallComponent(Entity entity) : base(entity)
    {
    }

    public int ButtonId { get; set; }

    public override void OnStart()
    {
        _spriteRendererComponent = Entity.GetComponent<SpriteRendererComponent>();

        foreach (var entity in Scene.RootEntities)
        {
            foreach (var component in entity.Components)
            {
                if (component is ButtonComponent buttonComponent && buttonComponent.ObjectId == ButtonId)
                {
                    buttonComponent.OnPressedActions.Add(Destroy);
                }
            }
        }
    }

    private void Destroy()
    {
        if (_isDestroyed) return;

        _isDestroyed = true;
        _spriteRendererComponent.Visible = false;
        Entity.RemoveComponent(Entity.GetComponent<TileColliderComponent>());
    }

    public void Respawn()
    {
        if (!_isDestroyed) return;

        _isDestroyed = false;
        _spriteRendererComponent.Visible = true;
        Entity.CreateComponent<TileColliderComponent>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DestructibleWallComponentFactory : ComponentFactory<DestructibleWallComponent>
{
    protected override DestructibleWallComponent CreateComponent(Entity entity) => new(entity);
}