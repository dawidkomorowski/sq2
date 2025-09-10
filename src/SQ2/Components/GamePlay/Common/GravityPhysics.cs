using Geisha.Engine.Core;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Physics.Components;

namespace SQ2.Components.GamePlay.Common;

internal static class GravityPhysics
{
    private static readonly Vector2 Gravity = new(0, -500);
    private const double MaxFallVelocity = -200;

    public static void Update(KinematicRigidBody2DComponent kinematicBody)
    {
        kinematicBody.LinearVelocity += Gravity * GameTime.FixedDeltaTimeSeconds;

        if (kinematicBody.LinearVelocity.Y < MaxFallVelocity)
        {
            kinematicBody.LinearVelocity = kinematicBody.LinearVelocity.WithY(MaxFallVelocity);
        }
    }
}