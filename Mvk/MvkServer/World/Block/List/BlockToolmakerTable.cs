using MvkServer.Entity.List;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Часть блока cтола инструментальщика
    /// </summary>
    public class BlockToolmakerTable : BlockAbCraftingTable
    {
        public BlockToolmakerTable() : base()
        {
            window = EnumWindowType.Toolmaker;
            Particle = 520;
            InitQuads(520, 520, 519, 519);
        }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
        {
            int r = rand.Next(10);
            if (r < 4) return Items.GetItemCache(EnumBlock.LogOak);
            if (r < 6) return Items.GetItemCache(EnumItem.Stick);
            if (r < 8) return Items.GetItemCache(EnumItem.WoodChips);
            return null;
        }

        /// <summary>
        /// Получить список предметов для крафта, используется для верстаков
        /// </summary>
        public override int[] GetListItemsCraft() => Items.craftToolmaker;
    }
}
