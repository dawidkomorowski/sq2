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

namespace SQ2.GamePlay.Boss.Worm;

internal sealed class WormBossComponent : BehaviorComponent, IWormBossController, IRespawnable
{
    private Transform2DComponent _headTransform = null!;
    private CircleColliderComponent _headCollider = null!;
    private KinematicRigidBody2DComponent _headKinematicBody = null!;

    private Vector2 _initialPosition;

    private readonly List<Contact2D> _contacts = new();

    private Vector2 _moveToPosition;

    public WormBossComponent(Entity entity) : base(entity)
    {
    }

    public List<Entity> Tail { get; } = new();
    public IWormBossAI WormBossAI { get; set; } = new NopWormBossAI();

    public override void OnStart()
    {
        _headTransform = Entity.GetComponent<Transform2DComponent>();
        _headCollider = Entity.GetComponent<CircleColliderComponent>();
        _headKinematicBody = Entity.GetComponent<KinematicRigidBody2DComponent>();

        _initialPosition = _headTransform.Translation;
        _moveToPosition = _headTransform.Translation;

        WormBossAI.Init(Entity, this);
    }

    public override void OnFixedUpdate()
    {
        if (ProcessPlayerCollision())
        {
            return;
        }

        ProcessAI();
        ProcessHeadMovement();
        ProcessTailMovement();
    }

    public void MoveTo(Vector2 position)
    {
        _moveToPosition = position;
    }

    public bool HasReachedDestination()
    {
        return _headTransform.Translation.Distance(_moveToPosition) < 10;
    }

    public Vector2 GetHeadPosition() => _headTransform.Translation;

    public void SetHeadPosition(Vector2 position)
    {
        _headTransform.Translation = position;
    }

    private bool ProcessPlayerCollision()
    {
        var contacts = _headCollider.GetContactsAsSpan(_contacts);
        if (KillPlayerOnContact(contacts))
        {
            return true;
        }

        foreach (var entity in Tail)
        {
            var collider = entity.GetComponent<CircleColliderComponent>();
            contacts = collider.GetContactsAsSpan(_contacts);
            if (KillPlayerOnContact(contacts))
            {
                return true;
            }
        }

        return false;
    }

    private static bool KillPlayerOnContact(ReadOnlySpan<Contact2D> contacts)
    {
        foreach (var contact in contacts)
        {
            if (contact.OtherCollider.Entity.HasComponent<PlayerComponent>())
            {
                var playerComponent = contact.OtherCollider.Entity.GetComponent<PlayerComponent>();
                playerComponent.KillPlayer();
                return true;
            }
        }

        return false;
    }

    private void ProcessAI()
    {
        WormBossAI.Update(TimeStep.FixedDeltaTime);
    }

    private void ProcessHeadMovement()
    {
        var distance = _headTransform.Translation.Distance(_moveToPosition);
        if (distance > 10)
        {
            const double velocity = 30;
            var direction = (_moveToPosition - _headTransform.Translation).Unit;
            _headTransform.Translation += direction * velocity * TimeStep.FixedDeltaTimeSeconds;
        }
    }

    private void ProcessTailMovement()
    {
        const double minDistance = 8;
        const double maxDistance = 10;

        var segment1 = Entity;

        for (var i = 0; i < Tail.Count; i++)
        {
            var segment2 = Tail[i];

            var transform1 = segment1.GetComponent<Transform2DComponent>();
            var transform2 = segment2.GetComponent<Transform2DComponent>();

            var distance = transform1.Translation.Distance(transform2.Translation);
            if (distance > maxDistance)
            {
                var translationFrom1To2 = transform2.Translation - transform1.Translation;
                transform2.Translation = transform1.Translation + translationFrom1To2.OfLength(maxDistance);
            }

            if (distance < minDistance && i + 1 < Tail.Count)
            {
                var segment3 = Tail[i + 1];
                var transform3 = segment3.GetComponent<Transform2DComponent>();
                var translationFrom2To3 = transform3.Translation - transform2.Translation;
                transform2.Translation += translationFrom2To3.OfLength(Math.Abs(minDistance - distance));
            }


            // Chain segment pairs.
            segment1 = segment2;
        }
    }

    public void Respawn()
    {
        _moveToPosition = _initialPosition;

        _headTransform.Translation = _initialPosition;
        _headKinematicBody.LinearVelocity = Vector2.Zero;
        foreach (var entity in Tail)
        {
            var transform = entity.GetComponent<Transform2DComponent>();
            transform.Translation = _initialPosition;
        }

        WormBossAI.Reset();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WormBossComponentFactory : ComponentFactory<WormBossComponent>
{
    protected override WormBossComponent CreateComponent(Entity entity) => new(entity);
}