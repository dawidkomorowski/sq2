using Geisha.Engine.Core.Components;
using Geisha.Engine.Core.Math;
using Geisha.Engine.Core.SceneModel;
using Geisha.Engine.Rendering;
using Geisha.Engine.Rendering.Components;

namespace SQ2.LoadingScreen;

internal sealed class LoadingScreenBehaviorFactory : ISceneBehaviorFactory
{
    private const string SceneBehaviorName = GlobalSettings.SceneNames.LoadingScreen;
    public string BehaviorName => SceneBehaviorName;

    public SceneBehavior Create(Scene scene) => new LoadingScreenBehavior(scene);

    private sealed class LoadingScreenBehavior : SceneBehavior
    {
        public LoadingScreenBehavior(Scene scene) : base(scene)
        {
        }

        public override string Name => SceneBehaviorName;

        protected override void OnLoaded()
        {
            var cameraEntity = Scene.CreateEntity();
            cameraEntity.CreateComponent<Transform2DComponent>();
            var cameraComponent = cameraEntity.CreateComponent<CameraComponent>();
            cameraComponent.ViewRectangle = GlobalSettings.ViewSize;
            cameraComponent.AspectRatioBehavior = AspectRatioBehavior.Underscan;

            var backgroundEntity = Scene.CreateEntity();
            backgroundEntity.CreateComponent<Transform2DComponent>();
            var backgroundRenderer = backgroundEntity.CreateComponent<RectangleRendererComponent>();
            backgroundRenderer.Dimensions = GlobalSettings.ViewSize * 2;
            backgroundRenderer.OrderInLayer = 0;
            backgroundRenderer.FillInterior = true;
            backgroundRenderer.Color = Color.Black;

            var textEntity = Scene.CreateEntity();
            textEntity.CreateComponent<Transform2DComponent>();
            var textRenderer = textEntity.CreateComponent<TextRendererComponent>();
            textRenderer.OrderInLayer = 1;
            textRenderer.FontSize = FontSize.FromDips(20);
            textRenderer.FontFamilyName = GlobalSettings.Font;
            textRenderer.Color = Color.White;
            textRenderer.TextAlignment = TextAlignment.Center;
            textRenderer.ParagraphAlignment = ParagraphAlignment.Center;
            textRenderer.MaxWidth = 500;
            textRenderer.MaxHeight = 500;
            textRenderer.Pivot = new Vector2(250, 250);
            textRenderer.Text = "Loading...";

            var loadGameEntity = Scene.CreateEntity();
            loadGameEntity.CreateComponent<LoadGameComponent>();
        }
    }
}