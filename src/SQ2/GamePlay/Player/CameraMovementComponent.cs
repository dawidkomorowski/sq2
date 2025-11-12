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

    public override void OnStart()
    {
        _cameraTransform = Entity.GetComponent<Transform2DComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);

        _cameraTransform.Translation = _playerTransform.InterpolatedTransform.Translation;
    }

    public override void OnUpdate(GameTime gameTime)
    {
        // Camera follow.
        var playerPosition = _playerTransform.InterpolatedTransform.Translation;

        const double minDistance = 30;
        var distanceToPlayer = _cameraTransform.Translation.Distance(playerPosition);
        if (distanceToPlayer > minDistance)
        {
            const double baseVelocity = 20;
            var distanceFactor = distanceToPlayer - minDistance;
            var directionToPlayer = (playerPosition - _cameraTransform.Translation).Unit;
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