using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Chunk;
using SharpGL;
using System.Collections.Generic;
using System.Drawing;

namespace MvkClient
{
    /// <summary>
    /// Класс для отладки, и видеть на экране
    /// </summary>
    public class Debug
    {
        /// <summary>
        /// Выводить ли на экран
        /// </summary>
        public static bool IsDraw = false;
        /// <summary>
        /// Прорисовать в 2д чанки 
        /// </summary>
        public static bool IsDrawServerChunk = false;
        /// <summary>
        /// Демонстрация FrustumCulling
        /// </summary>
        public static bool IsDrawFrustumCulling = false;
        /// <summary>
        /// Массив 2д чанков, которые попадают в FrustumCulling без офсет для отладки
        /// </summary>
        public static List<vec2i> DrawFrustumCulling = new List<vec2i>();
        /// <summary>
        /// Нужен рендер для отладки FrustumCulling
        /// </summary>
        public static bool RenderFrustumCulling = false;
        /// <summary>
        /// Прорисовка вокселей контуром линий
        /// </summary>
        public static bool IsDrawVoxelLine = false;
        /// <summary>
        /// Воксели подставить матрицу ortho
        /// </summary>
        public static bool IsDrawOrto = false;
        /// <summary>
        /// Количество мешей
        /// </summary>
        public static long CountMesh = 0;
        /// <summary>
        /// Количество мешей всего попадает под обзор
        /// </summary>
        public static long CountMeshAll = 0;
        /// <summary>
        /// Количество полигонов блоков
        /// </summary>
        public static int CountPoligon = 0;
        /// <summary>
        /// Сколько обновилось чанков
        /// </summary>
        public static int CountUpdateChunck = 0;
        /// <summary>
        /// Фокус блок
        /// </summary>
        public static string BlockFocus = "";
        /// <summary>
        /// Время рендера чанка в мс, среднее из последних 8
        /// </summary>
        public static float RenderChunckTime8 = 0;
        public static float RenderChunckTime = 0;

        /// <summary>
        /// Трафик в байтах
        /// </summary>
        public static long Traffic = 0;

        public static int DInt = 0;
        public static long DLong = 0;
        public static float DFloat = 0;
        public static string DStr = "";

        public static bool DStart = false;

        protected static string strTpsFps = "";
        public static void SetTpsFps(int fps, float speedFrame, int tps, float speedTick, int countUpdateChunk) 
            => strTpsFps = string.Format("Speed: {0} fps {1:0.00} ms {2} tps {3:0.00} ms ({4})", fps, speedFrame, tps, speedTick, countUpdateChunk);

        public static string strServer = "";
        public static string strClient = "";
        public static string strSound = "";
        public static string version = "";



        public static DebugChunk ListChunks { get; set; } = new DebugChunk();

        protected static string ToStringDebug()
        {
            string s = strServer == "" ? "" : "Server " + strServer + "\r\n";
            string c = strClient == "" ? "" : "Client " + strClient + "\r\n"
                    + "Traffic mb:" + (Traffic / 1048576f).ToString("0.00") + "\r\n"
                    + "RenderChunk8 ms:" + RenderChunckTime8.ToString("0.000") + "\r\n";
            
            return version + "\r\n"
                + strTpsFps + "\r\n"
                + "Sound: " + strSound + "\r\n"
                + s + c 
                + BlockFocus
                + string.Format("Mesh: {0}/{6} Poligons: {4}\r\nint: {1} float: {2:0.00} string: {3} long: {5}", 
                CountMesh, DInt, DFloat, DStr, CountPoligon, DLong, CountMeshAll);
        }

        #region Draw

        private static uint dList;
        private static uint dListCh;
        private static uint dListFC;
        

        public static void RenderDebug()
        {
            if (IsDraw)
            {
                GLRender.ListDelete(dList);
                dList = GLRender.ListBegin();
                GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
                GLWindow.gl.LoadIdentity();
                GLWindow.gl.Ortho2D(0, GLWindow.WindowWidth, GLWindow.WindowHeight, 0);
                GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
                GLWindow.gl.LoadIdentity();
                GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.Font12);
                FontRenderer.RenderText(11f, 11f, new vec4(.2f, .2f, .2f, 1f), ToStringDebug(), FontSize.Font12);
                FontRenderer.RenderText(10f, 10f, new vec4(0.9f, 0.9f, .6f, 1f), ToStringDebug(), FontSize.Font12);
                GLRender.ListEnd();
            }

