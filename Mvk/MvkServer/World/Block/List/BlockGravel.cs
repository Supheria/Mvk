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
        public BlockGravel() : base(69, new vec3(1))
        {
            Hardness = 5;
        }
    }
}
