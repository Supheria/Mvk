using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    /// <summary>
    /// Объект кнопки 32*32
    /// </summary>
    public class ButtonBox : Control
    {
        /// <summary>
        /// Ключи для нажатий кнопки и понимания их действий
        /// </summary>
        public EnumScreenKey ScreenKey { get; protected set; } = EnumScreenKey.None;

        public ButtonBox(string text) : base(text) { Height = 32; Width = 32; }
        public ButtonBox(EnumScreenKey key, string text) : base(text) => ScreenKey = key;

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, Alpha);
            float v1 = Enabled ? enter ? .875f : .75f : .625f;
            GLRender.Rectangle(0, 0, Width, Height, v1, .46875f, v1 + .125f, .59375f);

            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            int x = GetXAlight(Text, 12);
            vec3 color = Enabled ? enter ? new vec3(1, 1, .5f) : new vec3(1) : new vec3(.5f);
            FontRenderer.RenderString(x, 11, Text, size, color, Alpha, Enabled, .1f);
        }

        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                MouseMove(x, y);
                if (enter)
                {
                    SampleClick();
                    OnClick();
                }
            }
        }
    }
}
