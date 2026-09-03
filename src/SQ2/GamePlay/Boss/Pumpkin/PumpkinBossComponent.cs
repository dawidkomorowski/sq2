using System;
using System.Collections.Generic;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.Boss.Pumpkin;

internal sealed class PumpkinBossComponent : BehaviorComponent, IRespawnable
{
    internal static readonly Vector2 SpriteOffset = new(0, 2);

    private Transform2DComponent _transform2DComponent = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private Transform2DComponent _spriteTransformComponent = null!;


    private readonly List<Contact2D> _contacts = new();
    private double _animationTimer;

    public PumpkinBossComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();
        _spriteTransformComponent = Entity.Children[0].GetComponent<Transform2DComponent>();

        Query.GetCameraMovementComponent(Scene).PointOfInterest = _transform2DComponent;
    }

    public override void OnFixedUpdate()
    {
        var contacts = _rectangleColliderComponent.GetContactsAsSpan(_contacts);

        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();
                playerComponent.KillPlayer();
                break;
            }
        }

        const double animationSpeed = 10;
        _animationTimer += TimeStep.FixedDeltaTimeSeconds * animationSpeed;
        var verticalScale = 1 + Math.Sin(_animationTimer) * 0.1;
        _spriteTransformComponent.Scale = new Vector2(1, verticalScale);
        var animationYOffset = (verticalScale - 1) * GlobalSettings.TileSize.Height;
        _spriteTransformComponent.Translation = SpriteOffset + new Vector2(0, animationYOffset);
    }

    public void Respawn()
    {
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PumpkinBossComponentFactory : ComponentFactory<PumpkinBossComponent>
{
    protected override PumpkinBossComponent CreateComponent(Entity entity) => new(entity);
}