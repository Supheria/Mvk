using MvkServer.Entity.List;
using MvkServer.Item;
using MvkServer.NBT;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using System;

namespace MvkServer.Inventory
{
    /// <summary>
    /// Инвентарь игрока
    /// </summary>
    public class InventoryPlayer : IInventory
    {
        /// <summary>
        /// Количество ячеек для предметов
        /// </summary>
        public const int COUNT = 8;
        /// <summary>
        /// Количество ячеек для брони
        /// </summary>
        public const int COUNT_ARMOR = 4;
        /// <summary>
        /// Самый максимальный размер. Даже если значение ItemBase.MaxStackSize будет больше,
        /// обрежет по INVENTORY_STACK_LIMIT
        /// </summary>
        public const int INVENTORY_STACK_LIMIT = 255;
        /// <summary>
        /// Выбранный слот правой руки
        /// </summary>
        public int CurrentItem { get; private set; }
        /// <summary>
        /// Объект игрока
        /// </summary>
        public EntityPlayer Player { get; private set; }

        /// <summary>
        /// Инвентарь, пока 8 ячеек
        /// </summary>
        private readonly ItemStack[] mainInventory = new ItemStack[COUNT];
        /// <summary>
        /// Ячейки брони
        /// </summary>
        private readonly ItemStack[] armorInventory = new ItemStack[COUNT_ARMOR];

        public InventoryPlayer(EntityPlayer entityPlayer) => Player = entityPlayer;

        /// <summary>
        /// Задать слот
        /// </summary>
        public void SetCurrentItem(int slotIn)
        {
            if (slotIn < COUNT && slotIn >= 0 && slotIn != CurrentItem)
            {
                CurrentItem = slotIn;
            }
        }

        /// <summary>
        /// Сместить слот в большую сторону
        /// </summary>
        public void SlotMore()
        {
            if (CurrentItem < COUNT - 1) CurrentItem++;
            else CurrentItem = 0;
        }

        /// <summary>
        /// Сместить слот в меньшую сторону
        /// </summary>
        public void SlotLess()
        {
            if (CurrentItem > 0) CurrentItem--;
            else CurrentItem = COUNT - 1;
        }

        /// <summary>
        /// Получить выбранный стак правой руки
        /// </summary>
        public ItemStack GetCurrentItem() 
            => CurrentItem < COUNT && CurrentItem >= 0 ? mainInventory[CurrentItem] : null;

        /// <summary>
        /// Задать в правую руку стак
        /// </summary>
        public void SetCurrentItem(ItemStack stack)
        {
            mainInventory[CurrentItem] = stack;
            OnChanged(CurrentItem);
        }

        /// <summary>
        /// Получить стак брони
        /// </summary>
        /// <param name="slot">0-3 InventoryPlayer.COUNT_ARMOR</param>
        public ItemStack GetArmorInventory(int slot) => armorInventory[slot];

        /// <summary>
        /// Задать стак брони
        /// </summary>
        /// <param name="slot">0-3 InventoryPlayer.COUNT_ARMOR</param>
        public void SetArmorInventory(int slot, ItemStack stack) => armorInventory[slot] = stack;

        /// <summary>
        /// Возвращает количество слотов в инвентаре
        /// </summary>
        public int GetSizeInventory() => COUNT;

        /// <summary>
        /// Возвращает стек в слоте slotIn
        /// </summary>
        public ItemStack GetStackInSlot(int slotIn)
        {
            if (slotIn >= COUNT)
            {
                slotIn -= COUNT;
                return armorInventory[slotIn];
            }
            return mainInventory[slotIn];
        }

        /// <summary>
        /// Возвращает первый пустой стек элементов
        /// -1 нет пустого
        /// </summary>
        public int GetFirstEmptyStack()
        {
            for (int i = 0; i < mainInventory.Length; i++)
            {
                if (mainInventory[i] == null) return i;
            }
            return -1;
        }

        /// <summary>
        /// Устанавливает данный стек предметов в указанный слот в инвентаре (может быть раздел крафта или брони).
        /// </summary>
        public void SetInventorySlotContents(int slotIn, ItemStack stack)
        {
            if (slotIn >= COUNT)
            {
                slotIn -= COUNT;
                armorInventory[slotIn] = stack;
            }
            else
            {
                mainInventory[slotIn] = stack;
                OnChanged(slotIn);
            }
        }

        /// <summary>
        /// Отправить пакет слота
        /// </summary>
        public void SendSlot(int slotIn) => SendSetSlotPlayer(slotIn);

