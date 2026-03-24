using Geisha.Engine.Core;
using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.UI;

internal sealed class NumberRendererComponent : BehaviorComponent
{
    private SpriteRendererComponent _spriteRendererComponent1 = null!;
    private SpriteRendererComponent _spriteRendererComponent10 = null!;
    private SpriteRendererComponent _spriteRendererComponent100 = null!;

    public NumberRendererComponent(Entity entity) : base(entity)
    {
    }

    public string SortingLayerName { get; set; } = GlobalSettings.SortingLayers.UI;
    public Sprite? Digit0Sprite { get; set; }
    public Sprite? Digit1Sprite { get; set; }
    public Sprite? Digit2Sprite { get; set; }
    public Sprite? Digit3Sprite { get; set; }
    public Sprite? Digit4Sprite { get; set; }
    public Sprite? Digit5Sprite { get; set; }
    public Sprite? Digit6Sprite { get; set; }
    public Sprite? Digit7Sprite { get; set; }
    public Sprite? Digit8Sprite { get; set; }
    public Sprite? Digit9Sprite { get; set; }

    public int Value { get; set; }

    public override void OnStart()
    {
        const double spacing = 11;

        var entity1 = Entity.CreateChildEntity();
        var transform2DComponent1 = entity1.CreateComponent<Transform2DComponent>();
        transform2DComponent1.Translation = new Vector2(spacing, 0);
        _spriteRendererComponent1 = entity1.CreateComponent<SpriteRendererComponent>();
        _spriteRendererComponent1.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        _spriteRendererComponent1.SortingLayerName = SortingLayerName;
        _spriteRendererComponent1.Sprite = Digit0Sprite;

        var entity10 = Entity.CreateChildEntity();
        var transform2DComponent10 = entity10.CreateComponent<Transform2DComponent>();
        _spriteRendererComponent10 = entity10.CreateComponent<SpriteRendererComponent>();
        _spriteRendererComponent10.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        _spriteRendererComponent10.SortingLayerName = SortingLayerName;
        _spriteRendererComponent10.Sprite = Digit0Sprite;

        var entity100 = Entity.CreateChildEntity();
        var transform2DComponent100 = entity100.CreateComponent<Transform2DComponent>();
        transform2DComponent100.Translation = new Vector2(-spacing, 0);
        _spriteRendererComponent100 = entity100.CreateComponent<SpriteRendererComponent>();
        _spriteRendererComponent100.BitmapInterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        _spriteRendererComponent100.SortingLayerName = SortingLayerName;
        _spriteRendererComponent100.Sprite = Digit0Sprite;
    }

    public override void OnUpdate(GameTime gameTime)
    {
        var onesDigit = Value % 10;
        var tensDigit = (Value / 10) % 10;
        var hundredsDigit = (Value / 100) % 10;

        _spriteRendererComponent1.Sprite = GetSpriteForDigit(onesDigit);
        _spriteRendererComponent10.Sprite = GetSpriteForDigit(tensDigit);
        _spriteRendererComponent100.Sprite = GetSpriteForDigit(hundredsDigit);
    }

    private Sprite? GetSpriteForDigit(int digit) => digit switch
    {
        0 => Digit0Sprite,
        1 => Digit1Sprite,
        2 => Digit2Sprite,
        3 => Digit3Sprite,
        4 => Digit4Sprite,
        5 => Digit5Sprite,
        6 => Digit6Sprite,
        7 => Digit7Sprite,
        8 => Digit8Sprite,
        9 => Digit9Sprite,
        _ => null
    };
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class NumberRendererComponentFactory : ComponentFactory<NumberRendererComponent>
{
    protected override NumberRendererComponent CreateComponent(Entity entity) => new(entity);
}