using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Известняк
    /// </summary>
    public class BlockLimestone : BlockAbSolid
    {
        /// <summary>
        /// Блок Известняк
        /// </summary>
        public BlockLimestone() : base(4, new vec3(.76f, .77f, .67f)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 20;
    }
}
