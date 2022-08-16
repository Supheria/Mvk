using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Базовый биом, для всех
    /// </summary>
    public class BiomeBase
    {
        public ChunkProviderGenerate Provider { get; protected set; }

        private readonly float[] downNoise = new float[256];

        protected ChunkPrimer chunk;
        protected int xbc;
        protected int zbc;

        protected BiomeBase() { }
        public BiomeBase(ChunkProviderGenerate chunkProvider) => Provider = chunkProvider;

        public void Init(ChunkPrimer chunk, int xbc, int zbc)
        {
            this.chunk = chunk;
            this.xbc = xbc;
            this.zbc = zbc;
        }

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public virtual void Column(int x, int z, float height, float river)
        {
            int yh = 97 + (int)(height * 96f);
            int y;

            for (y = 3; y <= yh; y++)
            {
                chunk.data[x << 12 | z << 8 | y] = 3;
            }
            if (yh < 96)
            {
                yh++;
                for (y = yh; y < 97; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 13;
                }
            }
        }

        /// <summary>
        /// Генерация наза 0-3 блока
        /// </summary>
        public void Down()
        {
            float scale = 1.0f;
            Provider.NoiseDown.GenerateNoise2d(downNoise, xbc, zbc, 16, 16, scale, scale);

            int count = 0;
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    float n = downNoise[count];
                    count++;

                    chunk.data[x << 12 | z << 8] = 2;
                    chunk.data[x << 12 | z << 8 | 1] = (ushort)(n < .3 ? 2 : 3);
                    chunk.data[x << 12 | z << 8 | 2] = (ushort)(n < -.3 ? 2 : 3);
                }
            }
        }

        /// <summary>
        /// Генерация облостей
        /// </summary>
        public void Area(float[] noise, int x, int z, EnumBlock eBlock, bool big)
        {
            int realX = xbc + x;
            int realZ = zbc + z;
            float res;
            if (big)
            {
               // Provider.NoiseArea.GenerateNoise2d(downNoise, realX, realZ, 1, 1, .1f, .1f);
                res = -.5f;
            }
            else
            {
               // Provider.NoiseDown.GenerateNoise2d(downNoise, realX, realZ, 1, 1, .1f, .1f);
                res = -.8f;
            }

            //if (downNoise[0] < res)
            {
                
                ushort id = (ushort)eBlock;
                int y8 = (int)((downNoise[0] + 1f) * 64f) + 64;
                for (int y = 3; y < 128; y++)
                {
                    if (noise[x << 11 | z << 7 | y] < res)
                    {
                        chunk.data[x << 12 | z << 8 | y] = id;
                    }
                }
            }
        }

    }
}
