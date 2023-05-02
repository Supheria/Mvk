using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Chunk;
using System;
using System.Diagnostics;

namespace MvkServer.World.Gen
{
    public class ChunkProviderGenerateOld : ChunkProviderGenerateBase
    {
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
        ///// <summary>
        ///// Шум пещер
        ///// </summary>
        //private readonly NoiseGeneratorPerlin noiseCave;
        
        // Шум речных пещер, из двух частей
        private readonly NoiseGeneratorPerlin noiseCave1;
        private readonly NoiseGeneratorPerlin noiseCaveHeight1;
        private readonly NoiseGeneratorPerlin noiseCave2;
        private readonly NoiseGeneratorPerlin noiseCaveHeight2;
        private readonly NoiseGeneratorPerlin noiseCave3;
        private readonly NoiseGeneratorPerlin noiseCaveHeight3;

        private readonly float[] heightNoise = new float[256];
        private readonly float[] wetnessNoise = new float[256];
        private readonly float[] temperatureNoise = new float[256];
        private readonly float[] riversNoise = new float[256];
        private readonly float[] caveRiversNoise = new float[256];
        private readonly float[] caveHeightNoise = new float[256];
        private readonly float[] caveNoise2 = new float[256];
        private readonly float[] caveHeightNoise2 = new float[256];
        private readonly float[] caveNoise3 = new float[256];
        private readonly float[] caveHeightNoise3 = new float[256];

        /// <summary>
        /// Шум облостей
        /// </summary>
        public NoiseGeneratorPerlin noiseArea;

        public ChunkProviderGenerateOld(WorldServer worldIn) : base(worldIn)
        {

            heightBiome = new NoiseGeneratorPerlin(new Rand(Seed), 8);
            riversBiome = new NoiseGeneratorPerlin(new Rand(Seed + 6), 8);
            wetnessBiome = new NoiseGeneratorPerlin(new Rand(Seed + 8), 4); // 8
            temperatureBiome = new NoiseGeneratorPerlin(new Rand(Seed + 4), 4); // 8
            noiseCave1 = new NoiseGeneratorPerlin(new Rand(Seed + 7), 4);
            noiseCaveHeight1 = new NoiseGeneratorPerlin(new Rand(Seed + 5), 4);
            noiseCave2 = new NoiseGeneratorPerlin(new Rand(Seed + 9), 4);
            noiseCaveHeight2 = new NoiseGeneratorPerlin(new Rand(Seed + 11), 4);
            noiseCave3 = new NoiseGeneratorPerlin(new Rand(Seed + 12), 4);
            noiseCaveHeight3 = new NoiseGeneratorPerlin(new Rand(Seed + 13), 4);
            //noiseCave = new NoiseGeneratorPerlin(new Rand(Seed + 2), 2);
            noiseArea = new NoiseGeneratorPerlin(new Rand(Seed + 2), 4);

            for (int i = 0; i < biomes.Length; i++)
            {
                biomes[i].InitDecorator(false);
            }
        }

        public override void Populate(ChunkBase chunk)
        {
            BiomeBase biome;
            ChunkBase chunkSpawn;

            // Декорация областей которые могу выйти за 1 чанк
            for (int i = 0; i < 9; i++)
            {
                chunkSpawn = World.GetChunk(MvkStatic.AreaOne9[i] + chunk.Position);
                biome = biomes[(int)chunkSpawn.biome[136]];
                biome.Decorator.GenDecorationsArea(World, this, chunk, chunkSpawn);
            }

            // Декорация в одном столбце или 1 блок
            // Выбираем биом который в середине чанка
            biome = biomes[(int)chunk.biome[136]];
            biome.Decorator.GenDecorations(World, this, chunk);
        }

