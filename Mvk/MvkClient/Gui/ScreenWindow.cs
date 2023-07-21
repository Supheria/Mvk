using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    /// <summary>
    /// Абстрактный класс скрина которое создаёт окно
    /// </summary>
    public abstract class ScreenWindow : Screen
    {
        /// <summary>
        /// Точка положения
        /// </summary>
        protected vec2i position = new vec2i(0);
        /// <summary>
        /// Размер окна, контейнер для инвентаря и чата 512*354
        /// </summary>
        protected vec2i size = new vec2i(512, 354);

        protected string textTitle = "";
        /// <summary>
        /// Прозрачность чата
        /// </summary>
        protected float alpha = 1f;
        /// <summary>
        /// Цвет окна
        /// </summary>
        protected vec3 color = new vec3(1);
        /// <summary>
        /// Текстура контейнера
        /// </summary>
        protected AssetsTexture assetsTexture = AssetsTexture.ConteinerItems;

        public ScreenWindow(Client client) : base(client) { }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (IsOutsideWindow(x, y))
            {
                OnClickOutsideWindow();
            }
            else
            {
                base.MouseDown(button, x, y);
            }
        }

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected virtual void OnClickOutsideWindow() => ClientMain.Screen.GameMode();

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public override void KeyDown(int key)
        {
            if (key == 27 || key == 69) // ESC || E
            {
                ClientMain.Screen.GameMode();
            }
        }

        /// <summary>
        /// Курсор за пределами окна
        /// </summary>
        protected bool IsOutsideWindow(int x, int y) 
            => x < position.x || x > (position.x + size.x * SizeInterface) 
            || y < position.y || y > (position.y + size.y * SizeInterface);

        /// <summary>
        /// Окно
        /// </summary>
        protected override void RenderWindow()
        {
            GLRender.PushMatrix();
            GLRender.Texture2DEnable();
            GLWindow.Texture.BindTexture(assetsTexture);
            GLRender.Translate(position.x, position.y, 0);
            GLRender.Scale(SizeInterface);
            GLRender.Color(color.x, color.y, color.z, alpha);
            GLRender.Rectangle(0, 0, 512, 354, 0, 0, 1f, 0.69140625f);
            // назва
            GLWindow.Texture.BindTexture(AssetsTexture.Font12);
            string text = Language.T(textTitle);
            FontRenderer.RenderString(12, 10, text, FontSize.Font12, new vec3(1), alpha, true, .1f);

            GLRender.PopMatrix();
        }
    }
}
