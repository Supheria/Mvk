using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
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
            RenderSlot();
        }

        /// <summary>
        /// Перерендер
        /// </summary>
        public void PereRender() => RenderSlot();

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw(float timeIndex) => GLRender.ListCall(dList);

        private void RenderSlot()
        {
            if (slot == null) return;

            uint list = GLRender.ListBegin();

            GLRender.Texture2DEnable();
            GLRender.Color(1);
            if (!slot.Empty())
            {
                if (slot.Stack.Item.EItem == EnumItem.Block && slot.Stack.Item is ItemBlock itemBlock)
                {
                    // Прорисовка блока
                    screen.ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(0, 0, 26);
                }
                else
                {
                    // Прорисовка предмета
                    screen.ClientMain.World.WorldRender.GetItemGui(slot.Stack.Item.EItem).Render(0, 0);
                }
                if (slot.Stack.Amount > 1)
                {
                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
                    Text = slot.Stack.Amount.ToString();
                    int x = GetXAlight(Text, 12) + 6;
                    vec3 color = Enabled ? enter ? new vec3(1, 1, .5f) : new vec3(1) : new vec3(.5f);
                    FontRenderer.RenderString(x, 9, Text, size, color, Alpha, Enabled, .1f);
                }
            }
            GLRender.ListEnd();
            GLRender.ListDelete(dList);
            dList = list;
        }
    }
}
