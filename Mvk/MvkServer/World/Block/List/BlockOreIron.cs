using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Железная руда
    /// </summary>
    public class BlockOreIron : BlockAbOre
    {
        /// <summary>
        /// Блок Железная руда
        /// </summary>
        public BlockOreIron() : base(257, new vec3(.7f))
        {
            Hardness = 35;
        }
    }
}
