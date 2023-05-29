using MvkClient.Renderer.Shaders;
using MvkServer.Util;
using SharpGL;
using SharpGL.Shaders;
using System;
using System.Diagnostics;

namespace MvkClient.Renderer
{
    /// <summary>
    /// OpenGL обращение с окном
    /// </summary>
    public class GLWindow
    {
        /// <summary>
        /// Объект OpenGL
        /// </summary>
        public static OpenGL gl { get; protected set; }
        /// <summary>
        /// Текстуры
        /// </summary>
        public static TextureMap Texture { get; protected set; }
        /// <summary>
        /// Ширина окна
        /// </summary>
        public static int WindowWidth { get; protected set; }
        /// <summary>
        /// Высота окна
        /// </summary>
        public static int WindowHeight { get; protected set; }
        /// <summary>
        /// Объект шейдоров
        /// </summary>
        public static ShaderItems Shaders { get; protected set; } = new ShaderItems();

        /// <summary>
        /// Таймер для фиксации времени прорисовки кадра
        /// </summary>
        public static Stopwatch stopwatch = new Stopwatch();
        private static float speedFrameAll;
        private static long timerSecond;
        private static int fps;
        private static int tps;
        private static float speedTickAll;
        private static long tickDraw;

        /// <summary>
        /// Инициализировать, первый запуск OpenGL
        /// </summary>
        public static void Initialize(OpenGL gl)
        {
            string version = gl.GetString(OpenGL.GL_VERSION);
            string versionPrefix = version.Substring(0, 1);
            if (versionPrefix != "3" && versionPrefix != "4")
            {
                throw new Exception("Для игры нужна версия OpenGL 3.3 и выше!");
            }

            GLWindow.gl = gl;
            GLRender.Initialize();
            stopwatch.Start();

            gl.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            Texture = new TextureMap();
            Texture.InitializeOne();

            try
            {
                Shaders.Create(gl);
            }
            catch (ShaderCompilationException ex)
            {
                Logger.Crach(ex, "ErrorShader: {0}", ex.CompilerOutput);
                throw new Exception("ErrorShader: " + ex.CompilerOutput);
            }
        }

        public static void Resized(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
        }

        /// <summary>
        /// Прорисовка каждого кадра
        /// </summary>
        /// <param name="screen">объект GUI</param>
        public static void Draw(Client client)
        {
            try
            {
                DrawBegin();
                // тут мир
                if (client.World != null)
                {
                    // коэффициент интерполяции
                    float timeIndex = client.World.Interpolation();

                    // Мир
                    client.World.WorldRender.Draw(timeIndex);
                }
                // тут gui
                client.Screen.DrawScreen();

                DrawEnd();
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// В такте игрового времени
        /// </summary>
        /// <param name="time">время затраченное на такт</param>
        public static void UpdateTick(float time)
        {
            speedTickAll += time;
            tps++;
        }

        #region Draw

        /// <summary>
        /// Перед прорисовка каждого кадра OpenGL
        /// </summary>
        private static void DrawBegin()
        {
            fps++;
            tickDraw = stopwatch.ElapsedTicks;
            Debug.CountPoligon = 0;
            Debug.CountMesh = 0;
            //Debug.CountMeshAll = 0;
            //gl.Perspective(70.0f, (float)windowWidth / (float)windowHeight, 0.1f, 512);

            gl.Viewport(0, 0, WindowWidth, WindowHeight);
            // ! Устанавливаем шейдерную программу текущей
            // Подробнее на esate.ru: http://esate.ru/uroki/OpenGL/uroki_opengl/_p4208/
            //gl.UseProgram(Shaders.ShVoxel.ShaderProgramObject);

            // Включает Буфер глубины 
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            gl.ClearColor(.5f, .7f, .99f, 1f);

            // Код с фиксированной функцией может использовать альфа-тестирование
            // Чтоб корректно прорисовывался кактус
            gl.AlphaFunc(OpenGL.GL_GREATER, 0.1f);
            gl.Enable(OpenGL.GL_ALPHA_TEST);
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        private static void DrawEnd()
        {
            // Перерасчёт кадров раз в секунду, и среднее время прорисовки кадра
            if (Client.Time() >= timerSecond + 1000)
            {
                int countChunk = Debug.CountUpdateChunck;
                Debug.CountUpdateChunck = 0;
                int countChunkAlpha = Debug.CountUpdateChunckAlpha;
                Debug.CountUpdateChunckAlpha = 0;
                float speedTick = 0;
                if (tps > 0) speedTick = speedTickAll / tps;
                Debug.SetTpsFps(fps, speedFrameAll / fps, tps, speedTick, countChunk, countChunkAlpha);
                
                timerSecond += 1000;
                speedFrameAll = 0;
                speedTickAll = 0;
                fps = 0;
                tps = 0;
            }
            // ! Отключаем шейдерную программу
            //gl.UseProgram(0);
            Debug.DrawDebug();
            float timeFrame = (float)(stopwatch.ElapsedTicks - tickDraw) / MvkStatic.TimerFrequency;
            speedFrameAll += timeFrame;// (float)(stopwatch.ElapsedTicks - tickDraw) / MvkStatic.TimerFrequency;
            Debug.FrizFps += timeFrame < 8 ? "." : timeFrame < 17 ? "," : timeFrame < 34 ? ":" : "|";
            if (Debug.FrizFps.Length > 120) Debug.FrizFps = Debug.FrizFps.Substring(1);
        }

        #endregion
    }
}
