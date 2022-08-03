using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Стеклянная панель красная
    /// </summary>
    public class BlockGlassPaneRed : BlockAbGlassPane
    {
        /// <summary>
        /// Стеклянная панель красная
        /// </summary>
        public BlockGlassPaneRed() : base(322, 323, new vec3(1f, 0, 0))
        {
            LightOpacity = 0;
        }
    }
}
