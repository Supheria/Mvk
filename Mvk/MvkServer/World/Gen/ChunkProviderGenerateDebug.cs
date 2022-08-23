//using MvkServer.Gen;
//using MvkServer.Glm;
//using MvkServer.Util;
//using System.Collections.Generic;
//using System.Diagnostics;

//namespace MvkServer.World.Gen
//{
//    public class ChunkProviderGenerateDebug
//    {
//        /// <summary>
//        /// Сылка на объект мира
//        /// </summary>
//        private readonly WorldServer world;
//        /// <summary>
//        /// Шум высот биомов
//        /// </summary>
//        private NoiseGeneratorPerlin heightBiome;
//        /// <summary>
//        /// Шум влажности биомов
//        /// </summary>
//        private NoiseGeneratorPerlin wetnessBiome;
//        /// <summary>
//        /// Шум пещер
//        /// </summary>
//        private NoiseGeneratorPerlin cave;
//        /// <summary>
//        /// Шум облостей
//        /// </summary>
//        private NoiseGeneratorPerlin area;
//        /// <summary>
//        /// Шум нижнего слоя
//        /// </summary>
//        private NoiseGeneratorPerlin down;

//        private readonly float[] heightNoise = new float[256];
//        private readonly float[] wetnessNoise = new float[256];
//        private readonly float[] grassNoise = new float[256];

//        public ChunkProviderGenerateDebug(WorldServer worldIn)
//        {
//            world = worldIn;
//            int seed = world.Seed;
//            heightBiome = new NoiseGeneratorPerlin(new Rand(seed), 8);
//            wetnessBiome = new NoiseGeneratorPerlin(new Rand(seed + 8), 8);
//            cave = new NoiseGeneratorPerlin(new Rand(seed + 2), 2);
//            down = new NoiseGeneratorPerlin(new Rand(seed), 1);
//            area = new NoiseGeneratorPerlin(new Rand(seed + 2), 1);
//        }

//        public void GenerateChunk(ChunkBase chunk)
//        {
//            vec2i pos = chunk.Position;
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            //ChunkBase chunk = new ChunkBase(world, pos);
//            float scale = 0.2f;
//            heightBiome.GenerateNoise2d(heightNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
//            wetnessBiome.GenerateNoise2d(wetnessNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
//            scale = .5f;
//            down.GenerateNoise2d(grassNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);

//            int count = 0;
//            int stolb = 0;
//            int yMax = 0;
//            for (int x = 0; x < 16; x++)
//            {
//                for (int z = 0; z < 16; z++)
//                {
//                    float h = heightNoise[count] / 132f;
//                    int y0 = (int)(-h * 64f) + 32;
//                    if (x == 7 && z == 7) stolb = y0;
//                    chunk.SetEBlock(new vec3i(x, 0, z), Block.EnumBlock.Stone);
//                    //if (y0 > 0)
//                    {
//                        bool stop = false;
//                        for (int y = 1; y < 256; y++)
//                        {
//                            if (y < y0)
//                            {
//                                chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Dirt);
//                            }
//                            else
//                            {
//                                stop = true;
//                                if (y <= 16)
//                                {
//                                    chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Water);
//                                }
//                                else
//                                {
//                                    chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Turf);//, chunk.World.Rand.Next(0, 4));
//                                    if (grassNoise[count] > .61f)
//                                    {
//                                          chunk.SetEBlock(new vec3i(x, y + 1, z), Block.EnumBlock.Brol);
//                                    }
//                                    else if (grassNoise[count] > .1f)
//                                    {
//                                        chunk.SetEBlock(new vec3i(x, y + 1, z), Block.EnumBlock.TallGrass);//, chunk.World.Rand.Next(0, 5));
//                                    }
//                                }
//                                if (y > yMax) yMax = y;
//                                //break;
//                            }
//                            if (y >= 16 && stop)
//                            {
//                                break;
//                            }
//                        }
//                    }
//                    count++;
//                }
//            }
//            if (pos.x == -1 && pos.y == -1)
//            {
//                for (int y = 1; y <= 16; y++)
//                {
//                    chunk.SetEBlock(new vec3i(3, y + stolb, 7), Block.EnumBlock.Dirt);
//                    chunk.SetEBlock(new vec3i(7, y + stolb, 5), Block.EnumBlock.Glass);
//                    // chunk.SetEBlock(new vec3i(7, y + stolb, 9), Block.EnumBlock.GlassWhite);
//                    chunk.SetEBlock(new vec3i(11, y + stolb, 7), Block.EnumBlock.Lava);
//                    chunk.SetEBlock(new vec3i(12, y + stolb, 7), Block.EnumBlock.LogOak);
//                }
//                for (int y = 16; y <= 216; y++)
//                {
//                    chunk.SetEBlock(new vec3i(12, y + stolb, 7), Block.EnumBlock.LogOak);
//                    //chunk.SetEBlock(new vec3i(3, y + stolb, 7), Block.EnumBlock.Dirt);
//                }

