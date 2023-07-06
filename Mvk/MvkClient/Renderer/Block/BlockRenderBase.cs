using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// абстрактный клас рендера блока
    /// </summary>
    public abstract class BlockRenderBase
    {
        /// <summary>
        /// Статическая переменная всех 255
        /// </summary>
        public static byte[] colorsFF = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

        /// <summary>
        /// Объект блока кэш
        /// </summary>
        public BlockBase block;
        public BlockState blockState;
        public int met;

        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkX;
        /// <summary>
        /// Позиция блока в чанке 0..255
        /// </summary>
        public int posChunkY;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkZ;

        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        protected ChunkRender chunk;
        /// <summary>
        /// Пометка, разрушается ли блок и его стадия
        /// -1 не разрушается, 0-9 разрушается
        /// </summary>
        protected int damagedBlocksValue = -1;
        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        protected readonly float[] lightPoles = new float[] { 1, .6f, .7f, .7f, .85f, .85f };
        protected float lightPole;
        protected ColorsLights colorLight;
        protected vec3 color;
        protected vec3 colorWhite = new vec3(1);
        protected int side;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRenderBase()
        {
            colorLight = new ColorsLights();
            colorLight.Init();
        }

        public virtual void InitChunk(ChunkRender chunkRender) => chunk = chunkRender;

        /// <summary>
        /// Сгенерировать cетку блока
        /// </summary>
        public virtual void RenderMesh() { }

        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        protected virtual void GenColors(byte light)
        {
            color = GetBiomeColor(chunk, posChunkX, posChunkZ);
            //lightPole = block.NoSideDimming ? 0f : 1f - lightPoles[side];

            color.x -= lightPole; if (color.x < 0) color.x = 0;
            color.y -= lightPole; if (color.y < 0) color.y = 0;
            color.z -= lightPole; if (color.z < 0) color.z = 0;

            colorLight.colorr[0] = colorLight.colorr[1] = colorLight.colorr[2] = colorLight.colorr[3] = (byte)(color.x * 255);
            colorLight.colorg[0] = colorLight.colorg[1] = colorLight.colorg[2] = colorLight.colorg[3] = (byte)(color.y * 255);
            colorLight.colorb[0] = colorLight.colorb[1] = colorLight.colorb[2] = colorLight.colorb[3] = (byte)(color.z * 255);
            colorLight.light[0] = colorLight.light[1] = colorLight.light[2] = colorLight.light[3] = light;
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        /// <returns></returns>
        protected virtual vec3 GetBiomeColor(ChunkBase chunk, int bx, int bz) => colorWhite;

        /// <summary>
        /// Структура для вершин цвета и освещения
        /// </summary>
        protected struct ColorsLights
        {
            public byte[] colorr;
            public byte[] colorg;
            public byte[] colorb;
            public byte[] light;

            public void Init()
            {
                colorr = new byte[4];
                colorg = new byte[4];
                colorb = new byte[4];
                light = new byte[4];
            }

            public void InitColorsLights(vec3 color1, vec3 color2, vec3 color3, vec3 color4,
                byte light1, byte light2, byte light3, byte light4)
            {
                colorr[0] = (byte)(color1.x * 255);
                colorr[1] = (byte)(color2.x * 255);
                colorr[2] = (byte)(color3.x * 255);
                colorr[3] = (byte)(color4.x * 255);

                colorg[0] = (byte)(color1.y * 255);
                colorg[1] = (byte)(color2.y * 255);
                colorg[2] = (byte)(color3.y * 255);
                colorg[3] = (byte)(color4.y * 255);

                colorb[0] = (byte)(color1.z * 255);
                colorb[1] = (byte)(color2.z * 255);
                colorb[2] = (byte)(color3.z * 255);
                colorb[3] = (byte)(color4.z * 255);

                light[0] = light1;
                light[1] = light2;
                light[2] = light3;
                light[3] = light4;
            }
        }
    }
}
