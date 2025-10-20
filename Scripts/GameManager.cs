using Godot;
using System;
using System.Collections.Generic;

namespace MinecraftClone
{
    public partial class GameManager : Node3D
    {
        [Export] public PackedScene AnimalScene;
        [Export] public PackedScene ChestScene;
        [Export] public int AnimalSpawnRadius = 50;
        [Export] public int MaxAnimals = 20;
        [Export] public float AnimalSpawnInterval = 5.0f;

        private VoxelWorld world;
        private FirstPersonController player;
        private Inventory playerInventory;
        private List<Animal> animals = new List<Animal>();
        private float animalSpawnTimer = 0.0f;
        private RandomNumberGenerator rng = new RandomNumberGenerator();

        // UI Elements
        private Control hudControl;
        private Label healthLabel;
        private Label toolLabel;
        private ProgressBar healthBar;
        private HBoxContainer hotbar;

        public override void _Ready()
        {
            rng.Randomize();

            // Get references
            world = GetNode<VoxelWorld>("World");
            player = GetNode<FirstPersonController>("Player");

            // Initially disable player physics until world is ready
            player.SetPhysicsProcess(false);
            player.SetProcessInput(false);

            // Wait for world to be ready
            world.WorldReady += OnWorldReady;

            // Create and add inventory to player
            playerInventory = new Inventory();
            player.AddChild(playerInventory);

            // Set player reference in world
            world.SetPlayer(player);

            // Create HUD
            CreateHUD();

            // Give player starting items
            GiveStartingItems();

            // Connect signals
            playerInventory.InventoryChanged += OnInventoryChanged;
            playerInventory.HotbarSelectionChanged += OnHotbarSelectionChanged;
        }

        private void OnWorldReady()
        {
            // Find a safe spawn position
            Vector3 spawnPos = FindSafeSpawnPosition();
            player.GlobalPosition = spawnPos;

            // Enable player
            player.SetPhysicsProcess(true);
            player.SetProcessInput(true);

            // Spawn initial animals after a short delay
            GetTree().CreateTimer(1.0).Timeout += SpawnInitialAnimals;
        }

        private Vector3 FindSafeSpawnPosition()
        {
            // Start from center and work up until we find air above ground
            for (int y = 100; y > 0; y--)
            {
                Vector3 checkPos = new Vector3(0, y, 0);
                BlockType block = world.GetBlock(checkPos);
                BlockType blockBelow = world.GetBlock(checkPos + Vector3.Down);

                // Found air with solid ground below
                if (block == BlockType.Air && blockBelow != BlockType.Air && blockBelow != BlockType.Water)
                {
                    return checkPos + Vector3.Up; // Add a bit of height to avoid clipping
                }
            }

            // Fallback position
            return new Vector3(0, 80, 0);
        }

        private void CreateHUD()
        {
            hudControl = new Control();
            hudControl.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(hudControl);

            // Health display
            var healthContainer = new VBoxContainer();
            healthContainer.Position = new Vector2(20, 20);
            hudControl.AddChild(healthContainer);

            healthLabel = new Label();
            healthLabel.Text = "Gesundheit: 100/100";
            healthLabel.AddThemeFontSizeOverride("font_size", 20);
            healthContainer.AddChild(healthLabel);

            healthBar = new ProgressBar();
            healthBar.Value = 100;
            healthBar.CustomMinimumSize = new Vector2(200, 20);
            healthBar.ShowPercentage = false;

            var healthBarStyle = new StyleBoxFlat();
            healthBarStyle.BgColor = new Color(0.2f, 0.2f, 0.2f);
            healthBar.AddThemeStyleboxOverride("background", healthBarStyle);

            var healthBarFillStyle = new StyleBoxFlat();
            healthBarFillStyle.BgColor = new Color(0.8f, 0.2f, 0.2f);
            healthBar.AddThemeStyleboxOverride("fill", healthBarFillStyle);

            healthContainer.AddChild(healthBar);

            // Current tool display
            toolLabel = new Label();
            toolLabel.Text = "Werkzeug: Hand";
            toolLabel.AddThemeFontSizeOverride("font_size", 18);
            toolLabel.Position = new Vector2(20, 80);
            hudControl.AddChild(toolLabel);

            // Hotbar
            CreateHotbar();

            // Crosshair
            var crosshair = new Control();
            crosshair.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            hudControl.AddChild(crosshair);

            var crosshairH = new ColorRect();
            crosshairH.Color = Colors.White;
            crosshairH.Size = new Vector2(20, 2);
            crosshairH.Position = new Vector2(-10, -1);
            crosshair.AddChild(crosshairH);

            var crosshairV = new ColorRect();
            crosshairV.Color = Colors.White;
            crosshairV.Size = new Vector2(2, 20);
            crosshairV.Position = new Vector2(-1, -10);
            crosshair.AddChild(crosshairV);
        }

        private void CreateHotbar()
        {
            hotbar = new HBoxContainer();
            hotbar.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterBottom);
            hotbar.Position = new Vector2(0, -100);
            hotbar.AddThemeConstantOverride("separation", 5);
            hudControl.AddChild(hotbar);

