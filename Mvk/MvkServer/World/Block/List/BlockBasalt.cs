using MvkServer.Glm;
using MvkServer.Item;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Базальт
    /// </summary>
    public class BlockBasalt : BlockUniSolid
    {
        /// <summary>
        /// Блок Базальт
        /// </summary>
        public BlockBasalt() : base(10, new vec3(.7f), EnumItem.PieceBasalt, 15) { }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitQuads() => InitQuads(10, 10, 11, 11, 11, 11);
    }
}