        public override void GenerateChunk(ChunkBase chunk)
        {
            try
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                chunkPrimer.Clear();
                //for (int i = 0; i < biomesCount.Length; i++) biomesCount[i] = 0;
                int xbc = chunk.Position.x << 4;
                int zbc = chunk.Position.y << 4;

                biomeBase.Init(chunkPrimer, xbc, zbc);
                biomeBase.Down();

                // Пакет для биомов и высот с рекой
                heightBiome.GenerateNoise2d(heightNoise, xbc, zbc, 16, 16, .2f, .2f);
                riversBiome.GenerateNoise2d(riversNoise, xbc, zbc, 16, 16, .1f, .1f);
                wetnessBiome.GenerateNoise2d(wetnessNoise, xbc, zbc, 16, 16, .0125f, .0125f); //*4
                temperatureBiome.GenerateNoise2d(temperatureNoise, xbc, zbc, 16, 16, .0125f, .0125f); //*4

                // доп шумы
                noiseArea.GenerateNoise2d(AreaNoise, xbc, zbc, 16, 16, .4f, .4f);
                //  шумы речных пещер
                noiseCave1.GenerateNoise2d(caveRiversNoise, xbc, zbc, 16, 16, .05f, .05f);
                noiseCaveHeight1.GenerateNoise2d(caveHeightNoise, xbc, zbc, 16, 16, .025f, .025f);
                noiseCave2.GenerateNoise2d(caveNoise2, xbc, zbc, 16, 16, .05f, .05f);
                noiseCaveHeight2.GenerateNoise2d(caveHeightNoise2, xbc, zbc, 16, 16, .025f, .025f);
                noiseCave3.GenerateNoise2d(caveNoise3, xbc, zbc, 16, 16, .05f, .05f);
                noiseCaveHeight3.GenerateNoise2d(caveHeightNoise3, xbc, zbc, 16, 16, .025f, .025f);

                BiomeData biomeData;
                BiomeBase biome = biomes[2];
                int i, x, y, z, idBiome;
                EnumBiome enumBiome;
                int count = 0;
                float h, r, t, w;
                
                for (x = 0; x < 16; x++)
                {
                    for (z = 0; z < 16; z++)
                    {
                        h = heightNoise[count] / 132f;
                        r = riversNoise[count] / 132f;
                        t = temperatureNoise[count] / 8.3f;
                        w = wetnessNoise[count] / 8.3f;
                        biomeData = DefineBiome(h, r, t, w);
                        i = x << 4 | z;
                        chunk.biome[i] = chunkPrimer.biome[i] = biomeData.biome;
                    }
                }
                

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
                        enumBiome = biomeData.biome;
                        idBiome = (int)enumBiome;
                        biome = biomes[idBiome];
                        biome.Init(chunkPrimer, xbc, zbc);
                        biome.Column(x, z, biomeData.height, biomeData.river);

                        // Пещенры 2д ввиде рек
                        ColumnCave2d(caveRiversNoise[count] / 8f, caveHeightNoise[count] / 8f, x, z, enumBiome,
                            .12f, .28f, 12.5f, 5f, 104f, BiomeBase.HEIGHT_CENTER_CAVE);
                        ColumnCave2d(caveNoise2[count] / 8f, caveHeightNoise2[count] / 8f, x, z, enumBiome,
                            .13f, .27f, 14.3f, 9f, 128f, BiomeBase.HEIGHT_CENTER_CAVE);
                        ColumnCave2d(caveNoise3[count] / 8f, caveHeightNoise3[count] / 8f, x, z, enumBiome,
                           .10f, .30f, 10f, 12f, 16f, 16);

                        count++;
                    }
                }

                // Пещеры 3д
              //  Cave(chunk);

