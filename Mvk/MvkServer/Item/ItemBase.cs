using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World;

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
        public virtual bool ItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing) => false;

        public override string ToString() => Id.ToString();
    }
}
