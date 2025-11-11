using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class DestructibleWallComponent : BehaviorComponent, IRespawnable
{
    private readonly EntityFactory _entityFactory;
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private bool _isDestroyed;

    public DestructibleWallComponent(Entity entity, EntityFactory entityFactory) : base(entity)
    {
        _entityFactory = entityFactory;
    }

    public int ButtonId { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
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

        // Create particle effect
        _entityFactory.CreateWallParticleBig(Scene, _transform2DComponent.Translation);
        _entityFactory.CreateWallParticleBig(Scene, _transform2DComponent.Translation);
        _entityFactory.CreateWallParticleSmall(Scene, _transform2DComponent.Translation);
        _entityFactory.CreateWallParticleSmall(Scene, _transform2DComponent.Translation);
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
    private readonly EntityFactory _entityFactory;

    public DestructibleWallComponentFactory(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }

    protected override DestructibleWallComponent CreateComponent(Entity entity) => new(entity, _entityFactory);
}