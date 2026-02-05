using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Physics;
using Geisha.Engine.Physics.Components;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;
using System;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class RaisingWaterComponent : BehaviorComponent, IRespawnable
{
    private Transform2DComponent _transform2DComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private RectangleColliderComponent _rectangleColliderComponent = null!;
    private bool _hasStarted = false;
    private double _delayTimer = 0;
    private Vector2 _initialPosition;

    public RaisingWaterComponent(Entity entity) : base(entity)
    {
    }

    public double Velocity { get; set; }
    public double Delay { get; set; }
    public Vector2 Dimensions { get; set; }
    public double MaxY { get; set; }

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);
        _rectangleColliderComponent = Entity.GetComponent<RectangleColliderComponent>();

        _initialPosition = _transform2DComponent.Translation;
    }

    public override void OnFixedUpdate()
    {
        var contacts = _rectangleColliderComponent.IsColliding ? _rectangleColliderComponent.GetContacts() : Array.Empty<Contact2D>();

        foreach (var contact2D in contacts)
        {
            if (contact2D.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact2D.OtherCollider.Entity.GetComponent<PlayerComponent>();
                playerComponent.KillPlayer();
                break;
            }
        }

        if (!_hasStarted)
        {
            var activationArea = new AxisAlignedRectangle(_transform2DComponent.Translation + new Vector2(0, Dimensions.Y), Dimensions);

            if (activationArea.Contains(_playerTransform.Translation))
            {
                _hasStarted = true;
                _delayTimer = Delay;
            }

            return;
        }

        if (_delayTimer > 0)
        {
            _delayTimer -= GameTime.FixedDeltaTimeSeconds;
            return;
        }

        if (_transform2DComponent.Translation.Y >= MaxY)
        {
            return;
        }

        _transform2DComponent.Translation += new Vector2(0, Velocity * GameTime.FixedDeltaTimeSeconds);
    }

    public void Respawn()
    {
        _hasStarted = false;
        _delayTimer = 0;
        _transform2DComponent.SetTransformImmediate(_transform2DComponent.Transform with
        {
            Translation = _initialPosition
        });
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class RaisingWaterComponentFactory : ComponentFactory<RaisingWaterComponent>
{
    protected override RaisingWaterComponent CreateComponent(Entity entity) => new(entity);
}