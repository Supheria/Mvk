using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна берёза
    /// </summary>
    public class BlockLogBirch : BlockAbWood
    {
        /// <summary>
        /// Блок бревна берёза
        /// </summary>
        public BlockLogBirch() : base(136, 135, new vec3(.81f, .74f, .5f), new vec3(1f))
        {
            Hardness = 20;
        }
    }
}
