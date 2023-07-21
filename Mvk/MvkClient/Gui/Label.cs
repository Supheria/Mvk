using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkClient.Util;
using MvkServer.Glm;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    public class Label : Control
    {
        /// <summary>
        /// Масшатб
        /// </summary>
        public float Scale { get; set; } = 1f;

        public Label(string text, FontSize size) : base(text) => this.size = size;

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Render()
        {
            base.Render();
            if (Text == "") return;

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(1f, 1f, 1f, 1f);
            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            vec3 color = Enabled ? new vec3(1) : new vec3(.6f);

            GLRender.PushMatrix();
            {
                int w2 = Width / 2;
                int h2 = Height / 2;
                GLRender.Translate(w2, h2, 0);
                GLRender.Scale(Scale);
                GLRender.Translate(-w2, -h2, 0);

                string[] stringSeparators = new string[] { "\r\n" };
                string[] strs = Text.Split(stringSeparators, StringSplitOptions.None);
                int h = 0;
                foreach (string str in strs)
                {
                    int x = GetXAlight(str, 0);
                    FontRenderer.RenderString(x, 14 + h, str, size, color, Alpha, true);
                    h += FontAdvance.VertAdvance[(int)size] + 4;
                }
            }
            GLRender.PopMatrix();
        }

        /// <summary>
        /// Перенести текст согласно ширине контрола
        /// </summary>
        public void Transfer()
        {
            TransferText transfer = new TransferText(size, Width, sizeInterface);
            transfer.Run(Text);
            Text = transfer.OutText;
        }

    }
}
