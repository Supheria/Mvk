using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Стеклянная панель белая
    /// </summary>
    public class BlockGlassPaneWhite : BlockAbGlassPane
    {
        /// <summary>
        /// Стеклянная панель белая
        /// </summary>
        public BlockGlassPaneWhite() : base(322, 323, new vec3(1f))
        {
            LightOpacity = 0;
        }
    }
}
