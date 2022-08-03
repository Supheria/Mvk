using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок досок дуба
    /// </summary>
    public class BlockPlanksOak : BlockAbWood
    {
        /// <summary>
        /// Блок досок дуба
        /// </summary>
        public BlockPlanksOak() : base(131, 130, new vec3(.79f, .64f, .43f), new vec3(.79f, .64f, .43f))
        {
            Hardness = 20;
        }
    }
}
