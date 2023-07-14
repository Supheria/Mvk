using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет ведро с водой
    /// </summary>
    public class ItemBucketWater : ItemAbBucketLiquid
    {
        public ItemBucketWater() : base(EnumItem.BucketWater, 97)
        {
            BlockLiquid = Blocks.GetBlockCache(EnumBlock.Water);
        }
    }
}