        /// <summary>
        /// Добавляет стек предметов в инвентарь, если это невозможно заспавнит предмет рядом
        /// </summary>
        public void AddItemStackToInventory(WorldBase worldIn, EntityPlayer entityPlayer, ItemStack itemStack)
        {
            // Пробуем взять в инвентарь
            if (!AddItemStackToInventory(itemStack))
            {
                // Если не смогли взять, дропаем его
                if (!worldIn.IsRemote)
                {
                    // Дроп
                    entityPlayer.DropItem(itemStack, true);
                }
            }
        }

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        public bool AddItemStackToInventory(ItemStack stack)
        {
            if (stack != null && stack.Amount != 0 && stack.Item != null)
            {
                try
                {
                    //ItemStack itemStack = GetStackInSlot(CurrentItem);
                    //if (itemStack == null)
                    //{
                    //    SetInventorySlotContents(CurrentItem, stack.Copy());
                    //    stack.Zero();
                    //    return true;
                    //}


                    if (stack.IsItemDamaged())
                    {
                        int slot = GetFirstEmptyStack();

                        if (slot >= 0)
                        {
                            mainInventory[slot] = stack.Copy();
                            //mainInventory[slot].animationsToGo = 5;
                            stack.Zero();
                            SendSetSlotPlayer(slot);
                            return true;
                        }
                        else if (Player.IsCreativeMode)
                        {
                            stack.Zero();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        int amount;
                        do
                        {
                            amount = stack.Amount;
                            stack.SetAmount(StorePartialItemStack(stack));
                        }
                        while (stack.Amount > 0 && stack.Amount < amount);

                        if (stack.Amount == amount && Player.IsCreativeMode)
                        {
                            stack.Zero();
                            return true;
                        }
                        else
                        {
                            return stack.Amount < amount;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Crach(ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Отправить изменение размера выфбранного слота игроку
        /// </summary>
        public bool SendSetSlotPlayer()
        {
            bool b = false;
            if (mainInventory[CurrentItem] != null && mainInventory[CurrentItem].Amount == 0)
            {
                mainInventory[CurrentItem] = null;
                b = true;
            }
            SendSetSlotPlayer(CurrentItem);
            return b;
        }

        /// <summary>
        /// Отправить изменение размера слота игроку
        /// </summary>
        private void SendSetSlotPlayer(int slot)
        {
            if (Player is EntityPlayerServer entityPlayerServer)
            {
                entityPlayerServer.SendPacket(new PacketS2FSetSlot(slot, mainInventory[slot]));
                OnChanged(slot);
            }
        }

        /// <summary>
        /// Эта функция сохраняет как можно больше элементов ItemStack в соответствующем 
        /// слоте и возвращает количество оставшихся элементов.
        /// </summary>
        /// <param name="itemStack"></param>
        /// <returns></returns>
        private int StorePartialItemStack(ItemStack itemStack)
        {
            ItemBase item = itemStack.Item;
            int amount = itemStack.Amount;
            int slot = StoreItemStack(itemStack);

            if (slot < 0) slot = GetFirstEmptyStack();

            if (slot < 0)
            {
                return amount;
            }
            else
            {
                if (mainInventory[slot] == null)
                {
                    mainInventory[slot] = new ItemStack(item, 0, itemStack.ItemDamage);
                }

                int amount2 = amount;

                if (amount > mainInventory[slot].GetMaxStackSize() - mainInventory[slot].Amount)
                {
                    amount2 = mainInventory[slot].GetMaxStackSize() - mainInventory[slot].Amount;
                }

                if (amount2 > INVENTORY_STACK_LIMIT - mainInventory[slot].Amount)
                {
                    amount2 = INVENTORY_STACK_LIMIT - mainInventory[slot].Amount;
                }

                if (amount2 == 0)
                {
                    return amount;
                }
                else
                {
                    amount -= amount2;
                    mainInventory[slot].AddAmount(amount2);
                    SendSetSlotPlayer(slot);
                    //mainInventory[slot].animationsToGo = 5;
                    return amount;
                }
            }
        }

        /// <summary>
        /// Находим слот с таким же стакам, где можно ещё что-то засунуть. Типа не полный
        /// </summary>
        private int StoreItemStack(ItemStack itemStack)
        {
            for (int i = 0; i < mainInventory.Length; i++)
            {
                if (mainInventory[i] != null && mainInventory[i].Item.Id == itemStack.Item.Id 
                    && mainInventory[i].IsStackable() && mainInventory[i].Amount < mainInventory[i].GetMaxStackSize() 
                    && mainInventory[i].Amount < INVENTORY_STACK_LIMIT
                    && mainInventory[i].ItemDamage == itemStack.ItemDamage 
                    && ItemStack.AreItemsEqual(mainInventory[i], itemStack))
                {
                    return i;
                }
            }
            return -1;
        }

        

        /// <summary>
        /// Удаляет из слота инвентаря (первый аргумент) до указанного количества (второй аргумент) предметов и возвращает их в новый стек.
        /// </summary>
        public ItemStack DecrStackSize(int slot, int count)
        {
            if (mainInventory[slot] != null)
            {
                ItemStack itemStack;

                if (mainInventory[slot].Amount <= count)
                {
                    itemStack = mainInventory[slot];
                    mainInventory[slot] = null;
                }
                else
                {
                    itemStack = mainInventory[slot].SplitStack(count);
                    if (mainInventory[slot].Amount == 0)
                    {
                        mainInventory[slot] = null;
                    }
                }
                SendSetSlotPlayer(slot);
                return itemStack;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Получить список стака (броня и что в руке)
        /// </summary>
        public ItemStack[] GetCurrentItemAndArmor()
        {
            ItemStack[] stacks = new ItemStack[armorInventory.Length + 1];
            stacks[0] = GetCurrentItem();
            for (int i = 0; i < armorInventory.Length; i++)
            {
                stacks[i + 1] = armorInventory[i];
            }
            return stacks;
        }

        public void SetCurrentItemAndArmor(ItemStack[] stacks)
        {
            if (stacks.Length > 0)
            {
                SetCurrentItem(stacks[0]);
                int count = stacks.Length;
                if (count > COUNT_ARMOR + 1) count = COUNT_ARMOR + 1;
                for (int i = 1; i < count; i++)
                {
                    SetArmorInventory(i - 1, stacks[i]);
                }
            }
        }

        public ItemStack[] GetMainAndArmor()
        {
            ItemStack[] stacks = new ItemStack[mainInventory.Length + armorInventory.Length];
            for (int i = 0; i < mainInventory.Length; i++)
            {
                stacks[i] = mainInventory[i];
            }
            int j = mainInventory.Length;
            for (int i = 0; i < armorInventory.Length; i++)
            {
                stacks[i + j] = armorInventory[i];
            }
            return stacks;
        }

        public void SetMainAndArmor(ItemStack[] stacks)
        {
            if (mainInventory.Length + armorInventory.Length == stacks.Length)
            {
                for (int i = 0; i < mainInventory.Length; i++)
                {
                    mainInventory[i] = stacks[i];
                }
                int j = mainInventory.Length - 1;
                for (int i = 0; i < armorInventory.Length; i++)
                {
                    armorInventory[i] = stacks[i + j];
                }
            }
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < COUNT; i++)
            {
                if (CurrentItem == i) str += "*";
                str += i + 1;
                if (mainInventory[i] == null) str += " - ";
                else str += " " + mainInventory[i].Item.GetName() + " =" + mainInventory[i].Amount + " ";
            }
            return str;
        }

        public void Clear()
        {
            for (int i = 0; i < mainInventory.Length; i++)
            {
                mainInventory[i] = null;
            }
            for (int i = 0; i < armorInventory.Length; i++)
            {
                armorInventory[i] = null;
            }
            OnChanged(-1);
        }

        public void WriteToNBT(TagList nbt)
        {
            ItemStack[] items = GetMainAndArmor();

            TagCompound compound;
            ItemStack itemStack;
            for (int i = 0; i < items.Length; i++)
            {
                itemStack = items[i];
                if (itemStack != null && itemStack.Amount > 0)
                {
                    compound = new TagCompound();
                    compound.SetByte("Slot", (byte)i);
                    compound.SetByte("Amount", (byte)itemStack.Amount);
                    compound.SetShort("Damage", (short)itemStack.ItemDamage);
                    compound.SetShort("Id", (short)itemStack.Item.Id);
                    nbt.AppendTag(compound);
                }
            }
        }

        public void ReadFromNBT(TagList nbt)
        {
            Clear();
            int count = nbt.TagCount();
            ItemStack stak;
            NBTBase nbtBase;
            for (int i = 0; i < count; i++)
            {
                nbtBase = nbt.Get(i);
                if (nbtBase.GetId() == 10 && nbtBase is TagCompound compound)
                {
                    stak = new ItemStack(
                        compound.GetShort("Id"),
                        compound.GetByte("Amount"),
                        compound.GetShort("Damage")
                    );
                    SetInventorySlotContents(compound.GetByte("Slot"), stak);
                }
            }
        }

        #region Event

        /// <summary>
        /// Событие изменён рабочий инвентарь (8 предметов)
        /// </summary>
        public event SlotEventHandler Changed;
        private void OnChanged(int indexSlot) => Changed?.Invoke(this, new SlotEventArgs(indexSlot));

        #endregion
    }
}
