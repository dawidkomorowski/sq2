using System.Linq;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Player;

namespace SQ2.GamePlay.LevelGeometry;

internal sealed class DoorComponent : BehaviorComponent
{
    private Transform2DComponent _playerTransform2DComponent = null!;
    private PlayerComponent _playerComponent = null!;
    private Vector2 _exitPosition;
    private AxisAlignedRectangle _hitbox;

    public DoorComponent(Entity entity) : base(entity)
    {
    }

    public int ObjectId { get; set; }
    public int ExitObjectId { get; set; }
    public bool UpdateCameraPosition { get; set; }

    public override void OnStart()
    {
        var transform2DComponent = Entity.GetComponent<Transform2DComponent>();

        _playerTransform2DComponent = Query.GetPlayerTransform2DComponent(Scene);
        _playerComponent = Query.GetPlayerComponent(Scene);

        var exitEntity = Scene.RootEntities.FirstOrDefault(e => e.HasComponent<DoorComponent>() && ExitObjectId == e.GetComponent<DoorComponent>().ObjectId);
        _exitPosition = exitEntity?.GetComponent<Transform2DComponent>().Translation ?? transform2DComponent.Translation;

        _hitbox = new AxisAlignedRectangle(transform2DComponent.Translation, new Vector2(9, 9));
    }

    public override void OnFixedUpdate()
    {
        var playerHitBox = new AxisAlignedRectangle(_playerTransform2DComponent.Translation, new Vector2(9, 9));
        if (playerHitBox.Overlaps(_hitbox))
        {
            _playerComponent.DoorInRange = this;
        }
        else if (_playerComponent.DoorInRange == this)
        {
            _playerComponent.DoorInRange = null;
        }
    }

    public void EnterDoor()
    {
        _playerComponent.TeleportTo(_exitPosition, UpdateCameraPosition);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DoorComponentFactory : ComponentFactory<DoorComponent>
{
    protected override DoorComponent CreateComponent(Entity entity) => new(entity);
}