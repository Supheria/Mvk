using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.World.Block;
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
        public override void Render()
        {
            base.Render();
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = Enabled ? enter ? .390625f : .1953125f : 0;
            GLRender.Rectangle(0, 0, Width, Height, v1, .8046875f, v1 + .1953125f, 1);

            if (!slot.Empty())
            {
                if (slot.Stack.Item.EItem == EnumItem.Block && slot.Stack.Item is ItemBlock itemBlock)
                {
                    // Прорисовка блока
                    screen.ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(25, 25, 26);
                }
                else
                {
                    // Прорисовка предмета
                    screen.ClientMain.World.WorldRender.GetItemGui(slot.Stack.Item.EItem).Render(25, 25);
                }

                if (slot.Stack.Amount > 1)
                {
                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
                    Text = slot.Stack.Amount.ToString();
                    int x = GetXAlight(Text, 12) + 6;
                    vec3 color = Enabled ? enter ? new vec3(1, 1, .5f) : new vec3(1) : new vec3(.5f);
                    FontRenderer.RenderString(x, 34, Text, size, color, Alpha, Enabled, .1f);
                }
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
        /// Вернуть подсказку у контрола
        /// </summary>
        public override string GetToolTip()
        {
            if (enter && slot != null && !slot.Empty())
            {
                return Language.T(slot.Stack.GetItemName());
            }
            return "";
        }

        /// <summary>
        /// Событие клика правой клавишей мыши по кнопке
        /// </summary>
        public event EventHandler ClickRight;
        protected virtual void OnClickRight() => ClickRight?.Invoke(this, new EventArgs());

        public override string ToString()
        {
            if (slot == null) return "null";
            return string.Format("id:{0} a:{1} i:{2}", slot.Index, slot.Stack.Amount, slot.Empty() ? "null" : slot.Stack.Item.Id.ToString());
        }

    }
}
