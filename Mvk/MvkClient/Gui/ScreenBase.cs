using MvkClient.Renderer;
using MvkClient.Setitings;
using MvkServer.Network;
using SharpGL;

namespace MvkClient.Gui
{
    /// <summary>
    /// Абстрактный класс базового скрина, для последующих всех скринов
    /// </summary>
    public abstract class ScreenBase
    {
        /// <summary>
        /// Ширина окна
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высота окна
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        public int SizeInterface { get; protected set; }
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; private set; }
        
        /// <summary>
        /// Объект OpenGL
        /// </summary>
        protected OpenGL gl;

        public ScreenBase(Client client)
        {
            SizeInterface = Setting.SizeInterface;
            ClientMain = client;
        }

        public void Initialize()
        {
            gl = GLWindow.gl;
            Width = GLWindow.WindowWidth;
            Height = GLWindow.WindowHeight;
            Init();
            Resized();
        }

        protected virtual void Init() { }

        /// <summary>
        /// Такт игрового времени
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public virtual void Resized()
        {
            Width = GLWindow.WindowWidth;
            Height = GLWindow.WindowHeight;
            SizeInterface = Setting.SizeInterface;
        }

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public virtual void AcceptNetworkPackage(IPacket packet) { }

        /// <summary>
        /// Матрица проекции камеры для 2д, GUI
        /// </summary>
        protected void Ortho2D()
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho(0, Width, Height, 0, -100, 100);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

        /// <summary>
        /// Прорисовать прямоугольник с текстурой, где расчёт текстуры через пиксели, где текстура 512*512
        /// </summary>
        protected void DrawTexturedModalRect(int x, int y, int textureX, int textureY, int width, int height)
            => GLRender.Rectangle(x, y, x + width, y + height, textureX * .001953125f, textureY * .001953125f,
                (textureX + width) * .001953125f, (textureY + height) * .001953125f);
    }
}
