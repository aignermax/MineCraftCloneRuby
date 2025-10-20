using Godot;
using System;
using System.Collections.Generic;

namespace MinecraftClone
{
    public enum ItemType
    {
        // Blocks
        Grass,
        Wood,
        Stone,
        Iron,

        // Tools
        WoodenPickaxe,
        StonePickaxe,
        IronPickaxe,
        WoodenSword,
        StoneSword,
        IronSword,
        Bow,
        Arrow,

        // Resources
        Coal,
        IronOre,
        Diamond,

        // Food
        Apple,
        Bread,
        Meat,

        // Armor
        LeatherHelmet,
        LeatherChestplate,
        LeatherLeggings,
        LeatherBoots,
        IronHelmet,
        IronChestplate,
        IronLeggings,
        IronBoots,

        // Misc
        Stick,
        String,
        Gunpowder
    }

    public class ItemStack
    {
        public ItemType Type { get; set; }
        public int Count { get; set; }
        public int MaxStack { get; set; } = 64;

        public ItemStack(ItemType type, int count = 1)
        {
            Type = type;
            Count = count;

            // Some items have different stack sizes
            switch (type)
            {
                case ItemType.WoodenSword:
                case ItemType.StoneSword:
                case ItemType.IronSword:
                case ItemType.WoodenPickaxe:
                case ItemType.StonePickaxe:
                case ItemType.IronPickaxe:
                case ItemType.Bow:
                case ItemType.LeatherHelmet:
                case ItemType.LeatherChestplate:
                case ItemType.LeatherLeggings:
                case ItemType.LeatherBoots:
                case ItemType.IronHelmet:
                case ItemType.IronChestplate:
                case ItemType.IronLeggings:
                case ItemType.IronBoots:
                    MaxStack = 1;
                    break;
                case ItemType.Arrow:
                    MaxStack = 64;
                    break;
            }
        }

        public bool CanStackWith(ItemStack other)
        {
            return other != null && Type == other.Type && Count < MaxStack;
        }

        public int AddItems(int amount)
        {
            int canAdd = Math.Min(amount, MaxStack - Count);
            Count += canAdd;
            return amount - canAdd; // Return leftover
        }
    }

    public partial class Inventory : Node
    {
        public const int InventorySize = 36; // 9x4 slots
        public const int HotbarSize = 9;

        private ItemStack[] slots;
        private int selectedHotbarSlot = 0;

        // Armor slots
        private ItemStack helmetSlot;
        private ItemStack chestplateSlot;
        private ItemStack leggingsSlot;
        private ItemStack bootsSlot;

        [Signal]
        public delegate void InventoryChangedEventHandler();

        [Signal]
        public delegate void HotbarSelectionChangedEventHandler(int slot);

        public override void _Ready()
        {
            slots = new ItemStack[InventorySize];
        }

        public bool AddItem(ItemType type, int count = 1)
        {
            // First, try to stack with existing items
            for (int i = 0; i < InventorySize; i++)
            {
                if (slots[i] != null && slots[i].Type == type && slots[i].Count < slots[i].MaxStack)
                {
                    count = slots[i].AddItems(count);
                    if (count == 0)
                    {
                        EmitSignal(SignalName.InventoryChanged);
                        return true;
                    }
                }
            }

            // Then, find empty slots
            while (count > 0)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot == -1)
                    return false; // Inventory full

                ItemStack newStack = new ItemStack(type, Math.Min(count, 64));
                slots[emptySlot] = newStack;
                count -= newStack.Count;
            }

            EmitSignal(SignalName.InventoryChanged);
            return true;
        }

        public bool RemoveItem(ItemType type, int count = 1)
        {
            int totalCount = GetItemCount(type);
            if (totalCount < count)
                return false;

            for (int i = InventorySize - 1; i >= 0 && count > 0; i--)
            {
                if (slots[i] != null && slots[i].Type == type)
                {
                    int toRemove = Math.Min(count, slots[i].Count);
                    slots[i].Count -= toRemove;
                    count -= toRemove;

                    if (slots[i].Count == 0)
                        slots[i] = null;
                }
            }

            EmitSignal(SignalName.InventoryChanged);
            return true;
        }

        public int GetItemCount(ItemType type)
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot != null && slot.Type == type)
                    count += slot.Count;
            }
            return count;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < InventorySize; i++)
            {
                if (slots[i] == null)
                    return i;
            }
            return -1;
        }

        public ItemStack GetSlot(int index)
        {
            if (index < 0 || index >= InventorySize)
                return null;
            return slots[index];
        }

        public void SetSlot(int index, ItemStack stack)
        {
            if (index < 0 || index >= InventorySize)
                return;
            slots[index] = stack;
            EmitSignal(SignalName.InventoryChanged);
        }

        public ItemStack GetHotbarSlot(int index)
        {
            if (index < 0 || index >= HotbarSize)
                return null;
            return slots[index];
        }

        public void SelectHotbarSlot(int slot)
        {
            selectedHotbarSlot = Mathf.Clamp(slot, 0, HotbarSize - 1);
            EmitSignal(SignalName.HotbarSelectionChanged, selectedHotbarSlot);
        }

        public ItemStack GetSelectedItem()
        {
            return GetHotbarSlot(selectedHotbarSlot);
        }

        public bool EquipArmor(ItemStack armorItem)
        {
            if (armorItem == null)
                return false;

            switch (armorItem.Type)
            {
                case ItemType.LeatherHelmet:
                case ItemType.IronHelmet:
                    if (helmetSlot != null)
                        AddItem(helmetSlot.Type, 1);
                    helmetSlot = armorItem;
                    return true;

                case ItemType.LeatherChestplate:
                case ItemType.IronChestplate:
                    if (chestplateSlot != null)
                        AddItem(chestplateSlot.Type, 1);
                    chestplateSlot = armorItem;
                    return true;

                case ItemType.LeatherLeggings:
                case ItemType.IronLeggings:
                    if (leggingsSlot != null)
                        AddItem(leggingsSlot.Type, 1);
                    leggingsSlot = armorItem;
                    return true;

                case ItemType.LeatherBoots:
                case ItemType.IronBoots:
                    if (bootsSlot != null)
                        AddItem(bootsSlot.Type, 1);
                    bootsSlot = armorItem;
                    return true;
            }

            return false;
        }

        public float GetArmorProtection()
        {
            float protection = 0;

            if (helmetSlot != null)
            {
                protection += helmetSlot.Type == ItemType.IronHelmet ? 2 : 1;
            }
            if (chestplateSlot != null)
            {
                protection += chestplateSlot.Type == ItemType.IronChestplate ? 6 : 3;
            }
            if (leggingsSlot != null)
            {
                protection += leggingsSlot.Type == ItemType.IronLeggings ? 5 : 2;
            }
            if (bootsSlot != null)
            {
                protection += bootsSlot.Type == ItemType.IronBoots ? 2 : 1;
            }

            return protection;
        }

        public override void _Input(InputEvent @event)
        {
            // Hotbar selection with number keys
            for (int i = 0; i < HotbarSize; i++)
            {
                if (@event.IsActionPressed($"hotbar_{i + 1}"))
                {
                    SelectHotbarSlot(i);
                    break;
                }
            }

            // Scroll wheel hotbar selection
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                {
                    SelectHotbarSlot((selectedHotbarSlot - 1 + HotbarSize) % HotbarSize);
                }
                else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                {
                    SelectHotbarSlot((selectedHotbarSlot + 1) % HotbarSize);
                }
            }
        }
    }
}