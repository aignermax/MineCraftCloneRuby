using Godot;
using System;

namespace MinecraftClone
{
    public partial class MainMenu : Control
    {
        private Button playButton;
        private Button settingsButton;
        private Button quitButton;
        private Label titleLabel;
        private PackedScene gameScene;

        public override void _Ready()
        {
            // Load game scene
            gameScene = GD.Load<PackedScene>("res://Game.tscn");

            CreateUI();
            ConnectSignals();

            // Set window mode
            GetWindow().Mode = Window.ModeEnum.Windowed;
            GetWindow().Size = new Vector2I(1280, 720);
        }

        private void CreateUI()
        {
            // Background
            var background = new ColorRect();
            background.Color = new Color(0.1f, 0.1f, 0.2f);
            background.AnchorRight = 1;
            background.AnchorBottom = 1;
            AddChild(background);

            // Title
            titleLabel = new Label();
            titleLabel.Text = "Minecraft Clone";
            titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            titleLabel.AddThemeFontSizeOverride("font_size", 48);
            titleLabel.Position = new Vector2(0, 100);
            titleLabel.Size = new Vector2(1280, 100);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            AddChild(titleLabel);

            // Menu container
            var menuContainer = new VBoxContainer();
            menuContainer.Position = new Vector2(490, 300);
            menuContainer.Size = new Vector2(300, 400);
            menuContainer.AddThemeConstantOverride("separation", 20);
            AddChild(menuContainer);

            // Play button
            playButton = new Button();
            playButton.Text = "Spiel starten";
            playButton.CustomMinimumSize = new Vector2(300, 60);
            menuContainer.AddChild(playButton);

            // Settings button
            settingsButton = new Button();
            settingsButton.Text = "Einstellungen";
            settingsButton.CustomMinimumSize = new Vector2(300, 60);
            menuContainer.AddChild(settingsButton);

            // Quit button
            quitButton = new Button();
            quitButton.Text = "Beenden";
            quitButton.CustomMinimumSize = new Vector2(300, 60);
            menuContainer.AddChild(quitButton);

            // Style buttons
            StyleButton(playButton);
            StyleButton(settingsButton);
            StyleButton(quitButton);
        }

        private void StyleButton(Button button)
        {
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = new Color(0.3f, 0.3f, 0.4f);
            normalStyle.BorderWidthTop = 2;
            normalStyle.BorderWidthBottom = 2;
            normalStyle.BorderWidthLeft = 2;
            normalStyle.BorderWidthRight = 2;
            normalStyle.BorderColor = new Color(0.5f, 0.5f, 0.6f);
            normalStyle.CornerRadiusTopLeft = 5;
            normalStyle.CornerRadiusTopRight = 5;
            normalStyle.CornerRadiusBottomLeft = 5;
            normalStyle.CornerRadiusBottomRight = 5;

            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = new Color(0.4f, 0.4f, 0.5f);
            hoverStyle.BorderWidthTop = 2;
            hoverStyle.BorderWidthBottom = 2;
            hoverStyle.BorderWidthLeft = 2;
            hoverStyle.BorderWidthRight = 2;
            hoverStyle.BorderColor = new Color(0.6f, 0.6f, 0.7f);
            hoverStyle.CornerRadiusTopLeft = 5;
            hoverStyle.CornerRadiusTopRight = 5;
            hoverStyle.CornerRadiusBottomLeft = 5;
            hoverStyle.CornerRadiusBottomRight = 5;

            var pressedStyle = new StyleBoxFlat();
            pressedStyle.BgColor = new Color(0.2f, 0.2f, 0.3f);
            pressedStyle.BorderWidthTop = 2;
            pressedStyle.BorderWidthBottom = 2;
            pressedStyle.BorderWidthLeft = 2;
            pressedStyle.BorderWidthRight = 2;
            pressedStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
            pressedStyle.CornerRadiusTopLeft = 5;
            pressedStyle.CornerRadiusTopRight = 5;
            pressedStyle.CornerRadiusBottomLeft = 5;
            pressedStyle.CornerRadiusBottomRight = 5;

            button.AddThemeStyleboxOverride("normal", normalStyle);
            button.AddThemeStyleboxOverride("hover", hoverStyle);
            button.AddThemeStyleboxOverride("pressed", pressedStyle);
            button.AddThemeFontSizeOverride("font_size", 20);
        }

        private void ConnectSignals()
        {
            playButton.Pressed += OnPlayPressed;
            settingsButton.Pressed += OnSettingsPressed;
            quitButton.Pressed += OnQuitPressed;
        }

        private void OnPlayPressed()
        {
            GetTree().ChangeSceneToPacked(gameScene);
        }

        private void OnSettingsPressed()
        {
            // Create settings popup
            var settingsPopup = new AcceptDialog();
            settingsPopup.Title = "Einstellungen";
            settingsPopup.Size = new Vector2I(400, 300);
            settingsPopup.Position = new Vector2I(440, 210);

            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(20, 20);
            settingsPopup.AddChild(vbox);

            // Volume slider
            var volumeLabel = new Label();
            volumeLabel.Text = "Lautstärke";
            vbox.AddChild(volumeLabel);

            var volumeSlider = new HSlider();
            volumeSlider.MinValue = 0;
            volumeSlider.MaxValue = 100;
            volumeSlider.Value = 50;
            volumeSlider.CustomMinimumSize = new Vector2(300, 20);
            vbox.AddChild(volumeSlider);

            // Graphics quality
            var graphicsLabel = new Label();
            graphicsLabel.Text = "Grafikqualität";
            vbox.AddChild(graphicsLabel);

            var graphicsOption = new OptionButton();
            graphicsOption.AddItem("Niedrig");
            graphicsOption.AddItem("Mittel");
            graphicsOption.AddItem("Hoch");
            graphicsOption.Selected = 1;
            vbox.AddChild(graphicsOption);

            AddChild(settingsPopup);
            settingsPopup.Show();
        }

        private void OnQuitPressed()
        {
            GetTree().Quit();
        }
    }
}