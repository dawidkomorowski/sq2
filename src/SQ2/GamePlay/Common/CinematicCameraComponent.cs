using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;

namespace SQ2.GamePlay.Common;

internal sealed class CinematicCameraComponent : BehaviorComponent
{
    private Transform2DComponent _topBarTransform = null!;
    private Transform2DComponent _bottomBarTransform = null!;
    private RectangleRendererComponent _topBarRenderer = null!;
    private RectangleRendererComponent _bottomBarRenderer = null!;

    private readonly Vector2 _topBarOutOfViewPosition = new(0, 200);
    private readonly Vector2 _topBarInViewPosition = new(0, 110);
    private readonly Vector2 _bottomBarOutOfViewPosition = new(0, -200);
    private readonly Vector2 _bottomBarInViewPosition = new(0, -110);

    private Vector2 _topBarTargetPosition;
    private Vector2 _bottomBarTargetPosition;

    public CinematicCameraComponent(Entity entity) : base(entity)
    {
    }

    public override void OnStart()
    {
        _topBarTargetPosition = _topBarOutOfViewPosition;
        _bottomBarTargetPosition = _bottomBarOutOfViewPosition;

        var topBarEntity = Entity.CreateChildEntity();
        _topBarTransform = topBarEntity.CreateComponent<Transform2DComponent>();
        _topBarTransform.Translation = _topBarTargetPosition;
        _topBarRenderer = topBarEntity.CreateComponent<RectangleRendererComponent>();
        _topBarRenderer.SortingLayerName = GlobalSettings.SortingLayers.CameraEffects;
        _topBarRenderer.Color = Color.Black;
        _topBarRenderer.Dimensions = (GlobalSettings.ViewSize * 2).WithY(50);
        _topBarRenderer.FillInterior = true;
        _topBarRenderer.Visible = false;

        var bottomBarEntity = Entity.CreateChildEntity();
        _bottomBarTransform = bottomBarEntity.CreateComponent<Transform2DComponent>();
        _bottomBarTransform.Translation = _bottomBarTargetPosition;
        _bottomBarRenderer = bottomBarEntity.CreateComponent<RectangleRendererComponent>();
        _bottomBarRenderer.SortingLayerName = GlobalSettings.SortingLayers.CameraEffects;
        _bottomBarRenderer.Color = Color.Black;
        _bottomBarRenderer.Dimensions = (GlobalSettings.ViewSize * 2).WithY(50);
        _bottomBarRenderer.FillInterior = true;
        _bottomBarRenderer.Visible = false;
    }

    public override void OnUpdate(in TimeStep timeStep)
    {
        const double barMovementSpeed = 200;
        var distanceToMove = barMovementSpeed * timeStep.DeltaTimeSeconds;

        if (_topBarTransform.Translation != _topBarTargetPosition)
        {
            _topBarTransform.Translation = MoveTowards(_topBarTransform.Translation, _topBarTargetPosition, distanceToMove);
        }

        if (_bottomBarTransform.Translation != _bottomBarTargetPosition)
        {
            _bottomBarTransform.Translation = MoveTowards(_bottomBarTransform.Translation, _bottomBarTargetPosition, distanceToMove);
        }
    }

    public void Show()
    {
        _topBarRenderer.Visible = true;
        _bottomBarRenderer.Visible = true;
        _topBarTargetPosition = _topBarInViewPosition;
        _bottomBarTargetPosition = _bottomBarInViewPosition;
    }

    private static Vector2 MoveTowards(Vector2 current, Vector2 target, double maxDistanceDelta)
    {
        var toVector = target - current;
        var distance = toVector.Length;
        if (distance <= maxDistanceDelta || distance == 0)
            return target;
        return current + toVector.OfLength(maxDistanceDelta);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CinematicCameraComponentFactory : ComponentFactory<CinematicCameraComponent>
{
    protected override CinematicCameraComponent CreateComponent(Entity entity) => new(entity);
}