using MvkServer.Entity.List;
using MvkServer.Sound;
using MvkServer.World;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет взрывчатки
    /// </summary>
    public class ItemUniExplosives : ItemUniPiece
    {
        /// <summary>
        /// Сила взрыва
        /// </summary>
        private readonly float strength;
        /// <summary>
        /// Дистанция взрыва
        /// </summary>
        private readonly float distance;

        public ItemUniExplosives(EnumItem enumItem, int numberTexture, float strength, float distance) : base(enumItem, numberTexture, 0)
        {
            MaxStackSize = 16;
            this.strength = strength;
            this.distance = distance;
           // UpId();
        }

        public float GetStrength() => strength;
        public float GetDistance() => distance;

        /// <summary>
        /// Вернуть тип действия предмета
        /// </summary>
     //   public override EnumItemAction GetItemUseAction(ItemStack stack) => EnumItemAction.Throw;

        /// <summary>
        /// Вызывается всякий раз, когда этот предмет экипирован и нажата правая кнопка мыши.
        /// </summary>
        //public override ItemStack OnItemRightClick(ItemStack itemStackIn, WorldBase worldIn, EntityPlayer playerIn)
        //{
        //    EnumItem enumItem = itemStackIn.Item.EItem;
        //    if (!playerIn.IsCreativeMode)
        //    {
        //        playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
        //    }
        //    playerIn.PlaySound(AssetsSample.Bow, .5f, .4f / (worldIn.Rnd.NextFloat() * .4f + .8f));

        //    if (!worldIn.IsRemote)
        //    {
        //        OnAction(worldIn, playerIn, enumItem);
        //    }
        //    return itemStackIn;
        //}

        /// <summary>
        /// Действие при броске
        /// </summary>
        protected override void OnAction(WorldBase worldIn, EntityPlayer playerIn, EnumItem enumItem)
        {
            EntityExplosives entity = new EntityExplosives(worldIn, playerIn);
            entity.SetItem(enumItem);
            worldIn.SpawnEntityInWorld(entity);
        }
    }
}
