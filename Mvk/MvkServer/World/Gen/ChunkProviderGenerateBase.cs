using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen
{
    public abstract class ChunkProviderGenerateBase
    {
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Шум нижнего слоя
        /// </summary>
        public NoiseGeneratorPerlin NoiseDown { get; private set; }

        /// <summary>
        /// Шум для дополнительных областей, для корректировки рельефа
        /// </summary>
        public float[] AreaNoise { get; private set; } = new float[256];
        /// <summary>
        /// Вспомогательный рандом
        /// </summary>
        public Rand Random { get; private set; }
        public long Seed { get; private set; }

        protected readonly BiomeBase biomeBase;
        protected readonly BiomeBase biomeDebug;
        protected readonly BiomeBase[] biomes;

        /// <summary>
        /// Чанк для заполнения данных
        /// </summary>
        protected readonly ChunkPrimer chunkPrimer;

        public ChunkProviderGenerateBase(WorldServer worldIn)
        {
            World = worldIn;
            Seed = World.Info.Seed;

            NoiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
            Random = new Rand(Seed);
            chunkPrimer = new ChunkPrimer();

            biomeBase = new BiomeBase(this);
            biomeDebug = new BiomeDebug(this);
            biomes = new BiomeBase[12];

            biomes[0] = new BiomeSea(this);
            biomes[1] = new BiomeRiver(this);
            biomes[2] = new BiomePlain(this);
            biomes[3] = new BiomeDesert(this);
            biomes[4] = new BiomeBeach(this);
            biomes[5] = new BiomeMixedForest(this);
            biomes[6] = new BiomeConiferousForest(this);
            biomes[7] = new BiomeBirchForest(this);
            biomes[8] = new BiomeTropics(this);
            biomes[9] = new BiomeSwamp(this);
            biomes[10] = new BiomeMountains(this);
            biomes[11] = new BiomeMountainsDesert(this);
        }

        /// <summary>
        /// Генерация чанка
        /// </summary>
        public virtual void GenerateChunk(ChunkBase chunk) { }

        /// <summary>
        /// Декорация чанков, требуются соседние чанки (3*3)
        /// </summary>
        public virtual void Populate(ChunkBase chunk) { }

        /// <summary>
        /// Столбец речных шумов
        /// </summary>
        /// <param name="cr">шум реки</param>
        /// <param name="ch">шум высоты</param>
        /// <param name="x">координата столбца X</param>
        /// <param name="z">координата столбца Z</param>
        /// <param name="enumBiome">биом столбца</param>
        /// <param name="min">минимальный коэф для ширины реки</param>
        /// <param name="max">максимальны коэф для ширины реки</param>
        /// <param name="size">размер для разницы коэф, чтоб значение было 2, пример: min=0.1 и max=0.3 size = 2 / (max-min)</param>
        /// <param name="heightCave">Высота пещеры</param>
        /// <param name="heightLevel">Уровень амплитуды пещер по миру</param>
        /// <param name="level">Центр амплитуды Y</param>
        protected void ColumnCave2d(float cr, float ch, int x, int z, EnumBiome enumBiome,
            float min, float max, float size, float heightCave, float heightLevel, int level)
        {
            // Пещенры 2д ввиде рек
            if ((cr >= min && cr <= max) || (cr <= -min && cr >= -max))
            {
                float h = (enumBiome == EnumBiome.River || enumBiome == EnumBiome.Sea || enumBiome == EnumBiome.Swamp)
                    ? chunkPrimer.heightMap[x << 4 | z] : ChunkBase.COUNT_HEIGHT_BLOCK;
                h -= 4;
                if (h > BiomeBase.HEIGHT_WATER) h = ChunkBase.COUNT_HEIGHT_BLOCK;

                if (cr < 0) cr = -cr;
                cr = (cr - min) * size;
                if (cr > 1f) cr = 2f - cr;
                cr = 1f - cr;
                cr = cr * cr;
                cr = 1f - cr;
                int ych = (int)(cr * heightCave) + 3;
                ych = (ych / 2);

                int ych2 = level + (int)(ch * heightLevel);
                int cy1 = ych2 - ych;
                if (cy1 < 3) cy1 = 3;
                int cy2 = ych2 + ych;
                if (cy2 > ChunkBase.COUNT_HEIGHT_BLOCK) cy2 = ChunkBase.COUNT_HEIGHT_BLOCK;
                int index, id;

                // Высота пещерных рек 4 .. ~120
                for (int y = cy1; y <= cy2; y++)
                {
                    if (y < h)
                    {
                        index = x << 12 | z << 8 | y;
                        id = chunkPrimer.id[index];
                        if (id == 3 || id == 9 || id == 10 || id == 7
                            || (id == 8 && chunkPrimer.id[index + 1] != 13))
                        {
                            if (y < 12)
                            {
                                chunkPrimer.id[index] = 15; // лава
                                chunkPrimer.arrayLightBlocks.Add(new vec3i(x, y, z));
                            }
                            else chunkPrimer.id[index] = 0; // воздух
                        }
                    }
                }
            }
        }

    }
}
