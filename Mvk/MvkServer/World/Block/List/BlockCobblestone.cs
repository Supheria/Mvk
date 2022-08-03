using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок булыжника
    /// </summary>
    public class BlockCobblestone : BlockAbSolid
    {
        /// <summary>
        /// Блок булыжника
        /// </summary>
        public BlockCobblestone() : base(3, new vec3(.7f))
        {
            Hardness = 25;
        }
    }
}
