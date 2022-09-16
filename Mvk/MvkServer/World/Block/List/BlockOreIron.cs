using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Железная руда
    /// </summary>
    public class BlockOreIron : BlockAbOre
    {
        /// <summary>
        /// Блок Железная руда
        /// </summary>
        public BlockOreIron() : base(257, new vec3(.7f)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 35;
    }
}
