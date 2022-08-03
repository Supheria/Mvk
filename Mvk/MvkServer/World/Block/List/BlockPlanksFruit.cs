using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок досок плодовое
    /// </summary>
    public class BlockPlanksFruit : BlockAbWood
    {
        /// <summary>
        /// Блок досок плодовое
        /// </summary>
        public BlockPlanksFruit() : base(152, 151, new vec3(.84f, .72f, .3f), new vec3(.84f, .72f, .3f))
        {
            Hardness = 20;
        }
    }
}
