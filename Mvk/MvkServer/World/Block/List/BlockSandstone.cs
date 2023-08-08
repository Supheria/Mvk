using MvkServer.Item;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Песчаник
    /// </summary>
    public class BlockSandstone : BlockUniSolid
    {
        /// <summary>
        /// Блок Песчаник
        /// </summary>
        public BlockSandstone() : base(7, EnumItem.PieceSandstone, 15) { }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitQuads() => InitQuads(6, 8, 7, 7, 7, 7);
    }
}
