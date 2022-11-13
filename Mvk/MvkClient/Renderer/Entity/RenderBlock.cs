using MvkAssets;
using MvkClient.Renderer.Block;
using MvkServer.World.Block;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер блока
    /// </summary>
    public class RenderBlock : RenderDL
    {
        /// <summary>
        /// Тип блока
        /// </summary>
        private readonly EnumBlock enumBlock;

        public RenderBlock(EnumBlock enumBlock) => this.enumBlock = enumBlock;

        protected override void DoRender()
        {
            BlockGuiRender render = new BlockGuiRender(Blocks.GetBlockCache(enumBlock));
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.AtlasBlocks);
            GLWindow.Texture.BindTexture(ts.GetKey());
            render.RenderVBOtoDL();
        }
    }
}
