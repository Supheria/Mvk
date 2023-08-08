using MvkServer.Entity.List;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Часть блока столярного верстака
    /// </summary>
    public class BlockCraftingTableCarpentry : BlockAbCraftingTable
    {
        public BlockCraftingTableCarpentry() : base()
        {
            window = EnumWindowType.Carpentry;
            Particle = 518;
            InitQuads(518, 518, 517, 516);
        }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
        {
            int r = rand.Next(10);
            if (r < 4) return Items.GetItemCache(EnumBlock.LogBirch);
            if (r < 6) return Items.GetItemCache(EnumItem.Stick);
            if (r < 8) return Items.GetItemCache(EnumItem.WoodChips);
            return null;
        }

        /// <summary>
        /// Получить список предметов для крафта, используется для верстаков
        /// </summary>
        public override int[] GetListItemsCraft() => Items.craftCarpentry;
    }
}
