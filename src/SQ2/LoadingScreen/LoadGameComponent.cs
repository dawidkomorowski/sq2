using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.SceneModel;

namespace SQ2.LoadingScreen;

internal sealed class LoadGameComponent : BehaviorComponent
{
    private readonly ISceneManager _sceneManager;

    public LoadGameComponent(Entity entity, ISceneManager sceneManager) : base(entity)
    {
        _sceneManager = sceneManager;
    }

    public override void OnStart()
    {
        _sceneManager.LoadEmptyScene(GlobalSettings.SceneNames.GameWorld);
    }
}

internal sealed class LoadGameComponentFactory : ComponentFactory<LoadGameComponent>
{
    private readonly ISceneManager _sceneManager;

    public LoadGameComponentFactory(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    protected override LoadGameComponent CreateComponent(Entity entity) => new(entity, _sceneManager);
}