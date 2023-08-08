using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using MvkServer.World.Item;

namespace MvkServer.Item
{
    /// <summary>
    ///  Базовый объект предметов
    /// </summary>
    public class ItemBase
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
        /// Предмет имеет рендер через QuadSide
        /// </summary>
        public bool IsRenderQuadSide { get; protected set; } = false;
        /// <summary>
        /// Квадраты для объёмного рендера
        /// </summary>
        public ItemQuadSide[] Quads { get; protected set; }
        /// <summary>
        /// Вернуть тип действия предмета
        /// </summary>
        public EnumItemAction ItemUseAction { get; protected set; } = EnumItemAction.None;
        
        /// <summary>
        /// Объект крафта
        /// </summary>
        private CraftItem craft = new CraftItem();

        public ItemBase(EnumItem enumItem, int numberTexture, int maxStackSize)
        {
            EItem = enumItem;
            NumberTexture = numberTexture;
            if (maxStackSize != -1) MaxStackSize = maxStackSize;
            UpId();
        }

        public ItemBase(int numberTexture, int maxStackSize)
        {
            NumberTexture = numberTexture;
            if (maxStackSize != -1) MaxStackSize = maxStackSize;
        }

        /// <summary>
        /// Задать тип предмета
        /// </summary>
        protected void SetEnumItem(EnumItem enumItem)
        {
            EItem = enumItem;
            UpId();
        }

        /// <summary>
        /// Обновить id
        /// </summary>
        private void UpId() => Id = GetIdFromItem(this);

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
            return (int)itemIn.EItem + 5000;
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
        /// Вызывается, когда игрок заканчивает использовать этот предмет (например, заканчивает есть)
        /// Не вызывается, когда игрок перестает использовать предмет до завершения действия
        /// </summary>
        public virtual ItemStack OnItemUseFinish(ItemStack stack, WorldBase worldIn, EntityPlayer playerIn) => stack;

        /// <summary>
        /// Вызывается, когда блок уничтожается с помощью этого предмета
        /// Верните true, чтобы активировать статистику «Использовать предмет»
        /// </summary>
        public virtual bool OnBlockDestroyed(ItemStack stack, WorldBase worldIn, BlockBase blockIn, BlockPos pos, EntityPlayer playerIn) => false;

        /// <summary>
        /// Вызывается, когда происходит удар по блоку с помощью этого предмета, вернёт true если сломался инструмент
        /// </summary>
        /// <param name="damage">сила урона на инструмент</param>
        public virtual bool OnHitOnBlock(ItemStack stack, WorldBase worldIn, BlockBase blockIn, BlockPos pos, EntityPlayer playerIn, int damage) => false;

        /// <summary>
        /// Получить силу против блока
        /// </summary>
        public virtual float GetStrVsBlock(BlockBase block) => 1f;

        /// <summary>
        /// Получить урон для атаки предметом который в руке
        /// </summary>
        public virtual float GetDamageToAttack() => 1f;

        /// <summary>
        /// Время паузы между разрушениями блоков в тактах
        /// </summary>
        public virtual int PauseTimeBetweenBlockDestruction() => 6;

        /// <summary>
        /// Получить объект инструмента, если не инструмент тогда null
        /// </summary>
        public ItemAbTool GetTool() => this is ItemAbTool ? (ItemAbTool)this : null;

        /// <summary>
        /// Является ли предмет инструментом
        /// </summary>
        public bool IsTool() => this is ItemAbTool;

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
        /// Может ли блок быть разрушен тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public virtual bool CanDestroyedBlock(BlockBase block) => false;

        /// <summary>
        /// Может ли выпасть предмет после разрушения блока тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public virtual bool CanHarvestBlock(BlockBase block) => false;

        /// <summary>
        /// Объект крафта
        /// </summary>
        public virtual CraftItem GetCraft() => craft;

        #region Craft

        /// <summary>
        /// Задать рецепт
        /// </summary>
        public ItemBase SetCraftRecipe(int time, params Element[] recipe)
        {
            craft.SetRecipe(time, recipe);
            return this;
        }
        /// <summary>
        /// Задать рецепт
        /// </summary>
        public ItemBase SetCraftRecipe(int amount, int time, params Element[] recipe)
        {
            craft.SetRecipe(amount, time, recipe);
            return this;
        }

        /// <summary>
        /// Задать любой из требуемых инструментов для крафта
        /// </summary>
        public ItemBase SetCraftTools(params EnumItem[] tools)
        {
            craft.SetTools(tools);
            return this;
        }

        #endregion

        public override string ToString() => Id.ToString();

    }
}
