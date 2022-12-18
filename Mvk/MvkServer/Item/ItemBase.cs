using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.Item
{
    /// <summary>
    ///  Базовый объект предметов
    /// </summary>
    public abstract class ItemBase
    {
        /// <summary>
        /// Максимальное количество однотипный вещей в одной ячейке
        /// </summary>
        public int MaxStackSize { get; protected set; } = 64;
        /// <summary>
        /// Максимальное количество урона, при 0, нет учёта урона
        /// </summary>
        public int MaxDamage { get; protected set; } = 0;
        /// <summary>
        /// id предмета
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Тип
        /// </summary>
        public EnumItem EItem { get; protected set; }
        /// <summary>
        /// Номер текстуры
        /// </summary>
        public int NumberTexture { get; protected set; } = 0;

        /// <summary>
        /// Обновить id
        /// </summary>
        protected void UpId() => Id = GetIdFromItem(this);

        public virtual void Update()
        {
            //ItemStack stack, World worldIn, Entity entityIn, int itemSlot, boolean isSelected
        }

        public virtual string GetName() => "item." + EItem;

        /// <summary>
        /// Копия предмет
        /// </summary>
        public ItemBase Copy() => GetItemById(Id);

        /// <summary>
        /// По id сгенерировать объект предмета
        /// </summary>
        public static ItemBase GetItemById(int id) => Items.GetItemCache(id);

        /// <summary>
        /// По предмету сгенерировать id
        /// </summary>
        public static int GetIdFromItem(ItemBase itemIn)
        {
            if (itemIn == null) return 0;

            if (itemIn.EItem == EnumItem.Block && itemIn is ItemBlock itemBlock)
            {
                return (int)itemBlock.Block.EBlock;
            }
            // Остальное предметы
            return (int)itemIn.EItem + 4096;
        }

        /// <summary>
        /// Вызывается, когда блок щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="pos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing"></param>
        public virtual bool OnItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing) => false;

        /// <summary>
        /// Вызывается всякий раз, когда этот предмет экипирован и нажата правая кнопка мыши.
        /// </summary>
        public virtual ItemStack OnItemRightClick(ItemStack itemStackIn, WorldBase worldIn, EntityPlayer playerIn)
            => itemStackIn;

        /// <summary>
        /// Вызывается, когда игрок отпускает кнопку мыши использования предмета.
        /// </summary>
        /// <param name="timeLeft">Количество тактов, оставшихся до завершения использования</param>
        public virtual void OnPlayerStoppedUsing(ItemStack stack, WorldBase worldIn, EntityPlayer playerIn, int timeLeft) { }

        /// <summary>
        /// Вызывается, когда игрок заканчивает использовать этот предмет (например, заканчивает есть). 
        /// Не вызывается, когда игрок перестает использовать предмет до завершения действия.
        /// </summary>
        public virtual ItemStack OnItemUseFinish(ItemStack stack, WorldBase worldIn, EntityPlayer playerIn) => stack;

        /// <summary>
        /// Проверка, может ли блок устанавливаться в этом месте
        /// </summary>
        /// <param name="blockPos">Координата блока, по которому щелкают правой кнопкой мыши</param>
        /// <param name="block">Объект блока который хотим установить</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public bool CanPlaceBlockOnSide(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, BlockBase block, Pole side, vec3 facing)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase blockOld = blockState.GetBlock();
            // Проверка ставить блоки, на те которые можно, к примеру на траву
            if (!blockOld.IsReplaceable) return false;
            // Если устанавливаемый блок такой же как стоит
            if (blockOld == block) return false;
            // Если стак пуст
            if (stack == null || stack.Amount == 0) return false;

            bool isCheckCollision = !block.IsCollidable;
            if (!isCheckCollision)
            {
                AxisAlignedBB axisBlock = block.GetCollision(blockPos, blockState.met);
                // Проверка коллизии игрока и блока
                isCheckCollision = axisBlock != null && !playerIn.BoundingBox.IntersectsWith(axisBlock)
                    && worldIn.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axisBlock, playerIn.Id).Count == 0;
            }

            //if (isCheckCollision)
            //{
            //    return Block.CanBlockStay(worldIn, blockPos);
            //}

            return isCheckCollision;
        }

        /// <summary>
        /// Можно ли уменьшать количество урона предмета в слоте
        /// </summary>
        public bool IsDecreaseDamage() => MaxDamage > 0;

        /// <summary>
        /// Сколько времени требуется, чтобы использовать или потреблять предмет
        /// </summary>
        public virtual int GetMaxItemUseDuration(ItemStack stack) => 0;

        /// <summary>
        /// Сила удара при метании предмета, влияет на усточивость блоков (Resistance)
        /// </summary>
        public virtual float GetImpactStrength() => 0;

        /// <summary>
        /// Вернуть тип действия предмета
        /// </summary>
        public virtual EnumItemAction GetItemUseAction(ItemStack stack) => EnumItemAction.None;

        public override string ToString() => Id.ToString();

    }
}
