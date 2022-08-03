using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок досок пальмы
    /// </summary>
    public class BlockPlanksPalm : BlockAbWood
    {
        /// <summary>
        /// Блок досок пальмы
        /// </summary>
        public BlockPlanksPalm() : base(160, 159, new vec3(.8f, .62f, .5f), new vec3(.8f, .62f, .5f))
        {
            Hardness = 20;
        }
    }
}
