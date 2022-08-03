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
        public BlockClay() : base(70, new vec3(1))
        {
            Hardness = 5;
        }
    }
}
