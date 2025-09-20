using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics.Components;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class JumpPadComponent : BehaviorComponent
{
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private KinematicRigidBody2DComponent _playerKinematicComponent = null!;
    private int _launchTimer = 0;

    public JumpPadComponent(Entity entity) : base(entity)
    {
    }

    public Sprite HighSprite { get; set; }
    public Sprite LowSprite { get; set; }

    public override void OnStart()
    {
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _spriteRendererComponent = Entity.Children[0].GetComponent<SpriteRendererComponent>();
        _playerKinematicComponent = Query.GetPlayerKinematicRigidBody2DComponent(Scene);

        _spriteRendererComponent.Sprite = HighSprite;
        _launchTimer = 0;
    }

    public override void OnFixedUpdate()
    {
        const double jumpPadBoost = 300;

        if (_launchTimer > 0)
        {
            _launchTimer--;
            if (_launchTimer == 0)
            {
                _spriteRendererComponent.Sprite = HighSprite;

                if (IsPlayerOnJumpPad())
                {
                    _playerKinematicComponent.LinearVelocity = new Vector2(_playerKinematicComponent.LinearVelocity.X, jumpPadBoost);
                }
            }
        }
        else
        {
            if (IsPlayerOnJumpPad())
            {
                _spriteRendererComponent.Sprite = LowSprite;
                _launchTimer = 3; // Delay jump to allow for animation to play
            }
        }
    }

    private bool IsPlayerOnJumpPad()
    {
        if (!_rectangleColliderComponent.IsColliding) return false;
        var contacts = _rectangleColliderComponent.GetContacts();
        foreach (var contact in contacts)
        {
            if (contact.CollisionNormal.Y < 0 && contact.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                return true;
            }
        }

        return false;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class JumpPadComponentFactory : ComponentFactory<JumpPadComponent>
{
    protected override JumpPadComponent CreateComponent(Entity entity) => new(entity);
}