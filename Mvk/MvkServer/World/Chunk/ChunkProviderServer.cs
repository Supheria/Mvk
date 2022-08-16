using MvkServer.Glm;
using MvkServer.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Объект сервер который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderServer : ChunkProvider
    {
        /// <summary>
        /// Список чанков которые надо выгрузить
        /// </summary>
        public ListMessy<vec2i> DroppedChunks { get; protected set; } = new ListMessy<vec2i>();
        /// <summary>
        /// Дополнительный список чанков которых надо проверить свет
        /// Массив большой, выборка быстрее MapListVec2i чем List или ListMessy
        /// </summary>
        public MapListVec2i GenAddChunks { get; private set; } = new MapListVec2i();
        /// <summary>
        /// Объект для генерации чанков
        /// </summary>
        public ChunkProviderGenerate ChunkGenerate { get; private set; }

        /// <summary>
        /// Сылка на объект серверного мира
        /// </summary>
        private readonly WorldServer worldServer;

        /// <summary>
        /// Двойной список для запроса загрузки или генерации чанка
        /// </summary>
        private DoubleList<ChunkBase> listLoadGenRequest = new DoubleList<ChunkBase>(20);
        /// <summary>
        /// Двойной список для ответа загрузки или генерации чанка
        /// </summary>
        private DoubleList<ChunkBase> listLoadGenResponse = new DoubleList<ChunkBase>(20);

        public ChunkProviderServer(WorldServer worldIn)
        {
            world = worldServer = worldIn;
            ChunkGenerate = new ChunkProviderGenerate(worldIn);
        }

        /// <summary>
        /// Два режима работы: если передано true, сохранить все чанки за один раз. Если передано false, сохраните до двух фрагментов.
        /// </summary>
        /// <returns>Возвращает true, если все фрагменты были сохранены</returns>
        public bool SaveChunks(bool all)
        {
            int count = 0;
            Dictionary<vec2i, ChunkBase>.ValueCollection chunks = chunkMapping.Values();
            foreach (ChunkBase chunk in chunks)
            {
                if (all)
                {
                    SaveChunkExtraData(chunk);
                }
                if (chunk.NeedsSaving())
                {
                    SaveChunkData(chunk);
                    chunk.SavedNotModified();
                    count++;
                    if (count == 24 && !all) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Сохранить чанк
        /// </summary>
        private void SaveChunkData(ChunkBase chunk)
        {
            //if (chunkLoader != null)
            //{
            //    try
            //    {
            //        chunk.SetLastSaveTime(world.GetTotalWorldTime());
            //        chunkLoader.SaveChunk(world, chunk);
            //    }
            //    catch (Exception ex)
            //    {
            //        worldServer.Log.Error("Не удалось сохранить чанк {0}\r\n{1}", ex.Message, ex.StackTrace);
            //    }
            //}
        }

        /// <summary>
        /// Сохранить дополнительные данные чанка
        /// </summary>
        private void SaveChunkExtraData(ChunkBase chunk)
        {
            //if (chunkLoader != null)
            //{
            //    try
            //    {
            //        chunkLoader.SaveExtraChunkData(world, chunk);
            //    }
            //    catch (Exception ex)
            //    {
            //        worldServer.Log.Error("Не удалось сохранить чанк {0}\r\n{1}", ex.Message, ex.StackTrace);
            //    }
            //}
        }

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        //public override ChunkBase GetChunk(vec2i pos)
        //{
        //    //2022-04-04 эмитация не загруженного чанка
        //    if (pos.x == 4 && pos.y == 0) return null;
        //    return base.GetChunk(pos);
        //}
        /// <summary>
        /// Загрузить чанк в лоадинге
        /// </summary>
        public ChunkBase LoadingChunk(vec2i pos)
        {
            ChunkBase chunk = GetChunk(pos);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (chunk == null)
            {
                chunk = new ChunkBase(world, pos);
                // Загружаем
                // chunk = LoadChunkFromFile(pos);
                //if (chunk == null)
                {
                    ChunkGenerate.GenerateChunk(chunk);
                }

                chunkMapping.Set(chunk);
                chunk.OnChunkLoad();
            }
            return chunk;
        }

        public int LoadGenRequestCount => listLoadGenRequest.CountForward;
        public void LoadGenRequestAdd(vec2i pos)
        {
            ChunkBase chunk = new ChunkBase(world, pos);
            chunkMapping.Set(chunk);
            listLoadGenRequest.Add(chunk);
        }

        /// <summary>
        /// Получить готовые загруженные или сгенерированные чанки с другово потока
        /// </summary>
        public void LoadGenResponse()
        {
            listLoadGenResponse.Step();
            int count = listLoadGenResponse.CountBackward;
            ChunkBase chunk;
            for (int i = 0; i < count; i++)
            {
                chunk = listLoadGenResponse.GetNext();
                chunk.OnChunkLoad();
            }
        }

        /// <summary>
        /// Загрузить загруженные или сгенерированные чанки в другово потока
        /// </summary>
        public void LoadGenRequest()
        {
            listLoadGenRequest.Step();
            int count = listLoadGenRequest.CountBackward;
            ChunkBase chunk = null;
            for (int i = 0; i < count; i++)
            {
                chunk = listLoadGenRequest.GetNext();
                // Загружаем
                // chunk = LoadChunkFromFile(pos);
                //if (chunk == null)
                {
                    ChunkGenerate.GenerateChunk(chunk);
                }
                listLoadGenResponse.Add(chunk);
            }
        }
                    /*
                /// <summary>
                /// Загрузить чанк
                /// </summary>
                public ChunkBase LoadChunk(vec2i pos)
                {
                    DroppedChunks.Remove(pos);
                    ChunkBase chunk = GetChunk(pos);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    if (chunk == null)
                    {

                        // Загружаем
                        // chunk = LoadChunkFromFile(pos);
                        if (chunk == null)
                        {
                            // чанка нет после загрузки, значит надо генерировать

                            // это пока временно
                            chunk = new ChunkBase(world, pos);
                            //chunk.ChunkLoadGen();

                            float[] heightNoise = new float[256];
                            float[] wetnessNoise = new float[256];
                            float[] grassNoise = new float[256];
                            float scale = 0.2f;
                            worldServer.Noise.HeightBiome.GenerateNoise2d(heightNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
                            worldServer.Noise.WetnessBiome.GenerateNoise2d(wetnessNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
                            scale = .5f;
                            worldServer.Noise.Down.GenerateNoise2d(grassNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);

                            int count = 0;
                            int stolb = 0;
                            int yMax = 0;
                            for (int x = 0; x < 16; x++)
                            {
                                for (int z = 0; z < 16; z++)
                                {
                                    float h = heightNoise[count] / 132f;
                                    int y0 = (int)(-h * 64f) + 32;
                                    if (x == 7 && z == 7) stolb = y0;
                                    chunk.SetEBlock(new vec3i(x, 0, z), Block.EnumBlock.Stone);
                                    //if (y0 > 0)
                                    {
                                        bool stop = false;
                                        for (int y = 1; y < 256; y++)
                                        {
                                            if (y < y0)
                                            {
                                                chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Dirt);
                                            }
                                            else
                                            {
                                                stop = true;
                                                if (y <= 16)
                                                {
                                                    chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Water);
                                                }
                                                else
                                                {
                                                    chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Turf);//, chunk.World.Rand.Next(0, 4));
                                                    if (grassNoise[count] > .61f)
                                                    {
                                                      //  chunk.SetEBlock(new vec3i(x, y + 1, z), Block.EnumBlock.Brol);
                                                    }
                                                    else if (grassNoise[count] > .1f)
                                                    {
                                                        chunk.SetEBlock(new vec3i(x, y + 1, z), Block.EnumBlock.TallGrass);//, chunk.World.Rand.Next(0, 5));
                                                    }
                                                }
                                                if (y > yMax) yMax = y;
                                                //break;
                                            }
                                            if (y >= 16 && stop)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    count++;
                                }
                            }

                            if (pos.x == -1 && pos.y == -1)
                            {
                                for (int y = 1; y <= 16; y++)
                                {
                                    chunk.SetEBlock(new vec3i(3, y + stolb, 7), Block.EnumBlock.Dirt);
                                    chunk.SetEBlock(new vec3i(7, y + stolb, 5), Block.EnumBlock.Glass);
                                   // chunk.SetEBlock(new vec3i(7, y + stolb, 9), Block.EnumBlock.GlassWhite);
                                    chunk.SetEBlock(new vec3i(11, y + stolb, 7), Block.EnumBlock.Lava);
                                    chunk.SetEBlock(new vec3i(12, y + stolb, 7), Block.EnumBlock.LogOak);
                                }
                                //for (int y = 16; y <= 216; y++)
                                //{
                                //    chunk.SetEBlock(new vec3i(12, y + stolb, 7), Block.EnumBlock.LogOak);
                                //    //chunk.SetEBlock(new vec3i(3, y + stolb, 7), Block.EnumBlock.Dirt);
                                //}

                                //chunk.SetEBlock(new vec3i(5, 2 + stolb, 5), Block.EnumBlock.Brol);

                            }

                            if (pos.x == -3 && pos.y == 0)
                            {
                                //for (int x = 1; x <= 5; x++)
                                //    for (int y = 3; y <= 8; y++)
                                //    {
                                //        chunk.SetEBlock(new vec3i(x, stolb + y, 5), Block.EnumBlock.Dirt);
                                //    }
                                for (int x = 1; x <= 3; x++) for (int y = 6; y <= 8; y++) for (int z = 1; z <= 3; z++)
                                {
                                    chunk.SetEBlock(new vec3i(x, stolb + y, z), Block.EnumBlock.Dirt);
                                }
                                chunk.SetEBlock(new vec3i(0, 30 + stolb, 0), Block.EnumBlock.Dirt);
                                chunk.SetEBlock(new vec3i(8, 10 + stolb, 1), Block.EnumBlock.Dirt);
                                for (int x = 7; x <= 9; x++) for (int z = 1; z <= 3; z++)
                                {
                                    chunk.SetEBlock(new vec3i(x, stolb + 16, z), Block.EnumBlock.Water);
                                }
                                //for (int x = 0; x < 16; x++)
                                //{
                                //    for (int z = 0; z < 16; z++)
                                //    {
                                //        int y = 255;
                                //        while(y > 0)
                                //        {
                                //            if (chunk.GetEBlock(x, y, z) == Block.EnumBlock.Air)
                                //            {
                                //                int yc = y >> 4;
                                //                int yb = y & 15;
                                //                chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Sky, 15);
                                //                //chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Block, 15);
                                //                y--;
                                //            }
                                //            else
                                //            {
                                //                y = -1;
                                //            }
                                //        }
                                //    }
                                //}
                            }
                            //if (pos.x == -3 && pos.y == 1)
                            //{
                            //    for (int x = 0; x < 16; x++)
                            //    {
                            //        for (int z = 0; z < 16; z++)
                            //        {
                            //            int y = 255;
                            //            while (y > 0)
                            //            {
                            //                if (chunk.GetEBlock(x, y, z) == Block.EnumBlock.Air)
                            //                {
                            //                    int yc = y >> 4;
                            //                    int yb = y & 15;
                            //                    //chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Sky, 15);
                            //                    chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Block, 10);
                            //                    y--;
                            //                }
                            //                else
                            //                {
                            //                    y = -1;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                            Cave(chunk, yMax >> 4);

                        }

                        List<vec3i> list = new List<vec3i>();

                        for (int x = 0; x < 16; x++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                for (int y = 0; y <= ChunkBase.COUNT_HEIGHT_BLOCK; y++)
                                {
                                    if (chunk.GetBlockState(x, y, z).GetBlock().LightValue > 0)
                                    {
                                        list.Add(new vec3i(x, y, z));
                                    }
                                }
                            }
                        }
                        long le1 = stopwatch.ElapsedTicks;
                        chunk.Light.SetLightBlocks(list.ToArray());
                        long le2 = stopwatch.ElapsedTicks;
                        chunkMapping.Set(chunk);
                        long le3 = stopwatch.ElapsedTicks;
                        chunk.Light.GenerateHeightMapSky();
                        long le4 = stopwatch.ElapsedTicks;
                        chunk.OnChunkLoad();
                        long le5 = stopwatch.ElapsedTicks;
                        //worldServer.Log.Log("Chunk {0:0.00} {1:0.00} {2:0.00} {3:0.00} {4:0.00}",
                        //    le1 / (float)MvkStatic.TimerFrequency,
                        //    (le2 - le1) / (float)MvkStatic.TimerFrequency,
                        //    (le3 - le2) / (float)MvkStatic.TimerFrequency,
                        //    (le4 - le3) / (float)MvkStatic.TimerFrequency,
                        //    (le5 - le4) / (float)MvkStatic.TimerFrequency
                        //    );
                    }

                    return chunk;
                }

                /// <summary>
                /// Генерация пещер
                /// </summary>
                private void Cave(ChunkBase chunk, int yMax)
                {
                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();

                    int count = 0;
                    float[] noise = new float[4096];
                    for (int y0 = 0; y0 <= yMax; y0++)
                    {
                        worldServer.Noise.Cave.GenerateNoise3d(noise, chunk.Position.x * 16, y0 * 16, chunk.Position.y * 16, 16, 16, 16, .05f, .05f, .05f);
                        count = 0;
                        for (int x = 0; x < 16; x++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                for (int y = 0; y < 16; y++)
                                {
                                    if (noise[count] < -1f)
                                    {
                                        vec3i pos = new vec3i(x, y0 << 4 | y, z);
                                        int y2 = y0 << 4 | y;
                                        Block.EnumBlock enumBlock = chunk.GetBlockState(x, y2, z).GetBlock().EBlock;
                                        if (enumBlock != Block.EnumBlock.Air && enumBlock != Block.EnumBlock.Stone
                                            && enumBlock != Block.EnumBlock.Water)
                                        {
                                            chunk.SetEBlock(pos, Block.EnumBlock.Air);
                                        }
                                    }
                                    count++;
                                }
                            }
                        }
                    }
                    //long le1 = stopwatch.ElapsedTicks;
                    //worldServer.Log.Log("Cave t:{0:0.00}ms",
                    //        le1 / (float)MvkStatic.TimerFrequency);

                }*/

        /// <summary>
        /// Выгрузка ненужных чанков Для сервера
        /// </summary>
        public void UnloadQueuedChunks()
        {
            int i;
            int count = DroppedChunks.Count;
            int first = count - 100;
            if (first < 0) first = 0;
            vec2i pos;
            ChunkBase chunk;
            count--;
            for (i = count; i >= first; i--)
            {
                pos = DroppedChunks[i];
                DroppedChunks.RemoveLast();
                chunk = chunkMapping.Get(pos);
                if (chunk != null)
                {
                    chunk.OnChunkUnload();
                    // TODO::Тут сохраняем чанк
                    chunkMapping.Remove(pos);
                }
            }
        }

        /// <summary>
        /// Обновить проверку дополнительной генерации, чанки где нужны соседние 8 чанков
        /// </summary>
        public void UpdateCheckAddGeneration()
        {
            if (GenAddChunks.Count > 0)
            {
                int i = 0;
                vec2i pos;
                ChunkBase chunk;
                //worldServer.Log.Log("GenChunks.Count {0}", GenAddChunks.Count);
                while (GenAddChunks.Count > 0 && i < 10)
                {
                    pos = GenAddChunks.FirstRemove();
                    chunk = chunkMapping.Get(pos);
                    if (chunk != null)
                    {
                        chunk.Light.StartRecheckGaps();
                    }
                    i++;
                }
            }
        }

        /// <summary>
        /// Добавить чанк на удаление
        /// </summary>
        public void DropChunk(vec2i pos)
        {
            GenAddChunks.Remove(pos);
            DroppedChunks.Add(pos);
        }
    }
}