                ChunkStorage chunkStorage;
                int yc, ycb, y0;
                for (yc = 0; yc < ChunkBase.COUNT_HEIGHT; yc++)
                {
                    ycb = yc << 4;
                    chunkStorage = chunk.StorageArrays[yc];
                    for (y = 0; y < 16; y++)
                    {
                        y0 = ycb | y;
                        if (y0 <= ChunkBase.MAX_HEIGHT_BLOCK)
                        {
                            for (x = 0; x < 16; x++)
                            {
                                for (z = 0; z < 16; z++)
                                {
                                    chunkStorage.SetData(y << 8 | z << 4 | x, chunkPrimer.id[x << 12 | z << 8 | y0]);
                                }
                            }
                        }
                    }
                }
                for (i = 0; i < 256; i++)
                {
                    chunk.biome[i] = chunkPrimer.biome[i];
                }
                chunk.Light.SetLightBlocks(chunkPrimer.arrayLightBlocks.ToArray());
                chunk.Light.GenerateHeightMap();
                chunk.InitHeightMapGen();
                //World.Log.Log("ChunkGen[{1}]: {0:0.00} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, chunk.Position);
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Генерация пещер
        /// </summary>
        //private void Cave(ChunkBase chunk)
        //{
        //    int yMax = chunkPrimer.GetHeightMapMax() >> 4;
        //    int count = 0;
        //    int x, y, z, x1, y1, x2, y0, index, y2, y3, h;
        //    ushort id;
        //    EnumBiome enumBiome;
        //    //enumBiome = chunkPrimer.biome[x << 4 | z];
        //    for (y0 = 0; y0 <= yMax; y0++)
        //    {
        //        noiseCave.GenerateNoise3d(caveNoise, chunk.Position.x * 8, y0 * 8, chunk.Position.y * 16, 8, 8, 16, .1f, .2f, .05f);
        //        count = 0;
        //        for (x = 0; x < 8; x++)
        //        {
        //            for (z = 0; z < 16; z++)
        //            {
        //                for (y = 0; y < 8; y++)
        //                {
        //                    if (caveNoise[count] < -1f)
        //                    {
        //                        for (x1 = 0; x1 < 2; x1++)
        //                        {
        //                            x2 = x * 2 + x1;
        //                            enumBiome = chunkPrimer.biome[x2 << 4 | z];
        //                            h = (enumBiome == EnumBiome.River || enumBiome == EnumBiome.Sea || enumBiome == EnumBiome.Swamp) 
        //                                ? chunkPrimer.heightMap[x2 << 4 | z] : 255;
        //                            h -= 4;
        //                            if (h > 96) h = 255;
        //                            for (y1 = 0; y1 < 2; y1++)
        //                            {
        //                                y2 = y * 2 + y1;
        //                                if ((y0 == 0 && y2 > 3) || y0 > 0)
        //                                {
        //                                    y3 = y0 << 4 | y2;
        //                                    if (y3 < h)
        //                                    {
        //                                        index = x2 << 12 | z << 8 | y3;
        //                                        id = chunkPrimer.data[index];
        //                                        if (id == 3 || id == 9 || id == 10 || id == 7
        //                                            || (id == 8 && chunkPrimer.data[index + 1] != 13))
        //                                        {
        //                                            //if (y3 < 12)
        //                                            //{
        //                                            //    chunkPrimer.data[index] = 15; // лава
        //                                            //    chunkPrimer.arrayLightBlocks.Add(new vec3i(x2, y3, z));
        //                                            //}
        //                                            //else chunkPrimer.data[index] = 0; // воздух
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    count++;
        //                }
        //            }
        //        }
        //    }
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

            float levelSea = .18f;

            // Алгоритм для опускания рельефа где протекает река, чуть шире
            if (r < -0.0675f && r > -.1475f)
            {
                river = Mth.Abs(r + .1075f) * 25f;
            }
            else if (r > 0.1575f && r < .2375f)
            {
                river = Mth.Abs(r - .1975f) * 25f;
            }
            if (t >= .2f && t < .35f)// && w < .55f)
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

            bool beach = h > -levelSea && h <= -.17f;
            bool seaS​shore = false;

            // Делим горы и море
            if (h <= -levelSea)
            {
                biome = EnumBiome.Sea;
                if (h > -.22f) seaSshore = true;
            }
            else if (h > .4f)
            {
                biome = (t >= .225f && w < .5f) ? EnumBiome.MountainsDesert : EnumBiome.Mountains;
            }

            // пробуем сделать реку
            if (biome == EnumBiome.Plain || seaS​shore)
            {
                if (r < -0.0825f && r > -.1325f)
                {
                    river = Mth.Abs(r + .1075f) * 40f;
                }
                else if (r > 0.1725f && r < .2225f)
                {
                    river = Mth.Abs(r - .1975f) * 40f;
                }
                if (t >= .225f && t < .325f)// && w < .5f)
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

            if (biome == EnumBiome.Plain || seaS​shore)
            {
                if (t < -.3f && !seaS​shore)
                {
                    // Холодно и влажно
                    if (w > -.2f) biome = EnumBiome.ConiferousForest;
                }
                else if (t < .225f)
                {
                    // Тепло
                    if (w > .4f)
                    {   // влажно
                        if (h < -.1f) biome = EnumBiome.Swamp;
                        else if (!seaSshore) biome = EnumBiome.BirchForest; 
                    }
                    else if (w > .1f && !seaS​shore) biome = EnumBiome.MixedForest;
                }
                else if(!seaSshore)
                {
                    // Жарко
                    if (w < .3f) biome = EnumBiome.Desert; // сухо
                    else if (w < .5f) biome = EnumBiome.Tropics;
                    else if (!seaS​shore) biome = EnumBiome.MixedForest; // влажно
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
