using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок земли
    /// </summary>
    public class BlockDirt : BlockAbLoose
    {
        /// <summary>
        /// Блок земли
        /// </summary>
        public BlockDirt() : base(64, new vec3(.62f, .44f, .37f)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 5;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        public override ItemBase GetItemDropped(BlockState state, Rand rand, int fortune)
            => Items.GetItemCache(EnumItem.PieceDirt);
    }
}
