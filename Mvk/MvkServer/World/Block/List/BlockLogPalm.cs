using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна пальмы
    /// </summary>
    public class BlockLogPalm : BlockAbWood
    {
        /// <summary>
        /// Блок бревна пальмы
        /// </summary>
        public BlockLogPalm() : base(158, 157, new vec3(.8f, .62f, .5f), new vec3(.5f, .35f, .2f))
        {
            Hardness = 20;
        }
    }
}
