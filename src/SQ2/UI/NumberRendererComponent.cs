using Geisha.Engine.Core;
using Geisha.Engine.Core.Assets;
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

    public string SortingLayerName { get; set; } = GlobalSettings.SortingLayers.Hud;
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

    public void UseDefaultSprites(IAssetStore assetStore)
    {
        Digit0Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("7b973a46-3c17-4d6c-a20a-97214db7086a"));
        Digit1Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("19398cc8-0a86-4aee-9e82-0a95bb4317c7"));
        Digit2Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("71fa41ca-2162-47de-a7b1-de7462bd1cb0"));
        Digit3Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("aacd2f7c-5145-4b8c-b7f6-1342e3a7d8ad"));
        Digit4Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("72126ad3-6cd7-4c39-b3b7-266bb614686d"));
        Digit5Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("9eeb80fa-485b-4f21-8db9-0a3412796863"));
        Digit6Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("55a98703-d4e5-4f7e-b15e-b2a169cb1b93"));
        Digit7Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("6fd29513-cf28-4678-aeb6-40d2a93c8817"));
        Digit8Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("2db0e507-4e38-4d61-8c65-aca6aba26a20"));
        Digit9Sprite = assetStore.GetAsset<Sprite>(AssetId.Parse("74fe02bb-b7fc-4f9d-882f-fe584de17c26"));
    }

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

    public override void OnUpdate(in TimeStep timeStep)
    {
        var onesDigit = Value % 10;
        var tensDigit = (Value / 10) % 10;
        var hundredsDigit = (Value / 100) % 10;

        _spriteRendererComponent1.Sprite = GetSpriteForDigit(onesDigit);
        _spriteRendererComponent10.Sprite = GetSpriteForDigit(tensDigit);
        _spriteRendererComponent100.Sprite = GetSpriteForDigit(hundredsDigit);

        if (hundredsDigit == 0)
        {
            _spriteRendererComponent100.Visible = false;
            _spriteRendererComponent10.Visible = tensDigit != 0;
        }
        else
        {
            _spriteRendererComponent100.Visible = true;
        }
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