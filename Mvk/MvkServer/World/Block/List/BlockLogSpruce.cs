using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна ель
    /// </summary>
    public class BlockLogSpruce : BlockAbWood
    {
        /// <summary>
        /// Блок бревна ель
        /// </summary>
        public BlockLogSpruce() : base(143, 142, new vec3(.58f, .42f, .22f), new vec3(.35f, .25f, .12f))
        {
            Hardness = 20;
        }
    }
}
