using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    public class TextBox : Control
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public override int Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                UpTextDraw();
            }
        }
        /// <summary>
        /// Счётчик для анимации
        /// </summary>
        private int cursorCounter;
        /// <summary>
        /// Максимальная длинна 
        /// </summary>
        private readonly int limit = 24;
        /// <summary>
        /// Видимость курсора
        /// </summary>
        private bool isVisibleCursor;
        /// <summary>
        /// Ограничения набор символов 
        /// </summary>
        private readonly EnumRestrictions restrictions;
        /// <summary>
        /// Фиксированный фокус, к примеру для чата
        /// </summary>
        private bool fixFocus = false;
        /// <summary>
        /// Текст для экрана
        /// </summary>
        private string textDraw;

        public TextBox(string text, EnumRestrictions restrictions, int limit = 24) : base(text)
        {
            UpTextDraw();
            this.restrictions = restrictions;
            this.limit = limit;
        }
        /// <summary>
        /// Сделать фиксированный вокус, чтоб не терять курсор
        /// </summary>
        public void FixFocus()
        {
            Focus = true;
            fixFocus = true;
        }
        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Render()
        {
            base.Render();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = 0f;
            float v2 = 0.15625f;
            int wh = Width / 2;
            float wh2 = Width / 256f;
            GLRender.Rectangle(0, 0, wh, Height, 0, v1, 0.5f * wh2, v2);
            GLRender.Rectangle(wh, 0, Width, Height, 1f - 0.5f * wh2, v1, 1f, v2);

            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));

            vec3 color = Enabled ? enter ? new vec3(1, 1, .5f) : new vec3(1) : new vec3(.5f);
            FontRenderer.RenderString(12, 14, textDraw, size, color, Alpha, Enabled, .1f);

            if (isVisibleCursor)
            {
                int ws = FontRenderer.WidthString(textDraw, size);
                FontRenderer.RenderString(ws + 12, 14, "_", size, new vec3(1), Alpha, true, .1f);
            }
        }

        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                MouseMove(x, y);
                if (enter && !Focus)
                {
                    // Задать фокус
                    Focus = true;
                }
                else if(!enter && Focus && !fixFocus) 
                {
                    // Потерять фокус
                    isVisibleCursor = false;
                    Focus = false;
                    IsRender = true;
                }
            }
        }

        public override void KeyPress(char key)
        {
            int id = Convert.ToInt32(key);
            if (id == 8)
            {
                // back
                if (Text.Length > 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                    UpTextDraw();
                    IsRender = true;
                }
            }
            else if (Text.Length < limit && Check(id))
            {
                Text += key;
                UpTextDraw();
                IsRender = true;
            }
        }

        /// <summary>
        /// Задать текст
        /// </summary>
        public override void SetText(string text)
        {
            Text = text;
            UpTextDraw();
            IsRender = true;
        }

        /// <summary>
        /// Обновить текст для дисплея
        /// </summary>
        private void UpTextDraw() => textDraw = FontRenderer.GetStringEndToWidth(Text, Width - 24, size);

        private bool Check(int id)
        {
            switch (restrictions)
            {
                case EnumRestrictions.IpPort:
                    return (id >= 48 && id <= 57) // цифры
                    || id == 46 || id == 58; // точка и двое точие
                case EnumRestrictions.Number:
                    return id >= 48 && id <= 57; // цифры
                case EnumRestrictions.Name:
                    return (id >= 48 && id <= 57) // цифры
                    || (id >= 65 && id <= 90) // Большие англ
                    || (id >= 97 && id <= 122); // Маленькие англ
                case EnumRestrictions.All:
                    return Symbol.IsPresent(id);
            }
            return false;
        }

        /// <summary>
        /// Увеличивает счетчик курсора 
        /// </summary>
        public void UpdateCursorCounterTick() => cursorCounter++;
        /// <summary>
        /// Обработка курсора в draw
        /// </summary>
        public void CursorCounterDraw()
        {
            if (Focus && cursorCounter / 6 % 2 == 0)
            {
                if (!isVisibleCursor)
                {
                    isVisibleCursor = true;
                    IsRender = true;
                }
            }
            else
            {
                if (isVisibleCursor)
                {
                    isVisibleCursor = false;
                    IsRender = true;
                }
            }
        }

        /// <summary>
        /// Ограничения набор символов 
        /// </summary>
        public enum EnumRestrictions
        {
            /// <summary>
            /// Для цифры, точка и двоеточие
            /// </summary>
            IpPort,
            /// <summary>
            /// Только цифры
            /// </summary>
            Number,
            /// <summary>
            /// Цифры и английские буквы
            /// </summary>
            Name,
            /// <summary>
            /// Все доступные по графике
            /// </summary>
            All
        }
    }
}
