using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Network;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item
{
    /// <summary>
    /// Объект одной ячейки предметов
    /// </summary>
    public class ItemStack
    {
        /// <summary>
        /// Объект предмета
        /// </summary>
        public ItemBase Item { get; private set; }
        /// <summary>
        /// Урон
        /// </summary>
        public int ItemDamage { get; private set; }
        /// <summary>
        /// Количество вещей в стаке
        /// </summary>
        public int Amount { get; private set; }

        public ItemStack(BlockBase block) : this(block, 1, 0) { }
        public ItemStack(BlockBase block, int amount) : this(block, amount, 0) { }
        public ItemStack(BlockBase block, int amount, int itemDamage) 
            : this(Items.GetItemCache((int)block.EBlock), amount, itemDamage) { }

        public ItemStack(ItemBase item) : this(item, 1, 0) { }
        public ItemStack(ItemBase item, int amount) : this(item, amount, 0) { }
        public ItemStack(ItemBase item, int amount, int itemDamage)
        {
            Item = item;
            Amount = amount;
            ItemDamage = itemDamage;
            if (ItemDamage < 0) ItemDamage = 0;
        }

        public ItemStack(int id, int amount, int itemDamage)
        {
            Item = Items.GetItemCache(id);
            Amount = amount;
            ItemDamage = itemDamage;
            if (ItemDamage < 0) ItemDamage = 0;
        }
        /// <summary>
        /// Разделяет стек на заданное количество этого стека и уменьшает этот стек на указанную сумму
        /// </summary>
        public ItemStack SplitStack(int amount)
        {
            ItemStack itemStack = new ItemStack(Item, amount, ItemDamage);
            Amount -= amount;
            return itemStack;
        }

        /// <summary>
        /// Количество сделать нулю
        /// </summary>
        public void Zero() => Amount = 0;
        /// <summary>
        /// Добавить к количеству
        /// </summary>
        public void AddAmount(int amount) => Amount += amount;
        /// <summary>
        /// Уменьшить количество на amount
        /// </summary>
        public void ReduceAmount(int amount) => Amount -= amount;
        /// <summary>
        /// Задать новое количество
        /// </summary>
        public void SetAmount(int amount) => Amount = amount;

        /// <summary>
        /// Копия стака
        /// </summary>
        public ItemStack Copy() => new ItemStack(Item?.Copy(), Amount, ItemDamage);

        /// <summary>
        /// Сравнить два стака предметов
        /// </summary>
        public static bool AreItemStacksEqual(ItemStack stackA, ItemStack stackB)
            => stackA == null && stackB == null
            ? true : (stackA != null && stackB != null ? stackA.IsItemStackEqual(stackB) : false);

        /// <summary>
        /// Сравнить стак
        /// </summary>
        private bool IsItemStackEqual(ItemStack other) 
            => other != null && Item.Id == other.Item.Id && Amount == other.Amount && ItemDamage == other.ItemDamage;

        /// <summary>
        /// Сравнить два предмета (стак без учёта количества)
        /// </summary>
        public static bool AreItemsEqual(ItemStack stackA, ItemStack stackB) 
            => stackA == null && stackB == null 
                ? true : (stackA != null && stackB != null ? stackA.IsItemEqual(stackB) : false);

        /// <summary>
        /// Сравнить предмет (стак без учёта количества)
        /// </summary>
        public bool IsItemEqual(ItemStack other) 
            => other != null && Item.Id == other.Item.Id && ItemDamage == other.ItemDamage;

        /// <summary>
        /// возвращает true, когда повреждаемый элемент поврежден
        /// </summary>
        public bool IsItemDamaged() => IsItemStackDamageable() && ItemDamage > 0;

        /// <summary>
        /// true, если этот стек предметов можно повредить
        /// </summary>
        public bool IsItemStackDamageable()
        {
            return Item == null ? false : Item.MaxDamage > 0;// ? false : !this.hasTagCompound() || !this.getTagCompound().getBoolean("Unbreakable"));
        }

        /// <summary>
        /// Попытки повредить ItemStack с количеством повреждений amount. 
        /// Если у ItemStack есть зачарование Unbreaking, есть шанс, 
        /// что каждое очко урона будет нейтрализовано. Возвращает true, 
        /// если получает больше урона, чем getMaxDamage(). Возвращает false 
        /// в противном случае, или если ItemStack не может быть поврежден, 
        /// или если все точки повреждения сведены на нет.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public bool AttemptDamageItem(int amount)//, Rand rand)
        {
            if (!IsItemStackDamageable())
            {
                return false;
            }
            else
            {
                ItemDamage += amount;
                return ItemDamage >= Item.MaxDamage;
            }
        }

        public void DamageItem(int amount, EntityLiving entity, BlockPos blockPos)
        {
            if (entity is EntityPlayer entityPlayer && !entityPlayer.IsCreativeMode)
            {
                if (AttemptDamageItem(amount))
                {
                    ItemDamage = 0;
                    Zero();
                    // Сломался предмет
                    entity.World.PlaySound(entity, AssetsSample.Break, blockPos.ToVec3() + .5f, 1f, entity.World.Rnd.NextFloat() * .4f + .8f);
                }
            }
        }

        /// <summary>
        /// Действие игрока на этот стак, нажимая правую клавишу мыши
        /// </summary>
        public bool ItemUse(EntityPlayer playerIn, WorldBase worldIn, BlockPos pos, Pole side, vec3 facing)
        {
            bool b = Item.OnItemUse(this, playerIn, worldIn, pos, side, facing);
            if (b)
            {
                // статистика
                //playerIn.triggerAchievement(StatList.objectUseStats[Item.getIdFromItem(this.item)]);
            }

            return b;

        }

        /// <summary>
        /// Использовать элемент правой кнопкой мыши
        /// </summary>
        public ItemStack UseItemRightClick(WorldBase worldIn, EntityPlayer playerIn) => Item.OnItemRightClick(this, worldIn, playerIn);

        /// <summary>
        /// Возвращает максимальный размер стека
        /// </summary>
        public int GetMaxStackSize() => Item.MaxStackSize;

        /// <summary>
        /// Возвращает true, если стак может содержать 2 или более единиц элемента.
        /// </summary>
        public bool IsStackable() => GetMaxStackSize() > 1 && (!IsItemStackDamageable() || !IsItemDamaged());

        /// <summary>
        /// Получить максимальную продолжительность использования предмета
        /// </summary>
        public int GetMaxItemUseDuration() => Item.GetMaxItemUseDuration(this);

        /// <summary>
        /// Вызывается, когда игрок отпускает кнопку мыши использования предмета.
        /// </summary>
        /// <param name="timeLeft">Количество тактов, оставшихся до завершения использования</param>
        public void OnPlayerStoppedUsing(WorldBase worldIn, EntityPlayer playerIn, int timeLeft)
            => Item.OnPlayerStoppedUsing(this, worldIn, playerIn, timeLeft);

        /// <summary>
        /// Вызывается, когда количество используемых элементов достигает 0, например. пункт еда съедена.
        /// Верните новый ItemStack. Аргументы: мир, сущность
        /// </summary>
        public ItemStack OnItemUseFinish(WorldBase worldIn, EntityPlayer playerIn)
            => Item.OnItemUseFinish(this, worldIn, playerIn);

        /// <summary>
        /// Вернуть тип действия предмета
        /// </summary>
        public EnumItemAction GetItemUseAction() => Item != null && Amount > 0 ? Item.GetItemUseAction(this) : EnumItemAction.None;

        /// <summary>
        /// Записать стак в буффер пакета
        /// </summary>
        public static void WriteStream(ItemStack itemStack, StreamBase stream)
        {
            if (itemStack == null)
            {
                stream.WriteShort(-1);
            }
            else
            {
                stream.WriteShort((short)ItemBase.GetIdFromItem(itemStack.Item));
                stream.WriteByte((byte)itemStack.Amount);
                stream.WriteShort((short)itemStack.ItemDamage);
            }
        }


        /// <summary>
        /// Прочесть стак с буфера пакета
        /// </summary>
        public static ItemStack ReadStream(StreamBase stream)
        {
            int id = stream.ReadShort();
            if (id >= 0)
            {
                int amount = stream.ReadByte();
                int itemDamage = stream.ReadShort();
                return new ItemStack(ItemBase.GetItemById(id), amount, itemDamage);
            }
            return null;
        }

        /// <summary>
        /// Записать стак в NBT
        /// </summary>
        public TagCompound WriteToNBT(TagCompound nbt)
        {
            nbt.SetShort("Id", (short)ItemBase.GetIdFromItem(Item));
            nbt.SetByte("Amount", (byte)Amount);
            nbt.SetShort("Damage", (short)ItemDamage);
            return nbt;
        }


        /// <summary>
        /// Прочесть стак с NBT
        /// </summary>
        public static ItemStack ReadFromNBT(TagCompound nbt)
        {
            int id = nbt.GetShort("Id");
            if (id >= 0)
            {
                return new ItemStack(ItemBase.GetItemById(id), nbt.GetByte("Amount"), nbt.GetShort("Damage"));
            }
            return null;
        }

        public override string ToString() => Item.GetName() + " " + Amount;

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(ItemStack))
            {
                var vec = (ItemStack)obj;
                if (Amount == vec.Amount && ItemDamage == vec.ItemDamage && Item.Id == vec.Item.Id) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => Item.Id.GetHashCode() ^ Amount.GetHashCode() ^ ItemDamage.GetHashCode();
    }
}
