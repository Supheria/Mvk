using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Объект блока гравия
    /// </summary>
    public class BlockGravel : BlockUniLoose
    {
        public BlockGravel() : base(69, EnumItem.PieceGravel, 5, .5f) { }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
            => Items.GetItemCache(rand.Next(5) == 0 ? EnumItem.Flint : EnumItem.PieceGravel);
    }
}
