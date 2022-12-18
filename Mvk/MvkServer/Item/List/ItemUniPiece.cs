using MvkServer.Entity.List;
using MvkServer.Sound;
using MvkServer.World;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет кусочек
    /// </summary>
    public class ItemUniPiece : ItemBase
    {
        private readonly float power;

        public ItemUniPiece(EnumItem enumItem, int numberTexture, float power = 0)
        {
            EItem = enumItem;
            NumberTexture = numberTexture;
            MaxStackSize = 128;
            this.power = power;
            UpId();
        }

        /// <summary>
        /// Вернуть тип действия предмета
        /// </summary>
        public override EnumItemAction GetItemUseAction(ItemStack stack) => EnumItemAction.Throw;

        /// <summary>
        /// Вызывается всякий раз, когда этот предмет экипирован и нажата правая кнопка мыши.
        /// </summary>
        public override ItemStack OnItemRightClick(ItemStack itemStackIn, WorldBase worldIn, EntityPlayer playerIn)
        {
            EnumItem enumItem = itemStackIn.Item.EItem;
            if (!playerIn.IsCreativeMode)
            {
                playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
            }
            playerIn.PlaySound(AssetsSample.Bow, .5f, .4f / (worldIn.Rnd.NextFloat() * .4f + .8f));

            if (!worldIn.IsRemote)
            {
                OnAction(worldIn, playerIn, enumItem);
            }
            return itemStackIn;
        }

        /// <summary>
        /// Сила удара при метании предмета, влияет на усточивость блоков (Resistance)
        /// </summary>
        public override float GetImpactStrength() => power;

        /// <summary>
        /// Действие при броске
        /// </summary>
        protected virtual void OnAction(WorldBase worldIn, EntityPlayer playerIn, EnumItem enumItem)
        {
            EntityPiece entity = new EntityPiece(worldIn, playerIn);
            entity.SetItem(enumItem);
            worldIn.SpawnEntityInWorld(entity);
        }
    }
}
