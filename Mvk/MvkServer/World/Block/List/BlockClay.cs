using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Глина
    /// </summary>
    public class BlockClay : BlockAbLoose
    {
        /// <summary>
        /// Блок Глина
        /// </summary>
        public BlockClay() : base(70, new vec3(1)) { }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 5;
    }
}
