using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет ведро с водой
    /// </summary>
    public class ItemBucketWater : ItemAbBucketLiquid
    {
        public ItemBucketWater() : base()
        {
            BlockLiquid = Blocks.GetBlockCache(EnumBlock.Water);
            EItem = EnumItem.BucketWater;
            NumberTexture = 97;
            UpId();
        }
    }
}
