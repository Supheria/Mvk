using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна плодовое
    /// </summary>
    public class BlockLogFruit : BlockAbWood
    {
        /// <summary>
        /// Блок бревна плодовое
        /// </summary>
        public BlockLogFruit() : base(150, 149, new vec3(.84f, .72f, .3f), new vec3(.62f, .55f, .25f))
        {
            Hardness = 20;
        }
    }
}
