using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Item.List;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    /// <summary>
    /// Объект кнопки слота для предметов 50*50
    /// </summary>
    public class ButtonSlot : Control
    {
        /// <summary>
        /// Слот
        /// </summary>
        private Slot slot;

        /// <summary>
        /// Ключи для нажатий кнопки и понимания их действий
        /// </summary>
        public EnumScreenKey ScreenKey { get; protected set; } = EnumScreenKey.None;

        public ButtonSlot(Slot slot) : base()
        {
            this.slot = slot;
            Height = 50;
            Width = 50;
            Alight = EnumAlight.Right;
        }

        //public ButtonSlot(ItemBase item = null, int amount = 1) : base()
        //{
        //    slot = new Slot(item, amount);
        //    Height = 50;
        //    Width = 50;
        //    Alight = EnumAlight.Right;
        //}

        public void SetSlot(Slot slot)
        {
            this.slot = slot;
            IsRender = true;
        }

        public Slot GetSlot() => slot;

        //public void SetItem(ItemBase item = null, int amount = 1)
        //{
        //    slot.Set(item, amount);
        //    IsRender = true;
        //}

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = Enabled ? enter ? .390625f : .1953125f : 0;
            GLRender.Rectangle(0, 0, Width, Height, v1, .8046875f, v1 + .1953125f, 1);

            if (!slot.Empty() && slot.Item is ItemBlock itemBlock)
            {
                screen.ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(25, 25, 26);
            }

            if (slot.Amount > 1)
            {
                GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
                Text = slot.Amount.ToString();
                int x = GetXAlight(Text, 12) + 6;
                if (Enabled) FontRenderer.RenderString(x + 2, 35, new vec4(.1f, .1f, .1f, 1f), Text, size);
                vec4 color = Enabled ? enter ? new vec4(1f, 1f, .5f, 1f) : new vec4(1f) : new vec4(.5f, .5f, .5f, 1f);
                FontRenderer.RenderString(x, 34, color, Text, size);
            }
        }

        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                MouseMove(x, y);
                if (enter) OnClick();
            }
            else if (button == MouseButton.Right)
            {
                MouseMove(x, y);
                if (enter) OnClickRight();
            }
        }

        /// <summary>
        /// Событие клика правой клавишей мыши по кнопке
        /// </summary>
        public event EventHandler ClickRight;
        protected virtual void OnClickRight() => ClickRight?.Invoke(this, new EventArgs());

        public override string ToString()
        {
            if (slot == null) return "null";
            return string.Format("id:{0} a:{1} i:{2}", slot.Index, slot.Amount, slot.Item == null ? "null" : slot.Item.Id.ToString());

        }

    }
}
