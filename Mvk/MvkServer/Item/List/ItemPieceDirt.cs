using MvkServer.Entity.List;
using MvkServer.Sound;
using MvkServer.World;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет Земляной кусочек
    /// </summary>
    //public class ItemPieceDirt : ItemBase
    //{
    //    public ItemPieceDirt()
    //    {
    //        EItem = EnumItem.PieceDirt;
    //        NumberTexture = 192;
    //        MaxStackSize = 128;
    //        UpId();
    //    }

    //    /// <summary>
    //    /// Вернуть тип действия предмета
    //    /// </summary>
    //    public override EnumItemAction GetItemUseAction(ItemStack stack) => EnumItemAction.Throw;

    //    /// <summary>
    //    /// Вызывается всякий раз, когда этот предмет экипирован и нажата правая кнопка мыши.
    //    /// </summary>
    //    public override ItemStack OnItemRightClick(ItemStack itemStackIn, WorldBase worldIn, EntityPlayer playerIn)
    //    {
    //        if (!playerIn.IsCreativeMode)
    //        {
    //            playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
    //        }
    //        playerIn.PlaySound(AssetsSample.Bow, .5f, .4f / (worldIn.Rnd.NextFloat() * .4f + .8f));
            
    //        if (!worldIn.IsRemote)
    //        {
    //            playerIn.DropItem(new ItemStack(itemStackIn.Item, 1), true, true);
    //        }
    //        return itemStackIn;
    //    }
    //}
}
