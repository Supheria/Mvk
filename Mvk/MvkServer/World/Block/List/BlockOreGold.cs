using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Золотая руда
    /// </summary>
    public class BlockOreGold : BlockAbOre
    {
        /// <summary>
        /// Блок Золотая руда
        /// </summary>
        public BlockOreGold() : base(258, new vec3(.7f)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 35;

    }
}
