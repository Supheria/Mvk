using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Гравий
    /// </summary>
    public class BlockGravel : BlockAbLoose
    {
        /// <summary>
        /// Блок Гравий
        /// </summary>
        public BlockGravel() : base(69, new vec3(1)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 5;
    }
}
