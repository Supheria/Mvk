using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет ведро с нефтью
    /// </summary>
    public class ItemBucketOil : ItemAbBucketLiquid
    {
        public ItemBucketOil() : base()
        {
            BlockLiquid = Blocks.GetBlockCache(EnumBlock.Oil);
            EItem = EnumItem.BucketOil;
            NumberTexture = 99;
            UpId();
        }
    }
}
