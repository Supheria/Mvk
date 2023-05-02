using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Chunk;
using MvkServer.World.Gen.Layer;
using System;
using System.Diagnostics;

namespace MvkServer.World.Gen
{
    public class ChunkProviderGenerateRobinson : ChunkProviderGenerateBase
    {
        /// <summary>
        /// Объект генерации слоёв биомов
        /// </summary>
        private readonly GenLayer genLayerBiome;
        /// <summary>
        /// Объект генерации слоёв высот от биомов
        /// </summary>
        private readonly GenLayer genLayerHeight;

        /// <summary>
        /// Шум высот биомов
        /// </summary>
        private readonly NoiseGeneratorPerlin heightBiome;

        // Шум речных пещер, из двух частей
        private readonly NoiseGeneratorPerlin noiseCave1;
        private readonly NoiseGeneratorPerlin noiseCaveHeight1;
        //private readonly NoiseGeneratorPerlin noiseCave2;
        //private readonly NoiseGeneratorPerlin noiseCaveHeight2;
        private readonly NoiseGeneratorPerlin noiseCave3;
        private readonly NoiseGeneratorPerlin noiseCaveHeight3;

        private readonly float[] heightNoise = new float[256];
        private readonly float[] caveRiversNoise = new float[256];
        private readonly float[] caveHeightNoise = new float[256];
        //private readonly float[] caveNoise2 = new float[256];
        //private readonly float[] caveHeightNoise2 = new float[256];
        private readonly float[] caveNoise3 = new float[256];
        private readonly float[] caveHeightNoise3 = new float[256];

        /// <summary>
        /// Шум облостей
        /// </summary>
        public NoiseGeneratorPerlin noiseArea;

        public ChunkProviderGenerateRobinson(WorldServer worldIn) : base(worldIn)
        {
            GenLayer[] gens = GenLayer.BeginLayerBiome(Seed);
            genLayerBiome = gens[0];
            genLayerHeight = gens[1];

            heightBiome = new NoiseGeneratorPerlin(new Rand(Seed), 8);
            noiseCave1 = new NoiseGeneratorPerlin(new Rand(Seed + 7), 4);
            noiseCaveHeight1 = new NoiseGeneratorPerlin(new Rand(Seed + 5), 4);
            //noiseCave2 = new NoiseGeneratorPerlin(new Rand(Seed + 9), 4);
            //noiseCaveHeight2 = new NoiseGeneratorPerlin(new Rand(Seed + 11), 4);
            noiseCave3 = new NoiseGeneratorPerlin(new Rand(Seed + 12), 4);
            noiseCaveHeight3 = new NoiseGeneratorPerlin(new Rand(Seed + 13), 4);
            noiseArea = new NoiseGeneratorPerlin(new Rand(Seed + 2), 4);

            for (int i = 0; i < biomes.Length; i++)
            {
                biomes[i].InitDecorator(true);
            }
        }

        public override void Populate(ChunkBase chunk)
        {
            //return;
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
                int xbc = chunk.Position.x << 4;
                int zbc = chunk.Position.y << 4;

                biomeBase.Init(chunkPrimer, xbc, zbc);
                biomeBase.Down();

                // Пакет для биомов и высот с рекой
                heightBiome.GenerateNoise2d(heightNoise, xbc, zbc, 16, 16, 1.2f, 1.2f);

                // доп шумы
                noiseArea.GenerateNoise2d(AreaNoise, xbc, zbc, 16, 16, .4f, .4f);
                //  шумы речных пещер
                noiseCave1.GenerateNoise2d(caveRiversNoise, xbc, zbc, 16, 16, .05f, .05f);
                noiseCaveHeight1.GenerateNoise2d(caveHeightNoise, xbc, zbc, 16, 16, .025f, .025f);
                noiseCave3.GenerateNoise2d(caveNoise3, xbc, zbc, 16, 16, .05f, .05f);
                noiseCaveHeight3.GenerateNoise2d(caveHeightNoise3, xbc, zbc, 16, 16, .025f, .025f);

                BiomeBase biome = biomes[2];
                int x, y, z, idBiome, idx, level;
                EnumBiome enumBiome;
                int count = 0;

                int[] arHeight = genLayerHeight.GetInts(xbc, zbc, 16, 16);
                int[] arBiome = genLayerBiome.GetInts(xbc, zbc, 16, 16);

                // Пробегаемся по столбам
                for (x = 0; x < 16; x++)
                {
                    for (z = 0; z < 16; z++)
                    {
                        idx = z << 4 | x;
                        idBiome = arBiome[idx];
                        enumBiome = chunk.biome[count] = chunkPrimer.biome[count] = (EnumBiome)idBiome;
                        biome = biomes[idBiome];
                        biome.Init(chunkPrimer, xbc, zbc);

                        level = biome.ColumnRobinson(x, z,
                            arHeight[idx], 
                            heightNoise[count] / 132f, 
                            AreaNoise[count] / 8.3f
                        );
                        // biome.ViewDebugBiom(x, z);

                        // Пещенры 2д ввиде рек
                        if ((enumBiome == EnumBiome.Mountains || enumBiome == EnumBiome.MountainsDesert && level > 58))
                        {
                            // В горах и пустыных горах могут быть пещеры
                            ColumnCave2d(caveRiversNoise[count] / 8f, caveHeightNoise[count] / 8f, x, z, enumBiome,
                            .12f, .28f, 12.5f, 5f, 64f, 72);
                        }
                        if (enumBiome == EnumBiome.Mountains)
                        {
                            // В горах внизу может быть лава
                            ColumnCave2d(caveNoise3[count] / 8f, caveHeightNoise3[count] / 8f, x, z, enumBiome,
                               .10f, .30f, 10f, 12f, 12f, 16); // 16
                        }
                        count++;
                    }
                }

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
    }
}