            if (IsDrawServerChunk && ListChunks.isRender && (strServer != "" || ListChunks.listChunkPlayer != null))
            {
                ListChunks.isRender = false;
                GLRender.ListDelete(dListCh);
                dListCh = GLRender.ListBegin();
                // сетка карты
                GLWindow.gl.Disable(OpenGL.GL_TEXTURE_2D);

                // квадрат
                int x = GLWindow.WindowWidth / 2;
                int z = GLWindow.WindowHeight / 2;
                vec4[] colPr = new vec4[] {
                        new vec4(.1f, .5f, .9f, 1f),
                        new vec4(.1f, .6f, .8f, 1f),
                        new vec4(.2f, .7f, .7f, 1f),
                        new vec4(.3f, .8f, .6f, 1f),
                        new vec4(.2f, .9f, .5f, 1f),
                        new vec4(.9f, .9f, .9f, 1f),
                        new vec4(.0f, .0f, .6f, 1f),
                        new vec4(.9f, .0f, .0f, 1f),
                    };
                int size = 8;

                if (ListChunks.listChunkServer != null)
                {
                    // green || red
                    foreach (DebugChunkValue p in ListChunks.listChunkServer)
                    {
                        vec2i pos = new vec2i(p.pos.x * size, p.pos.y * size);
                        if (p.entities) GLRender.Rectangle(pos.x + x, -pos.y + z, pos.x + x + size, -pos.y + z + size, colPr[7]);
                        else GLRender.Rectangle(pos.x + x, -pos.y + z, pos.x + x + size, -pos.y + z + size, colPr[4]);
                    }
                }
                if (ListChunks.listChunkPlayers != null)
                {
                    // White
                    foreach (vec2i p in ListChunks.listChunkPlayers)
                    {
                        vec2i pos = p * size;
                        GLRender.Rectangle(pos.x + x + 2, -pos.y + z + 2, pos.x + x + size - 2, -pos.y + z + size - 2, colPr[5]);
                    }
                }
                if (ListChunks.listChunkPlayer != null)
                {
                    // Blue
                    foreach (DebugChunkValue p in ListChunks.listChunkPlayer)
                    {
                        vec2i pos = new vec2i(p.pos.x * size, p.pos.y * size);
                        GLRender.Rectangle(pos.x + x + 3, -pos.y + z + 3, pos.x + x + size - 3, -pos.y + z + size - 3, colPr[6]);
                    }
                }

                GLRender.ListEnd();
            }

            if (IsDrawFrustumCulling && RenderFrustumCulling)
            {
                RenderFrustumCulling = false;
                GLRender.ListDelete(dListFC);
                dListFC = GLRender.ListBegin();
                // сетка карты
                GLWindow.gl.Disable(OpenGL.GL_TEXTURE_2D);

                int x = GLWindow.WindowWidth / 2;
                int z = GLWindow.WindowHeight / 2;
                int size = 8;

                if (DrawFrustumCulling.Count > 0)
                {
                    foreach (vec2i p in DrawFrustumCulling)
                    {
                        vec2i pos = new vec2i(p.x * size, p.y * size);
                        GLRender.Rectangle(pos.x + x + 1, -pos.y + z + 1, pos.x + x + size - 1, -pos.y + z + size - 1, new vec4(.2f, .7f, .7f, 1f));
                    }
                }
                GLRender.Rectangle(3, 3, -2, -2, new vec4(.9f, .9f, .9f, 1f));
                GLRender.ListEnd();
            }
        }

        public static void DrawDebug()
        {
            if (IsDraw) GLRender.ListCall(dList);
            if (IsDrawServerChunk) GLRender.ListCall(dListCh);
            if (IsDrawFrustumCulling) GLRender.ListCall(dListFC);
        }

        #endregion

        public static void ScreenFileBiome(WorldClient world)
        {
            int ov = Setitings.Setting.OverviewChunk;
            vec2i posCh = new vec2i((int)(world.ClientMain.Player.Position.x) >> 4, (int)(world.ClientMain.Player.Position.z) >> 4);
            int ofxc = ((ov << 4) + 16) - (posCh.x << 4);
            int ofzc = ((ov << 4) + 16) - (posCh.y << 4);
            int size = (ov + ov + 1) * 16 + 32;
            Bitmap bitmap = new Bitmap(size, size);

            EnumBiome[] biome;
            ChunkBase chunk;
            int x, z, bxc, bzc;
            if (ListChunks.listChunkServer != null)
            {
                foreach (DebugChunkValue p in ListChunks.listChunkServer)
                {
                    bxc = p.pos.x << 4;
                    bzc = p.pos.y << 4;
                    chunk = world.GetChunk(p.pos);
                    if (chunk != null)
                    {
                        biome = chunk.biome;
                        for (x = 0; x < 16; x++)
                        {
                            for (z = 0; z < 16; z++)
                            {
                                bitmap.SetPixel(bxc + ofxc + x, bzc + ofzc + z, ConvertBiome(biome[x << 4 | z]));
                            }
                        }
                    }
                }
            }
            bitmap.Save("biome.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private static Color ConvertBiome(EnumBiome biome)
        {
            switch (biome)
            {
                case EnumBiome.Sea: return Color.Blue; // море
                case EnumBiome.River: return Color.MediumBlue; // река
                case EnumBiome.Plain: return Color.LightGreen; // Равнина
                case EnumBiome.Desert: return Color.Yellow; // Пустяня
                case EnumBiome.Beach: return Color.White; // Пляж
                case EnumBiome.MixedForest: return Color.Green; // Сешанный лес
                case EnumBiome.ConiferousForest: return Color.DarkGreen; // Хвойный лес
                case EnumBiome.BirchForest: return Color.ForestGreen; // Берёзовый лес
                case EnumBiome.Tropics: return Color.Orange; // Тропики
                case EnumBiome.Swamp: return Color.CadetBlue; // Болото
                case EnumBiome.Mountains: return Color.LightGray; // Горы
                case EnumBiome.MountainsDesert: return Color.Brown; // Горы в пустыне
            }
            return Color.Black;
        }
    }
}
