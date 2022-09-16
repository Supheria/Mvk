using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Угольная руда
    /// </summary>
    public class BlockOreCoal : BlockAbOre
    {
        /// <summary>
        /// Блок Угольная руда
        /// </summary>
        public BlockOreCoal() : base(256, new vec3(.7f)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 35;
    }
}
