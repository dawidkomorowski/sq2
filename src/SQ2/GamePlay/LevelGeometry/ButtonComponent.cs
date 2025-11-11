using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using System;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class ButtonComponent : BehaviorComponent, IRespawnable
{
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;

    private bool _isPressed;

    public ButtonComponent(Entity entity) : base(entity)
    {
    }

    public Sprite? PressedSprite { get; set; }
    public Sprite? ReleasedSprite { get; set; }

    public override void OnStart()
    {
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _spriteRendererComponent = Entity.Children[0].GetComponent<SpriteRendererComponent>();

        _spriteRendererComponent.Sprite = ReleasedSprite;
    }

    public override void OnFixedUpdate()
    {
        if (_isPressed) return;

        var contacts = _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();
        foreach (var contact2D in contacts)
        {
            if (contact2D.CollisionNormal.Y < 0)
            {
                OnPressed();
                break;
            }
        }
    }

    private void OnPressed()
    {
        _isPressed = true;
        _spriteRendererComponent.Sprite = PressedSprite;
    }

    public void Respawn()
    {
        _isPressed = false;
        _spriteRendererComponent.Sprite = ReleasedSprite;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ButtonComponentFactory : ComponentFactory<ButtonComponent>
{
    protected override ButtonComponent CreateComponent(Entity entity) => new(entity);
}