//                //chunk.SetEBlock(new vec3i(5, 2 + stolb, 5), Block.EnumBlock.Brol);

//            }

//            if (pos.x == -3 && pos.y == 0)
//            {
//                //for (int x = 1; x <= 5; x++)
//                //    for (int y = 3; y <= 8; y++)
//                //    {
//                //        chunk.SetEBlock(new vec3i(x, stolb + y, 5), Block.EnumBlock.Dirt);
//                //    }
//                for (int x = 1; x <= 3; x++) for (int y = 6; y <= 8; y++) for (int z = 1; z <= 3; z++)
//                        {
//                            chunk.SetEBlock(new vec3i(x, stolb + y, z), Block.EnumBlock.Dirt);
//                        }
//                chunk.SetEBlock(new vec3i(0, 30 + stolb, 0), Block.EnumBlock.Dirt);
//                chunk.SetEBlock(new vec3i(8, 10 + stolb, 1), Block.EnumBlock.Dirt);
//                for (int x = 7; x <= 9; x++) for (int z = 1; z <= 3; z++)
//                    {
//                        chunk.SetEBlock(new vec3i(x, stolb + 16, z), Block.EnumBlock.Water);
//                    }
//                //for (int x = 0; x < 16; x++)
//                //{
//                //    for (int z = 0; z < 16; z++)
//                //    {
//                //        int y = 255;
//                //        while(y > 0)
//                //        {
//                //            if (chunk.GetEBlock(x, y, z) == Block.EnumBlock.Air)
//                //            {
//                //                int yc = y >> 4;
//                //                int yb = y & 15;
//                //                chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Sky, 15);
//                //                //chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Block, 15);
//                //                y--;
//                //            }
//                //            else
//                //            {
//                //                y = -1;
//                //            }
//                //        }
//                //    }
//                //}
//            }
//            //if (pos.x == -3 && pos.y == 1)
//            //{
//            //    for (int x = 0; x < 16; x++)
//            //    {
//            //        for (int z = 0; z < 16; z++)
//            //        {
//            //            int y = 255;
//            //            while (y > 0)
//            //            {
//            //                if (chunk.GetEBlock(x, y, z) == Block.EnumBlock.Air)
//            //                {
//            //                    int yc = y >> 4;
//            //                    int yb = y & 15;
//            //                    //chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Sky, 15);
//            //                    chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Block, 10);
//            //                    y--;
//            //                }
//            //                else
//            //                {
//            //                    y = -1;
//            //                }
//            //            }
//            //        }
//            //    }
//            //}
//            Cave(chunk, yMax >> 4);

//            List<vec3i> list = new List<vec3i>();

//            for (int x = 0; x < 16; x++)
//            {
//                for (int z = 0; z < 16; z++)
//                {
//                    for (int y = 0; y <= ChunkBase.COUNT_HEIGHT_BLOCK; y++)
//                    {
//                        if (chunk.GetBlockState(x, y, z).GetBlock().LightValue > 0)
//                        {
//                            list.Add(new vec3i(x, y, z));
//                        }
//                    }
//                }
//            }
//            chunk.Light.SetLightBlocks(list.ToArray());
//            chunk.Light.GenerateHeightMapSky();
//            long le1 = stopwatch.ElapsedTicks;
//             //world.Log.Log("ChunkGen: {0:0.00} ms", le1 / (float)MvkStatic.TimerFrequency);
//        }

//        /// <summary>
//        /// Генерация пещер
//        /// </summary>
//        private void Cave(ChunkBase chunk, int yMax)
//        {
//            //Stopwatch stopwatch = new Stopwatch();
//            //stopwatch.Start();

//            int count = 0;
//            float[] noise = new float[4096];
//            for (int y0 = 0; y0 <= yMax; y0++)
//            {
//                cave.GenerateNoise3d(noise, chunk.Position.x * 16, y0 * 16, chunk.Position.y * 16, 16, 16, 16, .05f, .05f, .05f);
//                count = 0;
//                for (int x = 0; x < 16; x++)
//                {
//                    for (int z = 0; z < 16; z++)
//                    {
//                        for (int y = 0; y < 16; y++)
//                        {
//                            if (noise[count] < -1f)
//                            {
//                                vec3i pos = new vec3i(x, y0 << 4 | y, z);
//                                int y2 = y0 << 4 | y;
//                                Block.EnumBlock enumBlock = chunk.GetBlockState(x, y2, z).GetBlock().EBlock;
//                                if (enumBlock != Block.EnumBlock.Air && enumBlock != Block.EnumBlock.Stone
//                                    && enumBlock != Block.EnumBlock.Water)
//                                {
//                                    chunk.SetEBlock(pos, Block.EnumBlock.Air);
//                                }
//                            }
//                            count++;
//                        }
//                    }
//                }
//            }
//            //long le1 = stopwatch.ElapsedTicks;
//            //worldServer.Log.Log("Cave t:{0:0.00}ms",
//            //        le1 / (float)MvkStatic.TimerFrequency);

//        }

//    }
//}
