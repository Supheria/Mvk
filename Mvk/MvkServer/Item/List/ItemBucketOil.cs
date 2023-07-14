using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет ведро с нефтью
    /// </summary>
    public class ItemBucketOil : ItemAbBucketLiquid
    {
        public ItemBucketOil() : base(EnumItem.BucketOil, 99)
        {
            BlockLiquid = Blocks.GetBlockCache(EnumBlock.Oil);
        }
    }
}
