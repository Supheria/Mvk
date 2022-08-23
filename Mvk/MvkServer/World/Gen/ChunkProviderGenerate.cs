using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Chunk;
using System;
using System.Diagnostics;

namespace MvkServer.World.Gen
{
    public class ChunkProviderGenerate
    {
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Шум высот биомов
        /// </summary>
        private readonly NoiseGeneratorPerlin heightBiome;
        /// <summary>
        /// Шум влажности биомов
        /// </summary>
        private readonly NoiseGeneratorPerlin wetnessBiome;
        /// <summary>
        /// Шум температуры биомов
        /// </summary>
        private readonly NoiseGeneratorPerlin temperatureBiome;
        /// <summary>
        /// Шум рек биомов
        /// </summary>
        private readonly NoiseGeneratorPerlin riversBiome;
        /// <summary>
        /// Шум пещер
        /// </summary>
       // private readonly NoiseGeneratorPerlin cave;
        /// <summary>
        /// Шум облостей
        /// </summary>
        public NoiseGeneratorPerlin noiseArea;
        /// <summary>
        /// Шум нижнего слоя
        /// </summary>
        public NoiseGeneratorPerlin NoiseDown { get; private set; }

        private readonly float[] heightNoise = new float[256];
        private readonly float[] wetnessNoise = new float[256];
        private readonly float[] temperatureNoise = new float[256];
        private readonly float[] riversNoise = new float[256];
        //private readonly float[] downNoise = new float[256];
        //private readonly float[] areaNoise = new float[256];
        //private readonly float[] caveNoise = new float[4096];
        //private readonly float[] areaNoise = new float[32768];

        /// <summary>
        /// Шум для травы и другой растительности
        /// </summary>
        //public float[] GrassNoise { get; private set; } = new float[256];
        /// <summary>
        /// Шум для дополнительных областей, для корректировки рельефа
        /// </summary>
        public float[] AreaNoise { get; private set; } = new float[256];
        /// <summary>
        /// Вспомогательный рандом
        /// </summary>
        public Rand Random { get; private set; }
        public int Seed { get; private set; }

        public readonly BiomeBase[] biomes;
        private BiomeBase biomeBase;
        /// <summary>
        /// Счётчик биомов в чанке
        /// </summary>
        private int[] biomesCount;

        /// <summary>
        /// Чанк для заполнения данных
        /// </summary>
        private ChunkPrimer chunkPrimer;

        public ChunkProviderGenerate(WorldServer worldIn)
        {
            World = worldIn;
            Seed = World.Seed;
            heightBiome = new NoiseGeneratorPerlin(new Rand(Seed), 8);
            riversBiome = new NoiseGeneratorPerlin(new Rand(Seed + 6), 8);
            wetnessBiome = new NoiseGeneratorPerlin(new Rand(Seed + 8), 4); // 8
            temperatureBiome = new NoiseGeneratorPerlin(new Rand(Seed + 4), 4); // 8
            //cave = new NoiseGeneratorPerlin(new Rand(seed + 2), 2);
            NoiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
            noiseArea = new NoiseGeneratorPerlin(new Rand(Seed + 2), 4);

            Random = new Rand(Seed);
            chunkPrimer = new ChunkPrimer();

            biomeBase = new BiomeBase(this);
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

            biomesCount = new int[biomes.Length];


          //  for (int i = 0; i < 12; i++) biomes[i] = new BiomePlain(this);
        }

        /// <summary>
        /// Получить конкретный биом
        /// </summary>
        //private BiomeBase GetBiome(EnumBiome enumBiome) => biomes[(int)enumBiome];

        public ChunkBase GenerateChunk(ChunkBase chunk)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                chunkPrimer.Clear();
                //for (int i = 0; i < biomesCount.Length; i++) biomesCount[i] = 0;
                int xbc = chunk.Position.x << 4;
                int zbc = chunk.Position.y << 4;

                
                biomeBase.Init(chunkPrimer, xbc, zbc);
                biomeBase.Down();

                // Пакет для биомов и высот с рекой
                heightBiome.GenerateNoise2d(heightNoise, xbc, zbc, 16, 16, .2f, .2f);
                riversBiome.GenerateNoise2d(riversNoise, xbc, zbc, 16, 16, .1f, .1f);
                wetnessBiome.GenerateNoise2d(wetnessNoise, xbc, zbc, 16, 16, .0125f , .0125f); //*4
                temperatureBiome.GenerateNoise2d(temperatureNoise, xbc, zbc, 16, 16, .0125f , .0125f ); //*4

