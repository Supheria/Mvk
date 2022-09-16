using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна дуба
    /// </summary>
    public class BlockLogOak : BlockAbLog
    {
        /// <summary>
        /// Блок бревна дуба
        /// </summary>
        public BlockLogOak() : base(129, 128, new vec3(.79f, .64f, .43f), new vec3(.62f, .44f, .37f), EnumBlock.LeavesOak)
        {
            crownWidth = 5;
            heightTree = 16;
        }
    }
}
