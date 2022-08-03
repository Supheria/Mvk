using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Стеклянная панель прозрачная
    /// </summary>
    public class BlockGlassPane : BlockAbGlassPane
    {
        /// <summary>
        /// Стеклянная панель прозрачная
        /// </summary>
        public BlockGlassPane() : base(320, 321, new vec3(1f))
        {
            LightOpacity = 0;
            Translucent = false;
        }
    }
}
