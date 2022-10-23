using MvkServer.Entity;
using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Block;
using MvkServer.World.Chunk.Light;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public class ChunkBase
    {
        /// <summary>
        /// Количество псевдо чанков
        /// </summary>
        public const int COUNT_HEIGHT = 16;
        /// <summary>
        /// Количество псевдо чанков COUNT_HEIGHT - 1
        /// </summary>
        public const int COUNT_HEIGHT15 = 15;
        /// <summary>
        /// Количество блоков в высоту
        /// </summary>
        public const int COUNT_HEIGHT_BLOCK = 255;
        /// <summary>
        /// Количество блоков за так в чанке без приоритетных
        /// </summary>
        private const int COUNT_BLOCK_TICK = 8;

        /// <summary>
        /// Данные чанка
        /// </summary>
        public ChunkStorage[] StorageArrays { get; private set; } = new ChunkStorage[COUNT_HEIGHT];
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public WorldBase World { get; private set; }
        /// <summary>
        /// Позиция чанка
        /// </summary>
        public vec2i Position { get; private set; }
        /// <summary>
        /// Список сущностей в каждом псевдочанке
        /// </summary>
        public MapListEntity[] ListEntities { get; private set; } = new MapListEntity[COUNT_HEIGHT];
        /// <summary>
        /// Объект работы с освещением
        /// </summary>
        public ChunkLight2 Light { get; private set; }
        /// <summary>
        /// Присутствует, этап загрузки или начальная генерация #1
        /// </summary>
        public bool IsChunkPresent { get; private set; } = false;
        /// <summary>
        /// Было ли декорация чанка #2
        /// </summary>
        public bool IsPopulated { get; private set; } = false;
        /// <summary>
        /// Было ли карта высот с небесным освещением #3
        /// </summary>
        public bool IsHeightMapSky { get; private set; } = false;
        /// <summary>
        /// Было ли боковое небесное освещение и блочное освещение #4
        /// </summary>
        public bool IsSideLightSky { get; private set; } = false;
        /// <summary>
        /// Готов ли чанк для отправки клиентам #5
        /// </summary>
        public bool IsSendChunk { get; private set; } = false;
        /// <summary>
        /// Загружен ли чанк, false если была генерация, для дополнительной правки освещения
        /// </summary>
        public bool IsLoaded { get; private set; } = false;
        /// <summary>
        /// Биомы
        /// x << 4 | z;
        /// </summary>
        public EnumBiome[] biome = new EnumBiome[256];

        /// <summary>
        /// Статус готовности чанка 0-4
        /// 0 - генерация
        /// 1 - объект на этом чанке
        /// 2 - объект на соседнем чанке (карта высот)
        /// 3 - боковое освещение
        /// 4 - готов может и не надо, хватит 3 
        /// </summary>    
        //public int DoneStatus { get; set; } = 0;
        /// <summary>
        /// Совокупное количество тиков, которые игроки провели в этом чанке 
        /// </summary>
        public uint InhabitedTime { get; private set; }

        /// <summary>
        /// Карта высот по чанку, рельефа при генерации z << 4 | x
        /// </summary>
        private byte[] heightMapGen = new byte[256];
        /// <summary>
        /// Последнее обновление чанка в тактах
        /// </summary>
        protected long updateTime;
        /// <summary>
        /// Установите значение true, если чанк был изменен и нуждается в внутреннем обновлении. Для сохранения
        /// </summary>
        private bool isModified;
        /// <summary>
        /// Имеет ли этот фрагмент какие-либо сущности и, следовательно, требует сохранения на каждом тике
        /// </summary>
        private bool hasEntities = false;
        /// <summary>
        /// Время последнего сохранения этого фрагмента согласно World.worldTime
        /// </summary>
        private long lastSaveTime;
        /// <summary>
        /// Блоки которые должны тикать мгновенно в чанке
        /// </summary>
        private List<TickBlock> tickBlocks = new List<TickBlock>();

        protected ChunkBase() { }
        public ChunkBase(WorldBase worldIn, vec2i pos)
        {
            World = worldIn;
            Position = pos;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                StorageArrays[y] = new ChunkStorage(y << 4);
                ListEntities[y] = new MapListEntity();
            }
            Light = new ChunkLight2(this);
        }
        /// <summary>
        /// Возвращает значение карты высот рельефа при генерации в этой координате x, z в чанке. 
        /// </summary>
        public byte GetHeightGen(int x, int z) => heightMapGen[z << 4 | x];
        /// <summary>
        /// Сгенерировать копию высот для популяции
        /// </summary>
        public void InitHeightMapGen() => heightMapGen = Light.CloneHeightMap();

        /// <summary>
        /// Загружен чанк или сгенерирован
        /// </summary>
        //public void ChunkLoadGen()
        //{
        //    StorageArraysClear();

        //    //Random random = new Random();

        //    //if (random.Next(20) == 1)
        //    //{
        //    //    IsChunkLoaded = true;
        //    //    return;
        //    //}

        //    for (int y0 = 0; y0 < 24; y0++)
        //    {
        //        int sy = y0 >> 4;
        //        int y = y0 & 15;
        //        if (y0 > 8 && y0 < 16) continue;
        //        for (int x = 0; x < 16; x++)
        //        {
        //            for (int z = 0; z < 16; z++)
        //            {
        //                StorageArrays[sy].SetEBlock(x, y, z, (y0 == 23) ? EnumBlock.Turf : EnumBlock.Stone);
        //            }
        //        }
        //    }
        //    for (int i = 5; i <= 10; i++)
        //    {
        //        StorageArrays[1].SetEBlock(12, 7, i, EnumBlock.Air);
        //        StorageArrays[1].SetEBlock(13, 7, i, EnumBlock.Air);
        //        StorageArrays[1].SetEBlock(14, 7, i, EnumBlock.Air);
        //        StorageArrays[1].SetEBlock(15, 7, i, EnumBlock.Air);
        //    }
        //    StorageArrays[1].SetEBlock(14, 7, 11, EnumBlock.Air);
        //    StorageArrays[1].SetEBlock(15, 7, 11, EnumBlock.Air);
        //    StorageArrays[1].SetEBlock(14, 7, 12, EnumBlock.Air);
        //    StorageArrays[1].SetEBlock(15, 7, 12, EnumBlock.Air);

        //    StorageArrays[1].SetEBlock(10, 8, 6, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(10, 8, 5, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(11, 8, 6, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(11, 8, 5, EnumBlock.Turf);

        //    StorageArrays[1].SetEBlock(14, 8, 12, EnumBlock.Cobblestone);

        //    //   StorageArrays[1].SetEBlock(8, 8, 0, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(6, 8, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(5, 8, 0, EnumBlock.Stone);
        //    //StorageArrays[1].SetEBlock(6, 9, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(5, 9, 0, EnumBlock.Cobblestone);

        //    StorageArrays[1].SetEBlock(3, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(3, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(2, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(2, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(2, 10, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(1, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(1, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(1, 10, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(0, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(0, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(0, 10, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(0, 11, 0, EnumBlock.Dirt);
        //    StorageArrays[1].SetEBlock(0, 12, 0, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(0, 12, 14, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(0, 12, 15, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(1, 12, 14, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(1, 12, 15, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(1, 8, 15, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(0, 11, 1, EnumBlock.Dirt);
        //    StorageArrays[1].SetEBlock(0, 11, 2, EnumBlock.Dirt);
        //    StorageArrays[1].SetEBlock(1, 7, 0, EnumBlock.Dirt);

        //    int xv = 8;
        //    int zv = 1;

        //    StorageArrays[1].SetEBlock(xv, 8, zv, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(xv, 9, zv, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(xv, 10, zv, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(xv, 11, zv, EnumBlock.Stone);

        //    //IsChunkLoaded = true;// LoadinData();
        //    // Продумать, для клиента запрос для сервера данных чанка, 
        //    // для сервера чанк пытается загрузиться, если он не создан то создаём

        //    System.Threading.Thread.Sleep(2);
        //}

        /// <summary>
        /// Выгружаем чанк
        /// </summary>
        public void OnChunkUnload()
        {
            IsChunkPresent = false;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                World.UnloadEntities(ListEntities[y]);
            }
            // Продумать, для клиента просто удалить, для сервера записать и удалить
            //Save();
            //StorageArraysClear();
            //for (int y = 0; y < StorageArrays.Length; y++)
            //{
            //    StorageArrays[y].Delete();
            //}
        }

        public void OnChunkLoad()
        {
            IsChunkPresent = true;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                // Продумать загрузку чанка у сущности тип 
                // ListEntities[y].GetAt(0).OnChunkLoad();

                World.LoadEntities(ListEntities[y]);
            }
            if (!World.IsRemote && World is WorldServer worldServer)
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                worldServer.profiler.StartSection("PopulateChunk");
                // После генерации проверяем все близлежащие чанки для декорации
                ChunkBase chunk;
                for (int i = 0; i < 9; i++)
                {
                    chunk = worldServer.ChunkPrServ.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk != null && chunk.IsChunkPresent)
                    {
                        chunk.Populate(worldServer.ChunkPrServ);
                    }
                }
                worldServer.profiler.EndSection();
                //long le1 = stopwatch.ElapsedTicks;
                //worldServer.Log.Log("PopulateChunk[{1}]: {0:0.00} ms", le1 / (float)MvkStatic.TimerFrequency, Position);
            }
        }

        /// <summary>
        /// Заполнение чанка населённостью
        /// </summary>
        public void Populate(ChunkProviderServer provider)
        {
            if (!IsPopulated)
            {
                // Если его в чанке нет проверяем чтоб у всех чанков близлежащих была генерация
                ChunkBase chunk;
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk == null || !chunk.IsChunkPresent)
                    {
                        return;
                    }
                }
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                provider.ChunkGenerate.Populate(this);
                IsPopulated = true;
                //World.Log.Log("Populate[{1}]: {0:0.00} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, Position);
                // После населённостью проверяем все близлежащие чанки с населёностью для обработки небесного освещения
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk != null && chunk.IsChunkPresent && chunk.IsPopulated)
                    {
                        chunk.HeightMapSky(provider);
                    }
                }
            }
        }

        /// <summary>
        /// Карта высот с вертикальным небесным освещением
        /// </summary>
        private void HeightMapSky(ChunkProviderServer provider)
        {
            if (!IsHeightMapSky)
            {
                // Если его в чанке нет проверяем чтоб у всех чанков близлежащих была популяция
                ChunkBase chunk;
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk == null || !chunk.IsChunkPresent || !chunk.IsPopulated)
                    {
                        return;
                    }
                }
                // Карта высот с вертикальным небесным освещением
                // Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                Light.GenerateHeightMapSky();
                IsHeightMapSky = true;
                //World.Log.Log("HeightMapSky[{1}]: {0:0.00} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, Position);
                // После Карта высот проверяем все близлежащие чанки с картой высот для обработки бокового небесного освещения
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk != null && chunk.IsChunkPresent && chunk.IsPopulated && chunk.IsHeightMapSky)
                    {
                        chunk.SideLightSky(provider);
                    }
                }
            }
        }

        /// <summary>
        /// Боковое небесное освещение и блочное освещение
        /// </summary>
        private void SideLightSky(ChunkProviderServer provider)
        {
            if (!IsSideLightSky)
            {
                // Если его в чанке нет проверяем чтоб у всех чанков близлежащих было небесное освещение
                ChunkBase chunk;
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk == null || !chunk.IsChunkPresent || !chunk.IsPopulated || !chunk.IsHeightMapSky)
                    {
                        return;
                    }
                }
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                // Боковое небесное освещение и блочное освещение
                Light.StartRecheckGaps();
                IsSideLightSky = true;
                //World.Log.Log("SideLightSky[{1}]: {0:0.00} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, Position);
                // После бокового освещения проверяем все близлежащие чанки с боковым освещение для возможности отправки
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk != null && chunk.IsChunkPresent && chunk.IsPopulated && chunk.IsHeightMapSky && chunk.IsSideLightSky)
                    {
                        chunk.SendChunk(provider);
                    }
                }
            }
        }
        /// <summary>
        /// Возможность отправлять чанк клиентам
        /// </summary>
        private void SendChunk(ChunkProviderServer provider)
        {
            if (!IsSendChunk)
            {
                // Если его в чанке нет проверяем чтоб у всех чанков близлежащих было небесное освещение
                ChunkBase chunk;
                for (int i = 0; i < 9; i++)
                {
                    chunk = provider.GetChunk(MvkStatic.AreaOne9[i] + Position);
                    if (chunk == null || !chunk.IsChunkPresent || !chunk.IsPopulated || !chunk.IsHeightMapSky || !chunk.IsSideLightSky)
                    {
                        return;
                    }
                }
                IsSendChunk = true;
                //isModified = true;
            }
        }

        /// <summary>
        /// Очистить данные чанков
        /// </summary>
        protected void StorageArraysClear()
        {
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                StorageArrays[y].Clear();
                ListEntities[y].Clear();
            }
        }

        /// <summary>
        /// Задать чанк байтами
        /// </summary>
        public void SetBinary(byte[] buffer, bool biom, int flagsYAreas)
        {
            // World.Log.Log("SetBinaryBegin {0} {1}", Position, GetDebugAllSegment());
            int sy, i, value;
            ushort countMet;
            int count = 0;
            byte light;
            ushort id, key;
            try
            {
                for (sy = 0; sy < COUNT_HEIGHT; sy++)
                {
                    if ((flagsYAreas & 1 << sy) != 0)
                    {
                        ChunkStorage storage = StorageArrays[sy];
                        for (i = 0; i < 4096; i++)
                        {
                            value = buffer[count++] | buffer[count++] << 8;
                            id = (ushort)(value & 0xFFF);
                            storage.SetData(i, id, (ushort)(value >> 12));
                            light = buffer[count++];
                            storage.lightBlock[i] = (byte)(light >> 4);
                            storage.lightSky[i] = (byte)(light & 0xF);
                            if (!Blocks.blocksAddMet[id]) storage.addMet.Remove((ushort)i);
                        }
                        countMet = (ushort)(buffer[count++] | buffer[count++] << 8);
                        for (i = 0; i < countMet; i++)
                        {
                            key = (ushort)(buffer[count++] | buffer[count++] << 8);
                            if (storage.addMet.ContainsKey(key)) storage.addMet[key] = (ushort)(buffer[count++] | buffer[count++] << 8);
                            else storage.addMet.Add(key, (ushort)(buffer[count++] | buffer[count++] << 8) );
                        }
                        ModifiedToRender(sy);
                    }
                }
                // биом
                if (biom)
                {
                    for (i = 0; i < 256; i++)
                    {
                        biome[i] = (EnumBiome)buffer[count++];
                    }
                }
                IsChunkPresent = true;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }

            //World.Log.Log("SetBinaryEnd {0} {1}", Position, GetDebugAllSegment());
        }

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка для клиента
        /// </summary>
        public virtual void ModifiedToRender(int y) { }

        /// <summary>
        /// Задать чанк байтами
        /// </summary>
        //public void SetBinary(byte[] buffer, int height)
        //{
        //    int i = 0;
        //    for (int sy = 0; sy < height; sy++)
        //    {
        //        StorageArrays[sy].Clear();
        //        for (int y = 0; y < 16; y++)
        //        {
        //            for (int x = 0; x < 16; x++)
        //            {
        //                for (int z = 0; z < 16; z++)
        //                {
        //                    StorageArrays[sy].SetData(x, y, z, (ushort)(buffer[i++] | buffer[i++] << 8));
        //                    StorageArrays[sy].SetLightsFor(x, y, z, buffer[i++]);
        //                }
        //            }
        //        }
        //    }
        //    IsChunkLoaded = true;
        //}

        /// <summary>
        /// Задать псевдо чанк байтами
        /// </summary>
        //public void SetBinaryY(byte[] buffer, int sy)
        //{
        //    int i = 0;
        //    //StorageArrays[sy].Clear();
        //    for (int y = 0; y < 16; y++)
        //    {
        //        for (int x = 0; x < 16; x++)
        //        {
        //            for (int z = 0; z < 16; z++)
        //            {
        //                StorageArrays[sy].SetData(x, y, z, (ushort)(buffer[i++] | buffer[i++] << 8));
        //                StorageArrays[sy].SetLightsFor(x, y, z, buffer[i++]);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Обновить время использования чанка
        /// </summary>
        public void UpdateTime() => updateTime = DateTime.Now.Ticks;

        /// <summary>
        /// Старый ли чанк (больше 10 сек)
        /// </summary>
        public bool IsOldTime() => DateTime.Now.Ticks - updateTime > 100000000;

        /// <summary>
        /// Возвращает самый верхний экземпляр BlockStorage для этого фрагмента, который фактически содержит блок.
        /// </summary>
        public int GetTopFilledSegment()
        {
            for (int y = StorageArrays.Length - 1; y >= 0; y--)
            {
                if (!StorageArrays[y].IsEmptyData())
                {
                    return StorageArrays[y].GetYLocation();
                }
            }
            return 0;
        }

        /// <summary>
        /// Возвращает строку всех сегментов
        /// </summary>
        public string GetDebugAllSegment()
        {
            string s = "";
            ChunkStorage cs;
            for (int y = 0; y < StorageArrays.Length; y++)
            {
                cs = StorageArrays[y];
                s += cs.IsEmptyData() ? "." : "*" + cs.ToCountString();
                //s += cs.sky ? "S" : "s";
            }
            return s;
        }

        #region Block

        ///// <summary>
        ///// Получить блок по координатам чанка XZ 0..15, Y 0..255
        ///// </summary>
        //public BlockBase GetBlock0(int x, int y, int z)
        //{
        //    BlockState blockState = GetBlockState(x, y, z);
        //    return blockState.GetBlock();
        //}

        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            if (x >> 4 == 0 && z >> 4 == 0) return GetBlockStateNotCheck(x, y, z);
            return new BlockState().Empty();
        }

        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255 без проверки
        /// </summary>
        public BlockState GetBlockStateNotCheck(int x, int y, int z)
        {
            ChunkStorage chunkStorage = StorageArrays[y >> 4];
            if (chunkStorage.countBlock != 0)
            {
                return chunkStorage.GetBlockState(x, y & 15, z);
            }
            else
            {
                int index = (y & 15) << 8 | z << 4 | x;
                return new BlockState(0, 0, chunkStorage.lightBlock[index], chunkStorage.lightSky[index]);
            }
        }

        /// <summary>
        /// Получить id блока, XZ 0..15, Y 0..255
        /// </summary>
        public int GetBlockId(int x, int y, int z)
        {
            if (x >> 4 == 0 && z >> 4 == 0)
            {
                ChunkStorage chunkStorage = StorageArrays[y >> 4];
                if (chunkStorage.countBlock != 0)
                {
                    return chunkStorage.data[(y & 15) << 8 | z << 4 | x] & 0xFFF;
                }
            }
            return 0; // воздух
        }

        /// <summary>
        /// Получить тип блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        //public EnumBlock GetEBlock(vec3i pos) => GetEBlock(pos.x, pos.y, pos.z);
        ///// <summary>
        ///// Получить тип блок по координатам чанка XZ 0..15, Y 0..255
        ///// </summary>
        //public EnumBlock GetEBlock(int x, int y, int z)
        //{
        //    int ys = y >> 4;
        //    if (x >> 4 == 0 && z >> 4 == 0 && !StorageArrays[ys].IsEmpty()) return StorageArrays[ys].GetEBlock(x, y & 15, z);
        //    return EnumBlock.Air;
        //}

        /// <summary>
        /// Задать тип блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        public void SetEBlock(vec3i pos, EnumBlock eBlock, ushort met = 0)
        {
            int y = pos.y >> 4;
            if (pos.x >> 4 == 0 && pos.z >> 4 == 0 && pos.y >= 0 && y < COUNT_HEIGHT)
            {
                StorageArrays[y].SetData((pos.y & 15) << 8 | pos.z << 4 | pos.x, (ushort)eBlock, met);
            }
        }
        /// <summary>
        /// Задать тип блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        public void SetEBlock(int x, int y, int z, EnumBlock eBlock, ushort met = 0)
        {
            int chy = y >> 4;
            if (x >> 4 == 0 && z >> 4 == 0 && y >= 0 && chy < COUNT_HEIGHT)
            {
                StorageArrays[chy].SetData((y & 15) << 8 | z << 4 | x, (ushort)eBlock, met);
            }
        }

        public void SetBlockStateClient(BlockPos blockPos, BlockState blockState)
        {
            int bx = blockPos.X & 15;
            int by = blockPos.Y & 15;
            int bz = blockPos.Z & 15;
            int chy = blockPos.Y >> 4;
            int index = by << 8 | bz << 4 | bx;
            //if (StorageArrays[chy].IsEmptyData())// && StorageArrays[chy].IsEmptyLight())
            //{
            //    // Если вверхняя часть впервые создаётся, заполняем небесный свет все блоки
            //    StorageArrays[chy].LightSky();
            //}
            ChunkStorage storage = StorageArrays[chy];

            storage.SetData(index, blockState.id, blockState.met);
            storage.lightBlock[index] = blockState.lightBlock;
            storage.lightSky[index] = blockState.lightSky;
            //storage.light[by << 8 | bz << 4 | bx] = (byte)(blockState.lightBlock << 4 | blockState.lightSky & 0xF);
        }

        /// <summary>
        /// Задать новые данные блока, с перерасчётом освещения если надо и прочего, возвращает прошлые данные блока
        /// </summary>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="blockState">данные нового блока</param>
        /// <param name="isModify">Пометка надо сохранение чанка</param>
        /// <param name="isModifyRender">Пометка надо обновить рендер чанка</param>
        public BlockState SetBlockState(BlockPos blockPos, BlockState blockState, bool isModify, bool isModifyRender)
        {
            int bx = blockPos.X & 15;
            int by = blockPos.Y;
            int bz = blockPos.Z & 15;

            // Light.CheckPrecipitationHeightMap(blockPos);

            BlockState blockStateOld = GetBlockState(bx, by, bz);

            if (blockState.Equals(blockStateOld)) return new BlockState().Empty();

            //int height = Light.GetHeight(bx, bz);

            BlockBase block = blockState.GetBlock();
            BlockBase blockOld = blockStateOld.GetBlock();
            int chy = blockPos.Y >> 4;
            // bool heightMapUp = false;

            ChunkStorage storage = StorageArrays[chy];

            //data[y << 8 | z << 4 | x]
            if (storage.countBlock == 0)
            {
                if (block.EBlock == EnumBlock.Air) return new BlockState().Empty();
                //heightMapUp = by >= height;

                // Если вверхняя часть впервые создаётся, заполняем небесный свет все блоки
                //StorageArrays[chy].LightSky();
            }
            int index = (by & 15) << 8 | bz << 4 | bx;
            storage.SetData(index, blockState.id, blockState.met);

            //StorageArrays[chy].Set(bx, by & 15, bz, blockState);

            if (blockOld != block)
            {
                // Отмена тик блока
                RemoveBlockTick(bx, by, bz);

                // проверка света
                //if (World.IsRemote)

                // bool replaceAir = (block.IsAir && !blockOld.IsAir) || (!block.IsAir && blockOld.IsAir);
                bool differenceOpacity = block.LightOpacity != blockOld.LightOpacity;
                if (/*replaceAir || */differenceOpacity || block.LightValue != blockOld.LightValue)
                {
                    World.Light.ActionChunk(this);
                    World.Light.CheckLightFor(blockPos.X, blockPos.Y, blockPos.Z, differenceOpacity, isModify, isModifyRender);//, replaceAir);
                }
                else
                {
                    // проверка высоты
                    Light.CheckHeightMap(blockPos, block.LightOpacity);
                    if (World.IsRemote)
                    {
                        World.Light.ClearDebugString();
                    }
                    if (isModify) Modified();
                }
                if (isModifyRender) World.MarkBlockForRenderUpdate(blockPos.X, blockPos.Y, blockPos.Z);

                if (World.IsRemote)
                {
                    //World.DebugString(Light.debugStr);
                    World.DebugString(World.Light.ToDebugString());
                }
            }
            else if (blockState.met != blockStateOld.met)
            {
                if (isModifyRender) World.MarkBlockForRenderUpdate(blockPos.X, blockPos.Y, blockPos.Z);
            }

            if (!World.IsRemote && blockOld != block)
            {
                block.OnBlockAdded(World, blockPos, blockState);
            }

            //MarkBlockForUpdate(blockPos);

            if (storage.countBlock == 0 || (storage.data[index]) != (ushort)block.EBlock) return new BlockState();


            

            //if (heightMapUp)
            //{
            //    Light.GenerateSkylightMap();
            //}
            //else
            //{
            //    Light.CheckBlockState(blockPos, height, block.LightOpacity, blockOld.LightOpacity);
            //}
            //Light.CheckLightSetBlock(blockNew.Position, blockNew.LightOpacity, blockOld.LightOpacity,
            //    blockNew.LightValue != blockOld.LightValue);

            //TileEntity var15;

            //if (blockOld instanceof ITileEntityProvider)
            //{
            //    var15 = this.func_177424_a(blockPos, Chunk.EnumCreateEntityType.CHECK);

            //    if (var15 != null)
            //    {
            //        var15.updateContainingBlockInfo();
            //    }
            //}


            //if (block instanceof ITileEntityProvider)
            //{
            //    var15 = this.func_177424_a(blockPos, Chunk.EnumCreateEntityType.CHECK);

            //    if (var15 == null)
            //    {
            //        var15 = ((ITileEntityProvider)block).createNewTileEntity(this.worldObj, block.getMetaFromState(blockState));
            //        this.worldObj.setTileEntity(blockPos, var15);
            //    }

            //    if (var15 != null)
            //    {
            //        var15.updateContainingBlockInfo();
            //    }
            //}


            return blockStateOld;
        }

        /// <summary>
        /// Задать тик блока с локальной позицие и время через сколько тактов надо тикнуть
        /// </summary>
        public void SetBlockTick(int x, int y, int z, uint timeTackt, bool priority = false)
        {
            TickBlock tickBlock;
            bool empty = true;
            for (int i = 0; i < tickBlocks.Count; i++)
            {
                tickBlock = tickBlocks[i];
                if (tickBlock.x == x && tickBlock.y == y && tickBlock.z == z)
                {
                    tickBlock.scheduledTime = timeTackt + World.GetTotalWorldTime();
                    tickBlock.priority = priority;
                    tickBlocks[i] = tickBlock;
                    empty = false;
                    break;
                }
            }
            if (empty) tickBlocks.Add(new TickBlock() { x = x, y = y, z = z, scheduledTime = timeTackt + World.GetTotalWorldTime(), priority = priority });
        }

        /// <summary>
        /// Отменить мгновенный тик блока
        /// </summary>
        private void RemoveBlockTick(int x, int y, int z)
        {
            TickBlock tickBlock;
            int index = -1;
            for (int i = 0; i < tickBlocks.Count; i++)
            {
                tickBlock = tickBlocks[i];
                if (tickBlock.x == x && tickBlock.y == y && tickBlock.z == z)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1) tickBlocks.RemoveAt(index);
        }

        #endregion

        #region Entity

        /// <summary>
        /// Добавить сущность в чанк
        /// </summary>
        public void AddEntity(EntityBase entity)
        {
            hasEntities = true;
            int x = Mth.Floor(entity.Position.x) >> 4;
            int z = Mth.Floor(entity.Position.z) >> 4;

            if (x != Position.x || z != Position.y)
            {
                World.Log.Log("ChunkBase: Неверное местоположение! ({0}, {1}) должно быть ({2}), {3}", x, z, Position, entity.Type);
                entity.SetDead();
                return;
            }

            int y = Mth.Floor(entity.Position.y) >> 4;
            if (y < 0) y = 0;
            if (y >= COUNT_HEIGHT) y = COUNT_HEIGHT - 1;

            entity.SetPositionChunk(Position.x, y, Position.y);
            ListEntities[y].Add(entity);
        }

        /// <summary>
        /// Удаляет сущность из конкретного псевдочанка
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="y">уровень псевдочанка</param>
        public void RemoveEntityAtIndex(EntityBase entity, int y)
        {
            if (y < 0) y = 0;
            if (y >= COUNT_HEIGHT) y = COUNT_HEIGHT - 1;
            ListEntities[y].Remove(entity);
        }

        /// <summary>
        ///  Удаляет сущность, используя его координату y в качестве индекса
        /// </summary>
        /// <param name="entity">сущность</param>
        public void RemoveEntity(EntityBase entity) => RemoveEntityAtIndex(entity, entity.PositionChunkY);

        /// <summary>
        /// Получить список id всех сущностей в чанке
        /// </summary>
        public EntityBase[] GetEntities()
        {
            List<EntityBase> list = new List<EntityBase>();
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                for (int i = 0; i < ListEntities[y].Count; i++)
                {
                    EntityBase entity = ListEntities[y].GetAt(i);
                    if (entity != null && entity.AddedToChunk)
                    {
                        list.Add(entity);
                    }
                }
                //list.AddRange(ListEntities[y].GetList());
            }
            return list.ToArray();
        }

        /// <summary>
        /// Получить список всех сущностей попадающих в рамку кроме входящей сущности
        /// </summary>
        /// <param name="entityId">входящяя сущность</param>
        /// <param name="aabb">рамка</param>
        /// <param name="list">список</param>
        public void GetEntitiesAABB(int entityId, AxisAlignedBB aabb, MapListEntity list, EnumEntityClassAABB type)
        {
            int minY = Mth.Floor((aabb.Min.y - 2f) / 16f);
            int maxY = Mth.Floor((aabb.Max.y + 2f) / 16f);
            minY = Mth.Clamp(minY, 0, ListEntities.Length - 1);
            maxY = Mth.Clamp(maxY, 0, ListEntities.Length - 1);

            for (int y = minY; y <= maxY; y++)
            {
                for (int i = 0; i < ListEntities[y].Count; i++)
                {
                    EntityBase entity = ListEntities[y].GetAt(i);
                    if (entity != null && entity.Id != entityId
                        && (type == EnumEntityClassAABB.All
                            || (type == EnumEntityClassAABB.EntityItem && entity is EntityItem)
                            || (type == EnumEntityClassAABB.EntityLiving && entity is EntityLiving)
                            )
                        && entity.BoundingBox.IntersectsWith(aabb))
                    {
                        list.Add(entity);
                    }
                }
            }
        }

        public enum EnumEntityClassAABB
        {
            /// <summary>
            /// Все
            /// </summary>
            All,
            /// <summary>
            /// Наследники сущности EntityItem
            /// </summary>
            EntityItem,
            /// <summary>
            /// Наследники мобов и игроков
            /// </summary>
            EntityLiving
        }


        /// <summary>
        /// Получить количество сущностей в чанке
        /// </summary>
        public int CountEntity()
        {
            int count = 0;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                count += ListEntities[y].Count;
            }
            return count;
        }

        #endregion

        #region Modify

        /// <summary>
        /// Пометка что чанк надо будет перезаписать
        /// </summary>
        public void Modified() => isModified = true;
        /// <summary>
        /// Пыло сохранение чанка, пометка убирается
        /// </summary>
        //public void SavedNotModified() => isModified = false;

        /// <summary>
        /// Возвращает true, если этот чанке необходимо сохранить
        /// </summary>
        //public bool NeedsSaving()
        //{
        //    if (hasEntities && World.GetTotalWorldTime() != lastSaveTime || isModified)
        //    {
        //        return true;
        //    }

        //    return isModified;
        //}

        /// <summary>
        /// Заменить флаг на наличие сущностей в чанке.
        /// Этот метод вызывается в момент сохранения чанка
        /// </summary>
        public void SetHasEntities(bool hasEntitiesIn) => hasEntities = hasEntitiesIn;

        #endregion


        /// <summary>
        /// Обновление в такте активных чанков, только на сервере
        /// </summary>
        public void UpdateServer()
        {
            TickBlock tickBlock;
            List<int> listRemove = new List<int>();
            List<vec3i> listTick = new List<vec3i>();

            uint time = World.GetTotalWorldTime();
            int count = 0;
            // Пробегаемся по всем тикам блоков и собираем которые надо выполнять
            for (int i = 0; i < tickBlocks.Count; i++)
            {
                tickBlock = tickBlocks[i];
                if (tickBlock.scheduledTime <= time && (count < COUNT_BLOCK_TICK || tickBlock.priority))
                {
                    count++;
                    listRemove.Add(i);
                    listTick.Add(new vec3i(tickBlock.x, tickBlock.y, tickBlock.z));
                }
            }
            if (listRemove.Count > 0)
            {
                count = listRemove.Count - 1;
                // Удаляем которые надо выполнять
                for (int i = count; i >= 0; i--)
                {
                    tickBlocks.RemoveAt(listRemove[i]);
                }
                BlockState blockState;
                vec3i pos;
                int chx = Position.x << 4;
                int chz = Position.y << 4;
                // Выполнение
                for (int i = 0; i <= count; i++)
                {
                    pos = listTick[i];
                    blockState = GetBlockState(pos.x, pos.y, pos.z);
                    BlockBase block = blockState.GetBlock();
                    block.UpdateTick(World, new BlockPos(chx | pos.x, pos.y, chz | pos.z),
                        blockState, World.Rnd);
                }
            }
        }

        /// <summary>
        /// Задать совокупное количество тиков, которые игроки провели в этом чанке 
        /// </summary>
        public void SetInhabitedTime(uint time) => InhabitedTime = time;

        /// <summary>
        /// Сохранить чанк в файл региона
        /// </summary>
        public bool SaveFileChunk(WorldServer worldServer)
        {
            if (IsSendChunk && (hasEntities || isModified))
            {
                lastSaveTime = World.GetTotalWorldTime();
                TagCompound nbt = new TagCompound();
                WriteChunkToNBT(nbt);
                worldServer.Regions.Get(Position).WriteChunk(nbt, Position.x, Position.y);
                //worldServer.File.ChunkDataWrite(nbt, worldServer.Regions.Get(Position), Position);
                isModified = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Прочесть чанк в файл региона
        /// </summary>
        public void LoadFileChunk(WorldServer worldServer)
        {
            RegionFile region = worldServer.Regions.Get(Position);
            if (region != null)
            {
                TagCompound nbt = region.ReadChunk(Position.x, Position.y);
                if (nbt != null)
                {
                    try
                    {
                        ReadChunkFromNBT(nbt);
                        IsChunkPresent = true;
                        IsPopulated = true;
                        IsHeightMapSky = true;
                        IsSideLightSky = true;
                        IsSendChunk = true;
                        IsLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        World.Log.Error(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Количество тикающих блоков в чанке
        /// </summary>
        public int GetTickBlockCount() => tickBlocks.Count;

        #region NBT

        public void WriteChunkToNBT(TagCompound nbt)
        {
            uint time = World.GetTotalWorldTime();
            nbt.SetShort("Version", 1);
            nbt.SetInt("PosX", Position.x);
            nbt.SetInt("PosZ", Position.y);
            nbt.SetLong("LastUpdate", time);
            nbt.SetLong("InhabitedTime", InhabitedTime);
            nbt.SetShort("HeightMapMax", (short)Light.heightMapMax);
            nbt.SetByteArray("HeightMap", Light.heightMap);
            nbt.SetByteArray("HeightMapGen", heightMapGen);
            byte[] biomes = new byte[256];
            for (int i = 0; i < 256; i++) biomes[i] = (byte)biome[i];
            nbt.SetByteArray("Biomes", biomes);

            TagList tagListSections = new TagList();
            
            for (int ys = 0; ys < COUNT_HEIGHT; ys++)
            {
                StorageArrays[ys].WriteDataToNBT(tagListSections);
            }
            nbt.SetTag("Sections", tagListSections);

            if (tickBlocks.Count > 0)
            {
                TagList tagListTickBlocks = new TagList();
                TickBlock tickBlock;
                for (int i = 0; i < tickBlocks.Count; i++)
                {
                    tickBlock = tickBlocks[i];
                    TagCompound tagCompound = new TagCompound();
                    tagCompound.SetByte("X", (byte)tickBlock.x);
                    tagCompound.SetByte("Y", (byte)tickBlock.y);
                    tagCompound.SetByte("Z", (byte)tickBlock.z);
                    tagCompound.SetInt("Time", (int)(tickBlock.scheduledTime - time));
                    tagCompound.SetBool("P", tickBlock.priority);
                    tagListTickBlocks.AppendTag(tagCompound);
                }
                nbt.SetTag("TileTicks", tagListTickBlocks);
            }

            // проверяю есть ли сущность, если есть то true;
            hasEntities = false;
            for (int yc = 0; yc < COUNT_HEIGHT; yc++)
            {
                if (ListEntities[yc].Count > 0)
                {
                    hasEntities = true;
                    break;
                }
            }
        }

        public void ReadChunkFromNBT(TagCompound nbt)
        {
            uint time = World.GetTotalWorldTime();
            uint timeDelta = time - (uint)nbt.GetLong("LastUpdate");
            InhabitedTime = (uint)nbt.GetLong("InhabitedTime");
            Light.heightMapMax = nbt.GetShort("HeightMapMax");
            Light.heightMap = nbt.GetByteArray("HeightMap");
            heightMapGen = nbt.GetByteArray("HeightMapGen");
            byte[] biomes = nbt.GetByteArray("Biomes");
            for (int i = 0; i < 256; i++) biome[i] = (EnumBiome)biomes[i];

            TagList tagListSections = nbt.GetTagList("Sections", 10);
            int count = tagListSections.TagCount();
            if (tagListSections.GetTagType() == 10)
            {
                int y;
                bool[] flag = new bool[COUNT_HEIGHT];
                for (int i = 0; i < count; i++)
                {
                    TagCompound tagCompound = tagListSections.Get(i) as TagCompound;
                    y = tagCompound.GetByte("Y");
                    flag[y] = true;
                    StorageArrays[y].ReadDataFromNBT(tagCompound);
                }
                if (count < COUNT_HEIGHT)
                {
                    TagCompound tagCompound = new TagCompound();
                    for (int i = 0; i < COUNT_HEIGHT; i++)
                    {
                        if (!flag[i])
                        {
                            StorageArrays[i].ReadDataFromNBT(tagCompound);
                        }
                    }
                }
            }

            tickBlocks.Clear();
            TagList tagListTickBlocks = nbt.GetTagList("TileTicks", 10);
            count = tagListTickBlocks.TagCount();
            if (tagListTickBlocks.GetTagType() == 10)
            {
                for (int i = 0; i < count; i++)
                {
                    TagCompound tagCompound = tagListTickBlocks.Get(i) as TagCompound;
                    tickBlocks.Add(new TickBlock()
                    {
                        x = tagCompound.GetByte("X"),
                        y = tagCompound.GetByte("Y"),
                        z = tagCompound.GetByte("Z"),
                        scheduledTime = (uint)tagCompound.GetInt("Time") + time,
                        priority = tagCompound.GetBool("P")
                    });
                }
            }

            // TODO::2022-09-11:hasEntities; Слава Украине!
            // Проверяем сущность, если есть то //hasEntities = true;
            // Скорее всего будет добавление сущности AddEntity, а там hasEntities = true;
            return;
        }

        #endregion

        public override string ToString() => Position + " " 
            + (IsChunkPresent ? "Loaded " : "")
            + (IsPopulated ? "Populated " : "")
            + (IsHeightMapSky ? "HeightMap " : "")
            + (IsSideLightSky ? "Light " : "");
    }
}
;