                // доп шумы
                //NoiseDown.GenerateNoise2d(GrassNoise, xbc, zbc, 16, 16, .5f, .5f);
                noiseArea.GenerateNoise2d(AreaNoise, xbc, zbc, 16, 16, .8f, .8f);

                BiomeData biomeData;
                BiomeBase biome = biomes[2];
                int x, y, z, idBiome;
                int count = 0;
                float h, r, t, w;
                
                // Пробегаемся по столбам
                for (x = 0; x < 16; x++)
                {
                    for (z = 0; z < 16; z++)
                    {
                        h = heightNoise[count] / 132f;
                        r = riversNoise[count] / 132f;
                        t = temperatureNoise[count] / 8.3f;
                        w = wetnessNoise[count] / 8.3f;
                        biomeData = DefineBiome(h, r, t, w);
                        chunkPrimer.biome[x << 4 | z] = biomeData.biome;
                        idBiome = (int)biomeData.biome;
                        biomesCount[idBiome]++;
                        biome = biomes[idBiome];
                        biome.Init(chunkPrimer, xbc, zbc);
                        biome.Column(x, z, biomeData.height, biomeData.river);
                        //biome.Area(areaNoise, x, z, EnumBlock.Dirt, true);
                        //biome.Area(areaNoise, x, z, EnumBlock.Sand, false);

                        count++;
                    }
                }

                // Находим биом которого больше всего
                //int countMax = 0;
                //for (int i = 0; i < biomesCount.Length; i++)
                //{
                //    if (biomesCount[i] > countMax) 
                //    {
                //        biome = biomes[i];
                //        countMax = biomesCount[i];
                //    }
                //    biomesCount[i] = 0;
                //}
                //// Декорации от биома которого больше всего
                //biome.Decorator.GenDecorations(chunkPrimer, biome, xbc, zbc);

                ChunkStorage chunkStorage;
                int yc, ycb, y0;
                for (yc = 0; yc < ChunkBase.COUNT_HEIGHT; yc++)
                {
                    ycb = yc << 4;
                    chunkStorage = chunk.StorageArrays[yc];
                    for (y = 0; y < 16; y++)
                    {
                        y0 = ycb | y;
                        for (x = 0; x < 16; x++)
                        {
                            for (z = 0; z < 16; z++)
                            {
                                chunkStorage.SetData(y << 8 | z << 4 | x, chunkPrimer.data[x << 12 | z << 8 | y0]);
                            }
                        }
                    }
                }
                for (int i = 0; i < 256; i++)
                {
                    chunk.biome[i] = chunkPrimer.biome[i];
                }
                //chunk.Light.SetLightBlocks(chunkPrimer.arrayLightBlocks.ToArray());
                long le1 = stopwatch.ElapsedTicks;
                chunk.Light.GenerateHeightMapSky();
                long le2 = stopwatch.ElapsedTicks;
                //world.Log.Log("ChunkGen[{1}]: {0:0.00} ms hs: {2:0.00} ms",
                //    le1 / (float)MvkStatic.TimerFrequency, chunk.Position, (le2 - le1) / (float)MvkStatic.TimerFrequency);
                return chunk;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        public void Populate(ChunkBase chunk)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            BiomeBase biome;
            ChunkBase chunkSpawn;

            // Декорация областей которые могу выйти за 1 чанк
            int i, j;
            for (j = 0; j < 9; j++)
            {
                chunkSpawn = World.GetChunk(MvkStatic.AreaOne9[j] + chunk.Position);
                i = j + 1;
                if ((chunkSpawn.CountPopulated & (1 << j)) == 0)
                {
                    chunkSpawn.CountPopulated += (1 << j);
                }
                biome = World.ChunkPrServ.ChunkGenerate.biomes[(int)chunkSpawn.biome[136]];
                biome.Decorator.GenDecorationsArea(World, this, chunk, chunkSpawn);
            }

            // Декорация в одном столбце или 1 блок
            // Выбираем биом который в середине чанка
            biome = biomes[(int)chunk.biome[136]];
            biome.Decorator.GenDecorations(World, this, chunk);

            long le1 = stopwatch.ElapsedTicks;
            //World.Log.Log("Populate[{1}]: {0:0.00} ms",
            //    le1 / (float)MvkStatic.TimerFrequency, chunk.Position);

        }

        /// <summary>
        /// Генерация пещер
        /// </summary>
        //private void Cave(ChunkBase chunk, int yMax)
        //{
        //    //Stopwatch stopwatch = new Stopwatch();
        //    //stopwatch.Start();

