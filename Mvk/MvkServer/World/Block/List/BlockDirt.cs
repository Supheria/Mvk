using MvkServer.Glm;

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
    }
}
