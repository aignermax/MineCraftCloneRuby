using Godot;
using System;

namespace MinecraftClone
{
    public partial class Chest : StaticBody3D
    {
        [Export] public int ChestSize = 27; // 9x3 slots

        private ItemStack[] slots;
        private bool isOpen = false;
        private Node3D interactingPlayer = null;
        private MeshInstance3D meshInstance;

        [Signal]
        public delegate void ChestOpenedEventHandler(Chest chest);

        [Signal]
        public delegate void ChestClosedEventHandler();

        public override void _Ready()
        {
            slots = new ItemStack[ChestSize];
            CreateChestMesh();

            // Add collision
            var collision = new CollisionShape3D();
            var boxShape = new BoxShape3D();
            boxShape.Size = new Vector3(0.9f, 0.9f, 0.9f);
            collision.Shape = boxShape;
            AddChild(collision);

            // Add interaction area
            var area = new Area3D();
            area.BodyEntered += OnBodyEntered;
            area.BodyExited += OnBodyExited;

            var areaShape = new CollisionShape3D();
            var sphereShape = new SphereShape3D();
            sphereShape.Radius = 2.0f;
            areaShape.Shape = sphereShape;
            area.AddChild(areaShape);
            AddChild(area);
        }

        private void CreateChestMesh()
        {
            meshInstance = new MeshInstance3D();

            // Create chest base
            var boxMesh = new BoxMesh();
            boxMesh.Size = new Vector3(0.9f, 0.9f, 0.9f);
            meshInstance.Mesh = boxMesh;

            // Create material
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.55f, 0.35f, 0.2f); // Wood color
            meshInstance.MaterialOverride = material;

            AddChild(meshInstance);

            // Add chest details
            CreateChestDetails();
        }

        private void CreateChestDetails()
        {
            // Add metal bands
            var metalMaterial = new StandardMaterial3D();
            metalMaterial.AlbedoColor = new Color(0.4f, 0.4f, 0.4f);

            // Horizontal band
            var band1 = new MeshInstance3D();
            var bandMesh1 = new BoxMesh();
            bandMesh1.Size = new Vector3(0.95f, 0.1f, 0.95f);
            band1.Mesh = bandMesh1;
            band1.Position = new Vector3(0, 0.3f, 0);
            band1.MaterialOverride = metalMaterial;
            meshInstance.AddChild(band1);

            // Lock
            var mylock = new MeshInstance3D();
            var lockMesh = new BoxMesh();
            lockMesh.Size = new Vector3(0.15f, 0.2f, 0.05f);
            mylock.Mesh = lockMesh;
            mylock.Position = new Vector3(0, 0, 0.47f);
            mylock.MaterialOverride = metalMaterial;
            meshInstance.AddChild(mylock) ;
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body.Name == "Player")
            {
                interactingPlayer = body;
                GD.Print("Press E to open chest");
            }
        }

        private void OnBodyExited(Node3D body)
        {
            if (body == interactingPlayer)
            {
                if (isOpen)
                {
                    CloseChest();
                }
                interactingPlayer = null;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (interactingPlayer != null && @event.IsActionPressed("interact"))
            {
                if (isOpen)
                {
                    CloseChest();
                }
                else
                {
                    OpenChest();
                }
            }
        }

        private void OpenChest()
        {
            isOpen = true;
            EmitSignal(SignalName.ChestOpened, this);

            // Animate chest opening
            var tween = CreateTween();
            tween.TweenProperty(meshInstance, "rotation_degrees:x", -45, 0.3f);
        }

        private void CloseChest()
        {
            isOpen = false;
            EmitSignal(SignalName.ChestClosed);

            // Animate chest closing
            var tween = CreateTween();
            tween.TweenProperty(meshInstance, "rotation_degrees:x", 0, 0.3f);
        }

        public bool AddItem(ItemType type, int count = 1)
        {
            // Similar logic to inventory
            for (int i = 0; i < ChestSize; i++)
            {
                if (slots[i] != null && slots[i].Type == type && slots[i].Count < slots[i].MaxStack)
                {
                    count = slots[i].AddItems(count);
                    if (count == 0)
                        return true;
                }
            }

            // Find empty slots
            while (count > 0)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot == -1)
                    return false;

                ItemStack newStack = new ItemStack(type, Math.Min(count, 64));
                slots[emptySlot] = newStack;
                count -= newStack.Count;
            }

            return true;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < ChestSize; i++)
            {
                if (slots[i] == null)
                    return i;
            }
            return -1;
        }

        public ItemStack GetSlot(int index)
        {
            if (index < 0 || index >= ChestSize)
                return null;
            return slots[index];
        }

        public void SetSlot(int index, ItemStack stack)
        {
            if (index < 0 || index >= ChestSize)
                return;
            slots[index] = stack;
        }

        public ItemStack[] GetAllSlots()
        {
            return slots;
        }

        public bool TransferToInventory(int slotIndex, Inventory playerInventory)
        {
            if (slots[slotIndex] == null)
                return false;

            ItemStack item = slots[slotIndex];
            if (playerInventory.AddItem(item.Type, item.Count))
            {
                slots[slotIndex] = null;
                return true;
            }

            return false;
        }

        public bool TransferFromInventory(ItemStack item, int targetSlot)
        {
            if (targetSlot < 0 || targetSlot >= ChestSize)
                return false;

            if (slots[targetSlot] == null)
            {
                slots[targetSlot] = item;
                return true;
            }
            else if (slots[targetSlot].CanStackWith(item))
            {
                int leftover = slots[targetSlot].AddItems(item.Count);
                item.Count = leftover;
                return leftover == 0;
            }

            return false;
        }
    }
}