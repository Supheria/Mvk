using MvkServer.Entity.List;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.NBT;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
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
        /// Слот который используется в перемещении из слотов, образно он в указателе мыши
        /// </summary>
        public ItemStack StackAir { get; private set; }

        /// <summary>
        /// Инвентарь, пока 8 ячеек
        /// </summary>
        private readonly ItemStack[] mainInventory = new ItemStack[COUNT];
        /// <summary>
        /// Ячейки брони
        /// </summary>
        private readonly ItemStack[] armorInventory = new ItemStack[COUNT_ARMOR];

        #region Craft

        /// <summary>
        /// Статус идёт ли крафт
        /// </summary>
        private bool isCraft = false;
        /// <summary>
        /// Время крафта
        /// </summary>
        private int timeCraft = 0;
        /// <summary>
        /// Какой предмет крафтим
        /// </summary>
        private ItemBase itemCraft;
        /// <summary>
        /// Количество предметов в крафте
        /// </summary>
        private int amountCraft = 1;

        #endregion


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
        /// Если номер слота 255 то это стек который в воздухе, что возле курсора
        /// </summary>
        public void SetInventorySlotContents(int slotIn, ItemStack stack)
        {
            if (slotIn == 255)
            {
                StackAir = stack;
                OnChanged(255);
            }
            else if (slotIn >= COUNT)
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
        /// Отправить изменение размера воздушного стака игроку
        /// </summary>
        private void SendSetAirPlayer()
        {
            if (Player is EntityPlayerServer entityPlayerServer)
            {
                entityPlayerServer.SendPacket(new PacketS2FSetSlot(255, StackAir));
                OnChanged(255);
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
            int count = mainInventory.Length;
            ItemStack stack;
            for (int i = 0; i < count; i++)
            {
                stack = mainInventory[i];
                if (stack != null && stack.Item.Id == itemStack.Item.Id 
                    && stack.IsStackable() && stack.Amount < stack.GetMaxStackSize() 
                    && stack.Amount < INVENTORY_STACK_LIMIT
                    && stack.ItemDamage == itemStack.ItemDamage 
                    && ItemStack.AreItemsEqual(stack, itemStack))
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

        /// <summary>
        /// Может ли блок быть разрушен тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public bool CanDestroyedBlock(BlockBase block)
        {
            if (Player.IsCreativeMode || block.Material.RequiresNoTool) return true;
            ItemStack itemStack = GetStackInSlot(CurrentItem);
            return itemStack != null ? itemStack.Item.CanDestroyedBlock(block) : false;
        }

        /// <summary>
        /// Может ли выпасть предмет после разрушения блока тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public bool CanHarvestBlock(BlockBase block)
        {
            if (block.Material.RequiresNoTool) return true;
            ItemStack itemStack = GetStackInSlot(CurrentItem);
            return itemStack != null ? itemStack.Item.CanHarvestBlock(block) : false;
        }

        /// <summary>
        /// Вызывается, когда блок уничтожается с помощью этого предмета блок
        /// </summary>
        public void OnBlockDestroyed(WorldBase worldIn, BlockBase blockIn, BlockPos blockPos)
        {
            ItemStack stack = GetStackInSlot(CurrentItem);
            if (stack != null)
            {
                if (stack.Item.OnBlockDestroyed(stack, worldIn, blockIn, blockPos, Player))
                {
                    //if (!world.IsRemote)
                    //{
                    // Статистика    
                    //playerIn.triggerAchievement(StatList.objectUseStats[Item.getIdFromItem(this.item)]);
                    //}
                }
            }
        }

        /// <summary>
        /// Вызывается, когда происходит удар по блоку с помощью этого предмета, вернёт true если сломался инструмент
        /// </summary>
        /// <param name="damage">сила урона на инструмент</param>
        public bool OnHitOnBlock(WorldBase worldIn, BlockBase blockIn, BlockPos blockPos, int damage)
        {
            ItemStack stack = GetStackInSlot(CurrentItem);
            return stack != null ? stack.Item.OnHitOnBlock(stack, worldIn, blockIn, blockPos, Player, damage) : false;
        }

        /// <summary>
        /// Получить силу против блока со всем нюансами игрока
        /// </summary>
        public float GetStrVsBlock(BlockBase block)
        {
            ItemStack stack = GetStackInSlot(CurrentItem);
            float force = stack != null ? stack.Item.GetStrVsBlock(block) : 1f;
            if (Player.IsInWater()) force *= .2f;
            if (!Player.OnGround) force *= .2f;
            return force;
        }

        /// <summary>
        /// Получить урон для атаки предметом который в руке со всеми нюансами игрока
        /// </summary>
        public float GetDamageToAttack(WorldBase worldIn)
        {
            ItemStack stack = GetStackInSlot(CurrentItem);
            float damage = stack != null ? stack.Item.GetDamageToAttack() : 1f;
            if (Player.IsInWater()) damage *= .75f;
            if (!Player.OnGround) damage *= 1.1f;
            return damage;
        }

        /// <summary>
        /// Время паузы между разрушениями блоков в тактах
        /// </summary>
        public int PauseTimeBetweenBlockDestruction()
        {
            ItemStack stack = GetStackInSlot(CurrentItem);
            return stack != null ? stack.Item.PauseTimeBetweenBlockDestruction() : 6;
        }

        /// <summary>
        /// Получить предмет инструмента
        /// </summary>
        public ItemAbTool GetItemTool() => GetStackInSlot(CurrentItem)?.Item.GetTool();

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

        /// <summary>
        /// Выбросить предмет который в руке, только для сервера
        /// </summary>
        public void ThrowOutCurrentItem()
        {
            ItemStack stack = GetCurrentItem();
            if (stack != null)
            {
                Player.DropItem(stack, true);
                SetCurrentItem(null);
                SendSetSlotPlayer();
            }
        }

        #region ClickSlot

        /// <summary>
        /// Выкинуть предмет из рук который в воздухе
        /// </summary>
        public void ThrowOutAir()
        {
            if (StackAir != null)
            {
                Player.DropItem(StackAir, true);
                SetSendAirContents();
            }
        }

        /// <summary>
        /// Клик по указанному слоту в инвентаре, isRight указывает флаг правый ли клик мыши
        /// </summary>
        public void ClickInventorySlotContents(int slotIn, bool isRight)
        {
            ItemStack stackAir = StackAir?.Copy();
            ItemStack stackSlot = GetStackInSlot(slotIn)?.Copy();
            if (stackSlot == null)
            {
                if (stackAir != null)
                {
                    // Если в воздухе есть так будем укладывать в ячейку
                    if (isRight && stackAir.Amount > 1)
                    {
                        // Если был клик правой клавишей, то укладываем только одну единицу
                        SetSendSlotContents(slotIn, stackAir.Copy().SetAmount(1));
                        SetSendAirContents(stackAir.ReduceAmount(1));
                    }
                    else
                    {
                        // Перекладываем всё из воздуха в ячейку
                        SetSendAirContents();
                        SetSendSlotContents(slotIn, stackAir);
                    }
                }
            }
            else
            {
                // Имеется что-то в ячейке
                if (stackAir == null)
                {
                    // В воздухе нет ничего
                    if (isRight && stackSlot.Amount > 1)
                    {
                        // Берём половину в воздух
                        int amount = stackSlot.Amount / 2;
                        SetSendAirContents(stackSlot.Copy().SetAmount(amount));
                        SetSendSlotContents(slotIn, stackSlot.ReduceAmount(amount));
                    }
                    else
                    {
                        // Перекладываем всё из ячейку в воздух
                        SetSendAirContents(stackSlot);
                        SetSendSlotContents(slotIn);
                    }
                }
                else
                {
                    // В воздухе имеется и в ячейке имеется
                    if (stackSlot.Item.Id == stackAir.Item.Id)
                    {
                        // Если предметы одинаковые
                        if (isRight && stackAir.Amount > 1)
                        {
                            if (stackSlot.Item.MaxStackSize > stackSlot.Amount)
                            {
                                // Если был клик правой клавишей, то добавляем только одну единицу
                                SetSendSlotContents(slotIn, stackSlot.AddAmount(1));
                                SetSendAirContents(stackAir.ReduceAmount(1));
                            }
                        }
                        else
                        {
                            int aw = stackSlot.Amount + stackAir.Amount;
                            if (aw > stackSlot.Item.MaxStackSize)
                            {
                                // сумма больше слота значит не весь перекладываем в руке останется
                                SetSendSlotContents(slotIn, stackSlot.SetAmount(stackSlot.Item.MaxStackSize));
                                SetSendAirContents(stackAir.SetAmount(aw - stackSlot.Item.MaxStackSize));
                            }
                            else
                            {
                                // весь можно переложить
                                SetSendSlotContents(slotIn, stackSlot.SetAmount(aw));
                                SetSendAirContents();
                            }
                        }
                    }
                    else
                    {
                        // Если разные, меняем местами
                        SetSendAirContents(stackSlot);
                        SetSendSlotContents(slotIn, stackAir);
                    }
                }
            }
        }

        /// <summary>
        /// Задать потом отправить игроку изменения слота со стакам.
        /// </summary>
        private void SetSendSlotContents(int slotIn, ItemStack stack = null)
        {
            SetInventorySlotContents(slotIn, stack);
            SendSetSlotPlayer(slotIn);
        }

        /// <summary>
        /// Задать потом отправить игроку изменения воздушного стака.
        /// </summary>
        private void SetSendAirContents(ItemStack stack = null)
        {
            SetInventorySlotContents(255, stack);
            SendSetAirPlayer();
        }

        #endregion

        #region Craft

        /// <summary>
        /// Обновления в инвентаре, используется для крафта, только на сервере
        /// </summary>
        public void UpdateServer()
        {
            if (isCraft)
            {
                if (--timeCraft <= 0)
                {
                    isCraft = false;
                    CraftEnd();
                }
            }
        }

        /// <summary>
        /// Получить количество тиков времени на крафт этого предмета
        /// </summary>
        public int GetCountTimeCraft(ItemBase item, bool isMax, bool amountFix = false)
        {
            int time;
            int amountOne = item.GetCraft().Amount;
            int amount = isMax ? item.MaxStackSize : amountOne;
            int amountComp = amount / amountOne;
            // Знаем количество комплектов
            if (amountComp > 1)
            {
                // определяем сколько максимум можем
                int amountCache = MaxAmountItemCraft(item);
                if (amountCache < amountComp) amountComp = amountCache;
            }
            amount = amountOne * amountComp;
            if (amount > amountOne)
            {
                // если крафтим много, увеличиваем время
                time = item.GetCraft().CraftTime * amountComp;
                if (time == 0)
                {
                    // если время ноль, всё равно указываем время для количества
                    time = amount / (4 * amountOne);
                }
            }
            else
            {
                time = item.GetCraft().CraftTime;
            }
            if (amountFix) amountCraft = amount;
            return time;
        }

        /// <summary>
        /// Остановить крафт
        /// </summary>
        public void CraftStop()
        {
            isCraft = false;
        }

        /// <summary>
        /// Начинаем делать крафт на сервере
        /// </summary>
        public void CraftBeginServer(int itemId, bool isMax)
        {
            itemCraft = Items.GetItemCache(itemId);
            timeCraft = GetCountTimeCraft(itemCraft, isMax, true);
            
            if (timeCraft == 0)
            {
                CraftEnd();
            }
            else
            {
                isCraft = true;
            }
        }

        /// <summary>
        /// Сделан крафт
        /// </summary>
        private void CraftEnd()
        {
            if (itemCraft != null)
            {
                // Тратим предметы из инвентаря на крафт
                if (SpendItemsOnCrafting(itemCraft, amountCraft / itemCraft.GetCraft().Amount))
                {

                    // Берём предмет в руку
                    if (StackAir != null)
                    {
                        if (StackAir.Item.Id == itemCraft.Id && StackAir.Amount + amountCraft <= itemCraft.MaxStackSize)
                        {
                            // Один и тот же предмет что в руке, добавляем количество
                            SetSendAirContents(StackAir.AddAmount(amountCraft));
                        }
                        else
                        {
                            // Дропаем что в руке
                            ThrowOutAir();
                            // Укладываем то что сделали в руки
                            SetSendAirContents(new ItemStack(itemCraft, amountCraft));
                        }
                    }
                    else
                    {
                        // Укладываем то что сделали в руки
                        SetSendAirContents(new ItemStack(itemCraft, amountCraft));
                    }
                }
            }
            // Закончить крафт, чтоб включить кнопку
            if (Player is EntityPlayerServer entityPlayerServer)
            {
                entityPlayerServer.SendPacket(new PacketS31WindowProperty(PacketS31WindowProperty.EnumAction.CraftStop));
            }
        }

        /// <summary>
        /// Крафт закончен, на стороне клиента запрос
        /// </summary>
        public void CraftedClient() => OnCrafted();

        /// <summary>
        /// Проверка можем ли мы сделать этот предмет
        /// </summary>
        public bool CheckItemCraft(ItemBase itemCheck)
        {
            Element[] elements = itemCheck.GetCraft().CraftRecipe;
            int count = elements.Length;
            if (count > 0)
            {
                Element element;
                for (int i = 0; i < count; i++)
                {
                    element = elements[i];
                    if (GetInventoryItemCount(element.GetItem()) < element.GetAmount())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Максимальное количество комплектов можем сделать
        /// </summary>
        private int MaxAmountItemCraft(ItemBase itemCheck)
        {
            Element[] elements = itemCheck.GetCraft().CraftRecipe;
            int amount = itemCheck.MaxStackSize / itemCheck.GetCraft().Amount;
            int count = elements.Length;
            if (count > 0)
            {
                int amountCache;
                Element element;
                for (int i = 0; i < count; i++)
                {
                    element = elements[i];
                    amountCache = GetInventoryItemCount(element.GetItem()) / element.GetAmount();
                    if (amountCache < amount)
                    {
                        amount = amountCache;
                    }
                }
            }
            return amount;
        }

        /// <summary>
        /// Вернуть количество такого предмета
        /// </summary>
        private int GetInventoryItemCount(ItemBase item)
        {
            int count = mainInventory.Length;
            int amount = 0;
            ItemStack stack;
            for (int i = 0; i < count; i++)
            {
                stack = mainInventory[i];
                if (stack != null && stack.Item.Id == item.Id)
                {
                    amount += stack.Amount;
                }
            }
            return amount;
        }

        /// <summary>
        /// Тратим предметы из инвентаря на крафт, возвращает false, если это невозможно.
        /// </summary>
        private bool SpendItemsOnCrafting(ItemBase item, int amount)
        {
            Element[] elements = item.GetCraft().CraftRecipe;
            int count = elements.Length;
            if (count > 0)
            {
                Element element;
                for (int i = 0; i < count; i++)
                {
                    element = elements[i];
                    if (!SpendItemToInventory(element.GetItem(), element.GetAmount() * amount))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Тратим предметы в инвентаре, возвращает false, если это невозможно.
        /// </summary>
        private bool SpendItemToInventory(ItemBase item, int amount)
        {
            int count = mainInventory.Length;
            ItemStack stack;
            for (int i = 0; i < count; i++)
            {
                stack = mainInventory[i];
                if (stack != null && stack.Item.Id == item.Id)
                {
                    if (stack.Amount <= amount)
                    {
                        // удаляем этот стак
                        amount -= stack.Amount;
                        mainInventory[i] = null;
                        SetSendSlotContents(i);
                    }
                    else
                    {
                        stack.ReduceAmount(amount);
                        amount = 0;
                        SetSendSlotContents(i, stack);
                    }
                }
                if (amount == 0) return true;
            }
            return amount == 0;
        }


        #endregion

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

        /// <summary>
        /// Событие крафт сделан
        /// </summary>
        public event EventHandler Crafted;
        private void OnCrafted() => Crafted?.Invoke(this, new EventArgs());

        #endregion
    }
}
