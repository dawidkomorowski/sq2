using System;
using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Player;

internal sealed class CameraMovementComponent : BehaviorComponent
{
    private Transform2DComponent _cameraTransform = null!;
    private Transform2DComponent _playerTransform = null!;
    private TimeSpan _shakeDuration = TimeSpan.Zero;

    public CameraMovementComponent(Entity entity) : base(entity)
    {
    }

    public Transform2DComponent? PointOfInterest { get; set; }

    public override void OnStart()
    {
        _cameraTransform = Entity.GetComponent<Transform2DComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);

        _cameraTransform.Translation = _playerTransform.InterpolatedTransform.Translation;
    }

    public override void OnUpdate(GameTime gameTime)
    {
        // Determine target camera position.
        var playerPosition = _playerTransform.InterpolatedTransform.Translation;
        var targetPosition = playerPosition;

        // Remove point of interest if its entity is removed.
        if (PointOfInterest?.Entity.IsRemoved is true)
        {
            PointOfInterest = null;
        }

        // If player is close to point of interest, move camera to midpoint between player and point of interest.
        if (PointOfInterest is not null)
        {
            var poiPosition = PointOfInterest.InterpolatedTransform.Translation;
            var distance = playerPosition.Distance(poiPosition);

            switch (distance)
            {
                case < 300 and > 200:
                {
                    // Interpolate between midpoint and player position based on distance for smooth transition.
                    var alpha = (distance - 200) / 100;
                    targetPosition = Vector2.Lerp(playerPosition.Midpoint(poiPosition), playerPosition, alpha);
                    break;
                }
                case <= 200:
                    targetPosition = playerPosition.Midpoint(poiPosition);
                    break;
            }
        }

        // Move camera towards target position if it's farther than minDistance.
        const double minDistance = 30;
        var distanceToTarget = _cameraTransform.Translation.Distance(targetPosition);
        if (distanceToTarget > minDistance)
        {
            const double baseVelocity = 20;
            var distanceFactor = distanceToTarget - minDistance;
            var directionToPlayer = (targetPosition - _cameraTransform.Translation).Unit;
            _cameraTransform.Translation += directionToPlayer * distanceFactor * baseVelocity * gameTime.DeltaTimeSeconds;
        }

        // Camera shake.
        if (_shakeDuration > TimeSpan.Zero)
        {
            const double shakeIntensity = 4;
            var shakeOffsetX = (Random.Shared.NextDouble() * 2 - 1) * shakeIntensity;
            var shakeOffsetY = (Random.Shared.NextDouble() * 2 - 1) * shakeIntensity;
            _cameraTransform.Translation += new Vector2(shakeOffsetX, shakeOffsetY);
            _shakeDuration -= gameTime.DeltaTime;
            if (_shakeDuration < TimeSpan.Zero)
            {
                _shakeDuration = TimeSpan.Zero;
            }
        }
    }

    public void ShakeCamera()
    {
        _shakeDuration = TimeSpan.FromSeconds(0.5);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CameraMovementComponentFactory : ComponentFactory<CameraMovementComponent>
{
    protected override CameraMovementComponent CreateComponent(Entity entity) => new(entity);
}