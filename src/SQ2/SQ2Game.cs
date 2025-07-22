using System.Reflection;
using Geisha.Engine;

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
                StartUpSceneBehavior = "Default",
                ShowFps = true
            },
            Rendering = configuration.Rendering with
            {
                ScreenWidth = GlobalSettings.WindowSize.Width,
                ScreenHeight = GlobalSettings.WindowSize.Height
            },
            Physics = configuration.Physics with
            {
                TileSize = GlobalSettings.TileSize,
                RenderCollisionGeometry = true
            }
        };

    public override void RegisterComponents(IComponentsRegistry componentsRegistry)
    {
        // Common services
        componentsRegistry.RegisterSingleInstance<EntityFactory>();

        // Scene behaviors
        componentsRegistry.RegisterSceneBehaviorFactory<DefaultSceneBehaviorFactory>();

        // Components
        componentsRegistry.RegisterComponentFactory<DevControlsComponentFactory>();
        componentsRegistry.RegisterComponentFactory<PlayerComponentFactory>();
    }
}