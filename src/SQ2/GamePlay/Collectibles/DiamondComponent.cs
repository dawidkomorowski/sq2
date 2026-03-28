using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering.Components;
using SQ2.Core;
using SQ2.GamePlay.Common;

namespace SQ2.GamePlay.Collectibles;

internal sealed class DiamondComponent : BehaviorComponent, IRespawnable
{
    private readonly GameSaveService _gameSaveService;
    private Transform2DComponent _transform2DComponent = null!;
    private SpriteRendererComponent _spriteRendererComponent = null!;
    private Transform2DComponent _playerTransform = null!;
    private bool _isCollected;

    public DiamondComponent(Entity entity, GameSaveService gameSaveService) : base(entity)
    {
        _gameSaveService = gameSaveService;
    }

    public string Id { get; set; } = string.Empty;

    public override void OnStart()
    {
        _transform2DComponent = Entity.GetComponent<Transform2DComponent>();
        _spriteRendererComponent = Entity.GetComponent<SpriteRendererComponent>();
        _playerTransform = Query.GetPlayerTransform2DComponent(Scene);

        if (_gameSaveService.GameSave.CollectedDiamondIds.Contains(Id))
        {
            _isCollected = true;
            _spriteRendererComponent.Visible = false;
        }
    }

    public override void OnFixedUpdate()
    {
        if (_isCollected) return;

        var distanceToPlayer = _transform2DComponent.Translation.Distance(_playerTransform.Translation);
        if (distanceToPlayer < 13)
        {
            _isCollected = true;
            _spriteRendererComponent.Visible = false;
            _gameSaveService.GameSave.CollectedDiamondIds.Add(Id);
            _gameSaveService.SaveGame();
        }
    }

    public void Respawn()
    {
        if (_gameSaveService.GameSave.CollectedDiamondIds.Contains(Id)) return;

        _isCollected = false;
        _spriteRendererComponent.Visible = true;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DiamondComponentFactory : ComponentFactory<DiamondComponent>
{
    private readonly GameSaveService _gameSaveService;

    public DiamondComponentFactory(GameSaveService gameSaveService)
    {
        _gameSaveService = gameSaveService;
    }

    protected override DiamondComponent CreateComponent(Entity entity) => new(entity, _gameSaveService);
}