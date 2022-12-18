using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет ведро с лавой
    /// </summary>
    public class ItemBucketLava : ItemAbBucketLiquid
    {
        public ItemBucketLava() : base()
        {
            BlockLiquid = Blocks.GetBlockCache(EnumBlock.Lava);
            EItem = EnumItem.BucketLava;
            NumberTexture = 98;
            UpId();
        }
    }
}
