using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using MvkServer.World.Gen;
using System;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Базовый биом, для всех
    /// </summary>
    public class BiomeBase
    {
        /// <summary>
        /// Высота уровня воды
        /// </summary>
        public const int HEIGHT_WATER = 48;// 96;
        /// <summary>
        /// Высота воды -1
        /// </summary>
        public const int HEIGHT_WATER_MINUS = 47;// 95;
        /// <summary>
        /// Высота воды -2
        /// </summary>
        public const int HEIGHT_WATER_MINUS_2 = 46;// 94;
        /// <summary>
        /// Высота воды +1
        /// </summary>
        public const int HEIGHT_WATER_PLUS = 49;// 97;
        /// <summary>
        /// Высота холмов, плюсует к высоте воды
        /// </summary>
        public const int HEIGHT_HILL = 72;// 96;
        /// <summary>
        /// Высота в горах, проплешина с землёй
        /// </summary>
        public const int HEIGHT_MOUNTAINS_MIX = 58;//  58;// 106;
        /// <summary>
        /// Высота в горах пустынных
        /// </summary>
        public const int HEIGHT_MOUNTAINS_DESERT = 60;// 108;
        /// <summary>
        /// Высота холмов на пляже
        /// </summary>
        public const int HEIGHT_HILL_BEACH = 288; // 288;
        /// <summary>
        /// Высота холмов на море
        /// </summary>
        public const int HEIGHT_HILL_SEA = 384; // 384;
        /// <summary>
        /// Минимальная высота для декорации блинчиков
        /// </summary>
        public const int HEIGHT_PANCAKE_MIN = 40;// 88;
        /// <summary>
        /// Центр амплитуды пещер
        /// </summary>
        public const int HEIGHT_CENTER_CAVE = 32;// 64;

        public ChunkProviderGenerateBase Provider { get; protected set; }
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
        /// Блок для отладки визуализации биома
        /// </summary>
        protected ushort blockIdBiomDebug;
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
        public BiomeBase(ChunkProviderGenerateBase chunkProvider)
        {
            Provider = chunkProvider;
            rand = Provider.Random;
            blockIdBiomDebug = blockIdUp = blockIdBody = (ushort)EnumBlock.Stone;
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public virtual void InitDecorator(bool isRobinson)
        {
            Decorator = isRobinson ? new BiomeDecoratorRobinson(Provider.World) : new BiomeDecorator(Provider.World);
            Decorator.Init();
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
            if (yh < 2) yh = 2;
            chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y = 0;

            try
            {
                if (isBlockBody)
                {
                    // Определяем высоту тела по шуму (3 - 6)
                    int bodyHeight = (int)(Provider.AreaNoise[x << 4 | z] / 4f + 5f);

                    int yb = yh - bodyHeight;
                    if (yb < 2) yb = 2;

                    // заполняем камнем
                    for (y = 3; y < yb; y++) chunk.id[index | y] = 3;
                    // заполняем тело
                    for (y = yb; y < yh; y++) chunk.id[index | y] = blockIdBody;
                    chunk.id[x << 12 | z << 8 | yh] = yh < HEIGHT_WATER ? blockIdBody : blockIdUp;
                }
                else
                {
                    // заполняем камнем
                    for (y = 3; y <= yh; y++) chunk.id[index | y] = 3;
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex, "yh:{0} y:{1} index:{2}", yh, y, index);
            }
            if (yh < HEIGHT_WATER)
            {
                // меньше уровня воды
                yh++;
                for (y = yh; y < HEIGHT_WATER_PLUS; y++)
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
        protected virtual int GetLevelHeight(int x, int z, float height, float river) 
            => HEIGHT_WATER + (int)(height * HEIGHT_HILL);

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

        /// <summary>
        /// Возращаем сгенерированный блок на 4 высоте, конкретного биома
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public virtual void ViewDebugBiom(int x, int z)//, float height, float river)
        {
            chunk.id[x << 12 | z << 8 | 3] = blockIdBiomDebug;
        }

        #region Robinson

        /// <summary>
        /// Возращаем сгенерированный столбец и возвращает фактическую высоту, без воды
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        public int ColumnRobinson(int x, int z, int height)
        {
            int yh = height;
            if (yh < 2) yh = 2;
            int result = chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y = 0;

            try
            {
                if (isBlockBody)
                {
                    // Определяем высоту тела по шуму (3 - 6)
                    int bodyHeight = (int)(Provider.AreaNoise[x << 4 | z] / 4f + 5f);

                    int yb = yh - bodyHeight;
                    if (yb < 2) yb = 2;

                    // заполняем камнем
                    for (y = 3; y < yb; y++) chunk.id[index | y] = 3;
                    // заполняем тело
                    BodyRobinson(yb, yh, index);
                }
                else
                {
                    // заполняем камнем
                    for (y = 3; y <= yh; y++) chunk.id[index | y] = 3;
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex, "yh:{0} y:{1} index:{2}", yh, y, index);
            }
            if (yh < HEIGHT_WATER)
            {
                // меньше уровня воды
                yh++;
                for (y = yh; y < HEIGHT_WATER_PLUS; y++)
                {
                    // заполняем водой
                    chunk.id[index | y] = 13;
                }
            }
            return result;
        }

        /// <summary>
        /// Заполняем тело
        /// </summary>
        protected virtual void BodyRobinson(int yb, int yh, int index)
        {
            for (int y = yb; y < yh; y++) chunk.id[index | y] = blockIdBody;
            chunk.id[index | yh] = yh < HEIGHT_WATER ? blockIdBody : blockIdUp;
        }

        #endregion
    }
}

