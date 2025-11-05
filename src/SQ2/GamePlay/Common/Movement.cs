using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Physics.Components;

namespace SQ2.GamePlay.Common;

internal static class Movement
{
    private static readonly Vector2 Gravity = new(0, -500);
    private const double MaxFallVelocity = -200;

    public static void ApplyGravity(KinematicRigidBody2DComponent kinematicBody)
    {
        kinematicBody.LinearVelocity += Gravity * GameTime.FixedDeltaTimeSeconds;

        if (kinematicBody.LinearVelocity.Y < MaxFallVelocity)
        {
            kinematicBody.LinearVelocity = kinematicBody.LinearVelocity.WithY(MaxFallVelocity);
        }
    }

    public static void UpdateHorizontalSpriteFacing(Transform2DComponent transform, KinematicRigidBody2DComponent kinematicBody)
    {
        if (kinematicBody.LinearVelocity.X > 0)
        {
            transform.SetTransformImmediate(transform.Transform with { Scale = new Vector2(-1, 1) });
        }

        if (kinematicBody.LinearVelocity.X < 0)
        {
            transform.SetTransformImmediate(transform.Transform with { Scale = new Vector2(1, 1) });
        }
    }

    public static void UpdateVerticalSpriteFacing(Transform2DComponent transform, KinematicRigidBody2DComponent kinematicBody)
    {
        if (kinematicBody.LinearVelocity.Y > 0)
        {
            transform.SetTransformImmediate(transform.Transform with { Scale = new Vector2(1, 1) });
        }

        if (kinematicBody.LinearVelocity.Y < 0)
        {
            transform.SetTransformImmediate(transform.Transform with { Scale = new Vector2(1, -1) });
        }
    }
}