        //    int count = 0;
        //    float[] noise = new float[4096];
        //    for (int y0 = 0; y0 <= yMax; y0++)
        //    {
        //        cave.GenerateNoise3d(noise, chunk.Position.x * 16, y0 * 16, chunk.Position.y * 16, 16, 16, 16, .05f, .05f, .05f);
        //        count = 0;
        //        for (int x = 0; x < 16; x++)
        //        {
        //            for (int z = 0; z < 16; z++)
        //            {
        //                for (int y = 0; y < 16; y++)
        //                {
        //                    if (noise[count] < -1f)
        //                    {
        //                        vec3i pos = new vec3i(x, y0 << 4 | y, z);
        //                        int y2 = y0 << 4 | y;
        //                        Block.EnumBlock enumBlock = chunk.GetBlockState(x, y2, z).GetBlock().EBlock;
        //                        if (enumBlock != Block.EnumBlock.Air && enumBlock != Block.EnumBlock.Stone
        //                            && enumBlock != Block.EnumBlock.Water)
        //                        {
        //                            chunk.SetEBlock(pos, Block.EnumBlock.Air);
        //                        }
        //                    }
        //                    count++;
        //                }
        //            }
        //        }
        //    }
        //    //long le1 = stopwatch.ElapsedTicks;
        //    //worldServer.Log.Log("Cave t:{0:0.00}ms",
        //    //        le1 / (float)MvkStatic.TimerFrequency);

        //}

        ///// <summary>
        ///// Определить биом по двум шумам
        ///// </summary>
        ///// <param name="h">высота -1..+1</param>
        ///// <param name="r">река -1..+1</param>
        ///// <param name="t">температура -1..+1</param>
        ///// <param name="w">влажность -1..+1</param>
        //private BiomeData DefineBiome(float h, float r, float t, float w)
        //{
        //    EnumBiome biome;

        //    // -1 глубина, 0 уровень моря, 1 максимальная высота
        //    float height = h <= -2f ? (h + .2f) * 1.25f : (h + .2f) * .833f;
        //    // для реки определения центра 1 .. 0 .. 1
        //    float river = 0;
        //    bool br = false;

        //    if (t < -.3f)
        //    {
        //        // высоко
        //        if (w < -.2f) biome = EnumBiome.Plain;
        //        else biome = EnumBiome.ConiferousForest;
        //    }
        //    else if (t < .225f)
        //    {
        //        if (w < .1f) biome = EnumBiome.Plain;
        //        else if (w < .4f) biome = EnumBiome.MixedForest;
        //        else biome = EnumBiome.Swamp;
        //    }
        //    else if (t < .325f)
        //    {
        //        river = Mth.Abs(t - .275f) * 20f;
        //        br = true;
        //        biome = EnumBiome.River;
        //    }
        //    else
        //    {
        //        if (w < .3f) biome = EnumBiome.Desert;
        //        else if (w < .5f) biome = EnumBiome.Tropics;
        //        else biome = EnumBiome.MixedForest;
        //    }

        //    bool checkSwamp = false;
        //    // Перепроверка болота
        //    if (biome == EnumBiome.Swamp)
        //    {
        //        if (h < -.3f || h > -.07f)
        //        {
        //            biome = EnumBiome.MixedForest; // лес смеш
        //            checkSwamp = true;
        //        }
        //    }

        //    if (!checkSwamp && h <= -.2f)
        //    {
        //        biome = h > -.25f && biome == EnumBiome.Plain ? EnumBiome.Beach : EnumBiome.Sea;
        //        if (biome == EnumBiome.Sea) br = false;
        //    }
        //    else if (h < -.17f && biome == EnumBiome.Plain)
        //    {
        //        biome = EnumBiome.Beach;
        //    }
        //    else if (h > .2f)
        //    {
        //        if (biome == EnumBiome.ConiferousForest || biome == EnumBiome.Swamp) biome = EnumBiome.BirchForest;
        //        if (h > .3f && biome == EnumBiome.MixedForest) biome = EnumBiome.BirchForest;
        //        if (h > .4f)
        //        {
        //            if (biome == EnumBiome.Desert || biome == EnumBiome.Tropics) biome = EnumBiome.MountainsDesert;
        //            else biome = EnumBiome.Mountains;
        //            br = false;
        //        }
        //    }

