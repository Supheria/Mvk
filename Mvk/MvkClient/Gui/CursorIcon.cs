using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item.List;
using SharpGL;

namespace MvkClient.Gui
{
    /// <summary>
    /// Иконка возле курсора мыши
    /// </summary>
    public class CursorIcon : Control
    {
        /// <summary>
        /// Слот
        /// </summary>
        private Slot slot;

        /// <summary>
        /// Графический лист
        /// </summary>
        private uint dList;

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public override void MouseMove(int x, int y) => Position = new vec2i(x, y);

        public void SetSlot(Slot slot)
        {
            this.slot = slot;
            Render();
        }

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw() => GLRender.ListCall(dList);

        private void Render()
        {
            if (slot == null) return;

            uint list = GLRender.ListBegin();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = Enabled ? enter ? .390625f : .1953125f : 0;
            GLRender.Rectangle(0, 0, Width, Height, v1, .8046875f, v1 + .1953125f, 1);

            if (!slot.Empty() && slot.Item is ItemBlock itemBlock)
            {
                screen.ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(0, 0, 26);
            }
            if (slot.Amount > 1)
            {
                GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
                Text = slot.Amount.ToString();
                int x = GetXAlight(Text, 12) + 6;
                if (Enabled) FontRenderer.RenderString(x + 2, 10, new vec4(.1f, .1f, .1f, 1f), Text, size);
                vec4 color = Enabled ? enter ? new vec4(1f, 1f, .5f, 1f) : new vec4(1f) : new vec4(.5f, .5f, .5f, 1f);
                FontRenderer.RenderString(x, 9, color, Text, size);
            }
            GLRender.ListEnd();
            GLRender.ListDelete(dList);
            dList = list;
        }
    }
}
