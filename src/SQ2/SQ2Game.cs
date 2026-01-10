using System.Reflection;
using Geisha.Engine;
using Geisha.Engine.Rendering;
using SQ2.Core;
using SQ2.Development;
using SQ2.GamePlay.Boss.Blue;
using SQ2.GamePlay.Common;
using SQ2.GamePlay.Enemies;
using SQ2.GamePlay.LevelGeometry;
using SQ2.GamePlay.Player;
using SQ2.VFX;

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
                CustomGameLoopSteps = new[] { "CheckPointSystem", "RespawnSystem" },
                ShowFps = true,
                ShowRootEntitiesCount = true,
                ShowGameLoopStatistics = false
            },
            Rendering = configuration.Rendering with
            {
                ScreenSize = DevConfig.WindowSize ?? GlobalSettings.WindowSize,
                SortingLayersOrder = new[]
                {
                    GlobalSettings.SortingLayers.Background,
                    GlobalSettings.SortingLayers.DecorBackground,
                    RenderingConfiguration.DefaultSortingLayerName,
                    GlobalSettings.SortingLayers.DecorForeground
                },
                EnableVSync = true
            },
            Physics = configuration.Physics with
            {
                TileSize = GlobalSettings.TileSize,
                EnableDebugRendering = false
            }
        };

    public override void RegisterComponents(IComponentsRegistry componentsRegistry)
    {
        // Common services
        componentsRegistry.RegisterSingleInstance<EntityFactory>();
        componentsRegistry.RegisterSingleInstance<MapLoader>();

        // Scene behaviors
        componentsRegistry.RegisterSceneBehaviorFactory<GameWorldBehaviorFactory>();

        // Development components
        componentsRegistry.RegisterComponentFactory<DevControlsComponentFactory>();

        // Gameplay systems and services
        componentsRegistry.RegisterSingleInstance<RespawnService>();
        componentsRegistry.RegisterSystem<RespawnSystem>();
        componentsRegistry.RegisterSystem<CheckPointSystem>();

        // Gameplay components
        // Player
        componentsRegistry.RegisterComponentFactory<CameraMovementComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerSpawnPointComponentFactory>();
        componentsRegistry.RegisterComponentFactory<CheckPointComponentFactory>();
        // Enemies
        componentsRegistry.RegisterComponentFactory<BlueEnemyComponentFactory>();
        componentsRegistry.RegisterComponentFactory<BlueEnemyDeathAnimationComponentFactory>();
        componentsRegistry.RegisterComponentFactory<RedEnemyComponentFactory>();
        componentsRegistry.RegisterComponentFactory<YellowEnemyComponentFactory>();
        componentsRegistry.RegisterComponentFactory<FishEnemyComponentFactory>();
        // Level Geometry
        componentsRegistry.RegisterComponentFactory<WaterDeepComponentFactory>();
        componentsRegistry.RegisterComponentFactory<SpikesComponentFactory>();
        componentsRegistry.RegisterComponentFactory<DropPlatformComponentFactory>();
        componentsRegistry.RegisterComponentFactory<MovingPlatformComponentFactory>();
        componentsRegistry.RegisterComponentFactory<VanishPlatformComponentFactory>();
        componentsRegistry.RegisterComponentFactory<JumpPadComponentFactory>();
        componentsRegistry.RegisterComponentFactory<LadderComponentFactory>();
        componentsRegistry.RegisterComponentFactory<ButtonComponentFactory>();
        componentsRegistry.RegisterComponentFactory<DestructibleWallComponentFactory>();
        // Bosses
        componentsRegistry.RegisterComponentFactory<BlueBossComponentFactory>();
        componentsRegistry.RegisterComponentFactory<BlueBossProjectileComponentFactory>();

        // VFX
        componentsRegistry.RegisterComponentFactory<WallParticleComponentFactory>();
        componentsRegistry.RegisterComponentFactory<BackgroundComponentFactory>();
    }
}