        //    if (biome != EnumBiome.Sea && biome != EnumBiome.Desert && biome != EnumBiome.Swamp && h < .43f)
        //    {
        //        float s = 0;
        //        bool b = false;
        //        if (r < -0.0825f && r > -.1325f)
        //        {
        //            s = Mth.Abs(r + .1075f) * 40f;
        //            b = true;
        //        }
        //        else if (r > 0.1725f && r < .2225f)
        //        {
        //            s = Mth.Abs(r - .1975f) * 40f;
        //            b = true;
        //        }
        //        if (b)
        //        {
        //            if (br)
        //            {
        //                river = river > s ? s : river;
        //            }
        //            else
        //            {
        //                river = s;
        //                br = true;
        //            }
        //        }
        //    }

        //    if (br)
        //    {
        //        // Если есть река делаем плавность
        //        // height -= ((1f + (glm.cos((sr) * glm.pi))) * .125f);
        //        biome = EnumBiome.River;
        //    }

        //    return new BiomeData() { biome = biome, height = height, river = river };
        //}

        /// <summary>
        /// Определить биом по двум шумам
        /// </summary>
        /// <param name="h">высота -1..+1</param>
        /// <param name="r">река -1..+1</param>
        /// <param name="t">температура -1..+1</param>
        /// <param name="w">влажность -1..+1</param>
        private BiomeData DefineBiome(float h, float r, float t, float w)
        {
            // для реки определения центра 1 .. 0 .. 1
            float river = 1;

            // Алгоритм для опускания рельефа где протекает река, чуть шире
            if (r < -0.0675f && r > -.1475f)
            {
                river = Mth.Abs(r + .1075f) * 25f;
            }
            else if (r > 0.1575f && r < .2375f)
            {
                river = Mth.Abs(r - .1975f) * 25f;
            }
            if (t >= .2f && t < .35f && w < .55f)
            {
                float s = Mth.Abs(t - .275f) * 10f;
                river = river > s ? s : river;
            }
            if (river < 1)
            {
                if (h > 0) h = h - (glm.cos(river * glm.pi) * .5f + .5f) * h;
            }
            river = 1;

            // -1 глубина, 0 уровень моря, 1 максимальная высота
            float height = h <= -2f ? (h + .2f) * 1.25f : (h + .2f) * .833f;
            // Уменьшаем амплитуду рельефа в 4 раза, для плавного перехода между биомами, 
            // каждый биом будет корректировать высоту
            height *= .25f;

            // Определяем биомы

            // Биом по умолчанию равнина
            EnumBiome biome = EnumBiome.Plain;

            bool beach = h > -.2f && h <= -.17f;

            // Делим горы и море
            if (h <= -.2f)
            {
                biome = EnumBiome.Sea;
            }
            else if (h > .4f)
            {
                biome = (t >= .225f && w < .5f) ? EnumBiome.MountainsDesert : EnumBiome.Mountains;
            }

            // пробуем сделать реку
            if (biome == EnumBiome.Plain)
            {
                if (r < -0.0825f && r > -.1325f)
                {
                    river = Mth.Abs(r + .1075f) * 40f;
                }
                else if (r > 0.1725f && r < .2225f)
                {
                    river = Mth.Abs(r - .1975f) * 40f;
                }
                if (t >= .225f && t < .325f && w < .5f)
                {
                    float s = Mth.Abs(t - .275f) * 20f;
                    river = river > s ? s : river;
                }
                if (river < 1)
                {
                    if (h - (1f - river) * .2f < .4f)
                    {
                        biome = EnumBiome.River;
                    }
                }
            }

            if (biome == EnumBiome.Plain)
            {
                if (t < -.3f)
                {
                    // Холодно и влажно
                    if (w > -.2f) biome = EnumBiome.ConiferousForest;
                }
                else if (t < .225f)
                {
                    // Тепло
                    if (w > .4f) biome = h < -.1f ? EnumBiome.Swamp : EnumBiome.BirchForest; // влажно
                    else if (w > .1f) biome = EnumBiome.MixedForest;
                }
                else
                {
                    // Жарко
                    if (w < .3f) biome = EnumBiome.Desert; // сухо
                    else if (w < .5f) biome = EnumBiome.Tropics;
                    else biome = EnumBiome.MixedForest; // влажно
                }
            }

            // Пляж толко возле ровнины и моря
            if (beach && (biome == EnumBiome.Plain || biome == EnumBiome.Sea)) biome = EnumBiome.Beach;

            return new BiomeData() { biome = biome, height = height, river = river };
        }

        /// <summary>
        /// Структура данных определения биома и высот с рекой
        /// </summary>
        private struct BiomeData
        {
            /// <summary>
            /// Биом
            /// </summary>
            public EnumBiome biome;
            /// <summary>
            /// Высота -1..0..1
            /// </summary>
            public float height;
            /// <summary>
            /// Определение центра реки 1..0..1
            /// </summary>
            public float river;
        }

    }
}