            // Create 9 hotbar slots
            for (int i = 0; i < 9; i++)
            {
                var slot = new Panel();
                slot.CustomMinimumSize = new Vector2(64, 64);

                var slotStyle = new StyleBoxFlat();
                slotStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                slotStyle.BorderWidthTop = 2;
                slotStyle.BorderWidthBottom = 2;
                slotStyle.BorderWidthLeft = 2;
                slotStyle.BorderWidthRight = 2;
                slotStyle.BorderColor = i == 0 ? Colors.Yellow : new Color(0.5f, 0.5f, 0.5f);
                slot.AddThemeStyleboxOverride("panel", slotStyle);

                // Slot number
                var slotNumber = new Label();
                slotNumber.Text = (i + 1).ToString();
                slotNumber.Position = new Vector2(5, 5);
                slot.AddChild(slotNumber);

                // Item icon placeholder
                var itemLabel = new Label();
                itemLabel.Name = $"ItemLabel{i}";
                itemLabel.Position = new Vector2(20, 20);
                itemLabel.Size = new Vector2(44, 44);
                itemLabel.HorizontalAlignment = HorizontalAlignment.Center;
                itemLabel.VerticalAlignment = VerticalAlignment.Center;
                slot.AddChild(itemLabel);

                hotbar.AddChild(slot);
            }
        }

        private void GiveStartingItems()
        {
            // Give player some starting items
            playerInventory.AddItem(ItemType.WoodenPickaxe, 1);
            playerInventory.AddItem(ItemType.WoodenSword, 1);
            playerInventory.AddItem(ItemType.Wood, 10);
            playerInventory.AddItem(ItemType.Apple, 5);
            playerInventory.AddItem(ItemType.Arrow, 20);
        }

        private void SpawnInitialAnimals()
        {
            // Spawn a variety of animals
            for (int i = 0; i < 10; i++)
            {
                SpawnRandomAnimal();
            }
        }

        private void SpawnRandomAnimal()
        {
            if (animals.Count >= MaxAnimals)
                return;

            // Choose random animal type
            var animalTypes = Enum.GetValues<AnimalType>();
            AnimalType randomType = animalTypes[rng.RandiRange(0, animalTypes.Length - 1)];

            // Find spawn position
            float angle = rng.Randf() * Mathf.Pi * 2;
            float distance = rng.RandfRange(10, AnimalSpawnRadius);
            Vector3 spawnPos = player.GlobalPosition + new Vector3(
                Mathf.Cos(angle) * distance,
                50, // Spawn high and let them fall
                Mathf.Sin(angle) * distance
            );

            // Create animal
            var animal = new Animal();
            animal.Type = randomType;
            animal.Position = spawnPos;

            AddChild(animal);
            animals.Add(animal);
        }

        public override void _Process(double delta)
        {
            // Animal spawning
            animalSpawnTimer += (float)delta;
            if (animalSpawnTimer >= AnimalSpawnInterval)
            {
                SpawnRandomAnimal();
                animalSpawnTimer = 0.0f;
            }

            // Update tool display
            UpdateToolDisplay();

            // Clean up dead animals
            animals.RemoveAll(a => !IsInstanceValid(a));
        }

        private void UpdateToolDisplay()
        {
            var currentTool = player.GetCurrentTool();
            toolLabel.Text = $"Werkzeug: {GetToolName(currentTool)}";
        }

        private string GetToolName(FirstPersonController.ToolType tool)
        {
            return tool switch
            {
                FirstPersonController.ToolType.Hand => "Hand",
                FirstPersonController.ToolType.Sword => "Schwert",
                FirstPersonController.ToolType.Pickaxe => "Spitzhacke",
                FirstPersonController.ToolType.Bow => "Bogen",
                _ => "Unbekannt"
            };
        }

        private void OnInventoryChanged()
        {
            UpdateHotbarDisplay();
        }

        private void OnHotbarSelectionChanged(int slot)
        {
            // Update hotbar selection highlight
            for (int i = 0; i < 9; i++)
            {
                var panel = hotbar.GetChild<Panel>(i);
                var style = panel.GetThemeStylebox("panel") as StyleBoxFlat;
                style.BorderColor = i == slot ? Colors.Yellow : new Color(0.5f, 0.5f, 0.5f);
                panel.AddThemeStyleboxOverride("panel", style);
            }
        }

        private void UpdateHotbarDisplay()
        {
            // Update item display in hotbar
            for (int i = 0; i < 9; i++)
            {
                var slot = hotbar.GetChild<Panel>(i);
                var itemLabel = slot.GetNode<Label>($"ItemLabel{i}");
                var item = playerInventory.GetHotbarSlot(i);

                if (item != null)
                {
                    itemLabel.Text = GetItemSymbol(item.Type) + "\n" + item.Count;
                }
                else
                {
                    itemLabel.Text = "";
                }
            }
        }

        private string GetItemSymbol(ItemType type)
        {
            return type switch
            {
                ItemType.Wood => "🪵",
                ItemType.Stone => "🪨",
                ItemType.Apple => "🍎",
                ItemType.Meat => "🍖",
                ItemType.WoodenPickaxe => "⛏️",
                ItemType.WoodenSword => "🗡️",
                ItemType.Bow => "🏹",
                ItemType.Arrow => "➡️",
                _ => "📦"
            };
        }

        public void SpawnChest(Vector3 position)
        {
            if (ChestScene == null)
            {
                // Create chest programmatically if no scene
                var chest = new Chest();
                chest.Position = position;
                AddChild(chest);
            }
            else
            {
                var chest = ChestScene.Instantiate<Chest>();
                chest.Position = position;
                AddChild(chest);
            }
        }

        public VoxelWorld GetWorld()
        {
            return world;
        }

        public FirstPersonController GetPlayer()
        {
            return player;
        }

        public Inventory GetPlayerInventory()
        {
            return playerInventory;
        }
    }
}