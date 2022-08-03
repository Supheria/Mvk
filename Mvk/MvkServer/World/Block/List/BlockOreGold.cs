using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Золотая руда
    /// </summary>
    public class BlockOreGold : BlockAbOre
    {
        /// <summary>
        /// Блок Золотая руда
        /// </summary>
        public BlockOreGold() : base(258, new vec3(.7f))
        {
            Hardness = 35;
        }
    }
}
