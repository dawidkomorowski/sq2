﻿using System.Reflection;
using Geisha.Engine;
using SQ2.Components.Development;
using SQ2.Components.GamePlay;

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
        componentsRegistry.RegisterComponentFactory<PlayerComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerSpawnPointComponentFactory>();
        componentsRegistry.RegisterComponentFactory<SpikesComponentFactory>();
    }
}