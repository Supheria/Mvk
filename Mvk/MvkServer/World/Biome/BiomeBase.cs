using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Базовый биом, для всех
    /// </summary>
    public class BiomeBase
    {
        public ChunkProviderGenerate Provider { get; protected set; }
        /// <summary>
        /// Декорация
        /// </summary>
        public BiomeDecorator Decorator { get; private set; }

        private readonly float[] downNoise = new float[256];

        protected ChunkPrimer chunk;
        protected int xbc;
        protected int zbc;

        protected Rand rand;

        /// <summary>
        /// Вверхний блок
        /// </summary>
        protected ushort blockIdUp;
        /// <summary>
        /// Блок тела
        /// </summary>
        protected ushort blockIdBody;

        protected bool isBlockBody;

        protected BiomeBase() { }
        public BiomeBase(ChunkProviderGenerate chunkProvider)
        {
            Provider = chunkProvider;
            rand = Provider.Random;
            blockIdUp = (int)EnumBlock.Turf;
            blockIdBody = (int)EnumBlock.Dirt;
            isBlockBody = true;
            Decorator = new BiomeDecorator(Provider.World);
        }

        public void Init(ChunkPrimer chunk, int xbc, int zbc)
        {
            this.chunk = chunk;
            this.xbc = xbc;
            this.zbc = zbc;
        }

        /// <summary>
        /// Обновить сид по колонке
        /// </summary>
        public void ColumnUpSeed(int x, int z)
        {
            rand.SetSeed(Provider.Seed);
            int realX = (xbc + x) * rand.Next();
            int realZ = (zbc + z) * rand.Next();
            rand.SetSeed(realX ^ realZ ^ Provider.Seed);
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
            int yh = GetLevelHeight(x, z, height, river);
            chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y;

            if (isBlockBody)
            {
                // Определяем высоту тела по шуму (3 - 6)
                int bodyHeight = (int)(Provider.AreaNoise[x << 4 | z] / 4f + 5f);

                int yb = yh - bodyHeight;
                // заполняем камнем
                for (y = 3; y < yb; y++) chunk.id[index | y] = 3;
                // заполняем тело
                for (y = yb; y < yh; y++) chunk.id[index | y] = blockIdBody;
                chunk.id[x << 12 | z << 8 | yh] = yh < 96 ? blockIdBody : blockIdUp;
            }
            else
            {
                // заполняем камнем
                for (y = 3; y <= yh; y++) chunk.id[index | y] = 3;
            }

            if (yh < 96)
            {
                // меньше уровня воды
                yh++;
                for (y = yh; y < 97; y++)
                {
                    // заполняем водой
                    chunk.id[index | y] = 13;
                }
            }
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected virtual int GetLevelHeight(int x, int z, float height, float river) => 96 + (int)(height * 96f);

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
                    chunk.id[x << 12 | z << 8] = 2;
                    chunk.id[x << 12 | z << 8 | 1] = (ushort)(n < .1 ? 2 : 3);
                    chunk.id[x << 12 | z << 8 | 2] = (ushort)(n < -.1 ? 2 : 3);
                }
            }
        }
    }
}

