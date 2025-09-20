using System.Reflection;
using Geisha.Engine;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;

namespace SQ2;

// ReSharper disable once InconsistentNaming
internal class SQ2Game : Game
{
    private static string EngineInformation => $"Geisha Engine {Assembly.GetAssembly(typeof(Game))?.GetName().Version?.ToString(3)}";
    private static string GameInformation => $"SQ2 {Assembly.GetAssembly(typeof(SQ2Game))?.GetName().Version?.ToString(3)}";
    public override string WindowTitle => $"{GameInformation} - {EngineInformation}";

    public override Configuration Configure(Configuration configuration) =>
        configuration with
        {
            Core = configuration.Core with
            {
                StartUpSceneBehavior = "GameWorld",
                ShowFps = true,
                ShowRootEntitiesCount = true
            },
            Rendering = configuration.Rendering with
            {
                ScreenWidth = DevConfig.WindowSize?.Width ?? GlobalSettings.WindowSize.Width,
                ScreenHeight = DevConfig.WindowSize?.Height ?? GlobalSettings.WindowSize.Height,
                EnableVSync = true
            },
            Physics = configuration.Physics with
            {
                TileSize = GlobalSettings.TileSize,
                RenderCollisionGeometry = false
            }
        };

    public override void RegisterComponents(IComponentsRegistry componentsRegistry)
    {
        // Common services
        componentsRegistry.RegisterSingleInstance<EntityFactory>();
        componentsRegistry.RegisterSingleInstance<MapLoader>();

        // Scene behaviors
        componentsRegistry.RegisterSceneBehaviorFactory<GameWorldBehaviorFactory>();

        // Development Components
        componentsRegistry.RegisterComponentFactory<DevControlsComponentFactory>();

        // GamePlay Components
        // Player
        componentsRegistry.RegisterComponentFactory<CameraMovementComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerSpawnPointComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerCheckPointComponentFactory>();
        // Enemies
        componentsRegistry.RegisterComponentFactory<EnemyComponentFactory>();
        componentsRegistry.RegisterComponentFactory<YellowEnemyComponentFactory>();
        // Level Geometry
        componentsRegistry.RegisterComponentFactory<SpikesComponentFactory>();
        componentsRegistry.RegisterComponentFactory<DropPlatformComponentFactory>();
        componentsRegistry.RegisterComponentFactory<JumpPadComponentFactory>();
    }
}