using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Угольная руда
    /// </summary>
    public class BlockOreCoal : BlockAbOre
    {
        /// <summary>
        /// Блок Угольная руда
        /// </summary>
        public BlockOreCoal() : base(256, new vec3(.7f))
        {
            Hardness = 35;
        }
    }
}
