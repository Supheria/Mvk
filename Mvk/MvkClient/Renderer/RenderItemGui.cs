using MvkAssets;
using MvkServer.Item;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Рендер предмета для GUI
    /// </summary>
    public class RenderItemGui : RenderDL
    {
        /// <summary>
        /// Тип предмета
        /// </summary>
        private readonly EnumItem enumItem;

        public RenderItemGui(EnumItem enumItem) => this.enumItem = enumItem;

        public void Render(int x, int y)
        {
            GLRender.PushMatrix();
            GLRender.Translate(x, y, 0);
            Render();
            GLRender.PopMatrix();
        }

        protected override void DoRender()
        {
            if (enumItem == EnumItem.Block)
            {
                IsHidden = true;
                return;
            }
            ItemBase item = Items.GetItemCache(enumItem);
            if (item == null) return;

            float u = (item.NumberTexture % 32) * .03125f;
            float v = item.NumberTexture / 32 * .03125f;

            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.AtlasItems);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.PushMatrix();
            {
                GLRender.DepthDisable();
                GLRender.Rectangle(-16, -16, 16, 16, u, v, u + .03125f, v + .03125f);
                GLRender.DepthEnable();
            }
            GLRender.PopMatrix();
        }
    }
}
