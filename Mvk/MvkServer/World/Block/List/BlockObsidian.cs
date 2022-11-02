using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Обсидиан 
    /// </summary>
    public class BlockObsidian : BlockAbSolid
    {
        /// <summary>
        /// Блок Обсидиан 
        /// </summary>
        public BlockObsidian() : base(12, new vec3(.6f))
        {
            Resistance = 20; // 2000 minecraft
        }
    }
}
