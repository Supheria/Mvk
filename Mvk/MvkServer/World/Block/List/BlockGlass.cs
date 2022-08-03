using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стекла
    /// </summary>
    public class BlockGlass : BlockAbGlass
    {
        /// <summary>
        /// Блок стекла
        /// </summary>
        public BlockGlass() : base(320, new vec3(1f))
        {
            LightOpacity = 0;
            Translucent = false;
        }
    }
}
