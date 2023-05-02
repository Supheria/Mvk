using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World.Gen;
using System;
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
        /// Объект для генерации чанков
        /// </summary>
        public ChunkProviderGenerateBase ChunkGenerate { get; private set; }

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
        /// <summary>
        /// Список чанков для сохранения
        /// </summary>
        private List<ChunkBase> listSavingChunk = new List<ChunkBase>();
        /// <summary>
        /// Флаг для сохранения региона,
        /// 0 - нет действий,
        /// 1 - был запуск сохранения но чанки ещё не готовы,
        /// 2 - надо сохранять регион
        /// </summary>
        private byte flagSaveRegion = 0;

        public ChunkProviderServer(WorldServer worldIn, int slot)
        {
            world = worldServer = worldIn;
            if (slot == 1 || slot == 5)
            {
                ChunkGenerate = new ChunkProviderGenerateRobinson(worldIn);
            }
            else
            {
                ChunkGenerate = new ChunkProviderGenerateOld(worldIn);
            }
        }

        #region Save

        /// <summary>
        /// Начало сохранение чанков
        /// </summary>
        public void BeginSaving()
        {
            if (listSavingChunk.Count == 0)
            {
                // готовим список чанков для сохранения
                Dictionary<vec2i, ChunkBase>.ValueCollection chunks = chunkMapping.Values();
                foreach (ChunkBase chunk in chunks)
                {
                    listSavingChunk.Add(chunk);
                }
                flagSaveRegion = 1;
            }
        }

        /// <summary>
        /// Сохранение чанков в одном тике
        /// </summary>
        public void TickSaving()
        {
            if (flagSaveRegion == 1)
            {
                if (listSavingChunk.Count > 0)
                {
                    // Если были не сохранённые чанки сохраняем пакет из 50 чанков за тик
                    int i = 0;
                    bool b = true;
                    while (i < 50 && b)
                    {
                        if (listSavingChunk.Count == 0) b = false;
                        else if (SaveChunkData(listSavingChunk[0])) i++;
                    }
                }
                else
                {
                    flagSaveRegion = 2;
                }
            }
        }

        /// <summary>
        /// Сохранение регионов
        /// </summary>
        public void SavingRegions()
        {
            if (flagSaveRegion == 2)
            {
                worldServer.Regions.WriteToFile(false);
                flagSaveRegion = 0;
            }
        }

        /// <summary>
        /// Сохранить все чанки при закрытии мира
        /// </summary>
        public void SaveChunks()
        {
            Dictionary<vec2i, ChunkBase>.ValueCollection chunks = chunkMapping.Values();
            int i = 0;
            int s = 0;
            foreach (ChunkBase chunk in chunks)
            {
                if (SaveChunkData(chunk)) s++;
                i++;
            }
            worldServer.Log.Log("server.saving.chunk all count:{0} save count:{1}", i, s);
        }

        /// <summary>
        /// Сохранить основные данные чанка
        /// </summary>
        private bool SaveChunkData(ChunkBase chunk)
        {
            if (chunk != null)
            {
                try
                {
                    listSavingChunk.Remove(chunk);
                    return chunk.SaveFileChunk(worldServer);
                }
                catch (Exception ex)
                {
                    worldServer.Log.Error("Не удалось сохранить чанк [{2}] в регионе [{3}] {0}\r\n{1}",
                    ex.Message, ex.StackTrace,
                    chunk.Position, new vec2i(chunk.Position.x >> 5, chunk.Position.y >> 5));
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Загрузить чанк в лоадинге
        /// </summary>
        public ChunkBase LoadingChunk(vec2i pos)
        {
            ChunkBase chunk = GetChunk(pos);
            if (chunk == null)
            {
                chunk = new ChunkBase(world, pos);
                LoadOrGen(chunk);
                chunkMapping.Set(chunk);
                chunk.OnChunkLoad();
            }
            return chunk;
        }

        /// <summary>
        /// Загружаем, если нет чанка то генерируем
        /// </summary>
        /// <param name="chunk">Объект чанка не null</param>
        private void LoadOrGen(ChunkBase chunk)
        {
            if (!chunk.IsChunkPresent)
            {
                // Пробуем загрузить с файла
                try
                {
                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();
                    chunk.LoadFileChunk(worldServer);
                    // if (chunk.IsChunkLoaded) world.Log.Log("ChunkLoad[{1}]: {0:0.00} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, chunk.Position);
                }
                catch (Exception ex)
                {
                    worldServer.Log.Error("Не удалось прочесть чанк [{2}] в регионе [{3}] {0}\r\n{1}",
                        ex.Message, ex.StackTrace,
                        chunk.Position, new vec2i(chunk.Position.x >> 5, chunk.Position.y >> 5));
                }
                if (!chunk.IsChunkPresent)
                {
                    // Начинаем генерацию
                    ChunkGenerate.GenerateChunk(chunk);
                }
            }
        }

        /// <summary>
        /// Загрузка или генерация чанка, с пополнением его в карту чанков
        /// </summary>
        /// <param name="pos">Позиция чанка</param>
        public void LoadGenAdd(vec2i pos)
        {
            ChunkBase chunk = new ChunkBase(world, pos);
            chunkMapping.Set(chunk);
            LoadOrGen(chunk);
            chunk.OnChunkLoad();
        }

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
                    //Тут сохраняем чанк
                    SaveChunkData(chunk);
                    chunkMapping.Remove(pos);
                }
            }
        }

        /// <summary>
        /// Добавить чанк на удаление
        /// </summary>
        public void DropChunk(vec2i pos)
        {
            DroppedChunks.Add(pos);
        }

        /// <summary>
        /// Почистить чанки где нет игроков из-за не коректной выгрузки
        /// </summary>
        public void CleanChunksNotPlayer()
        {
            Dictionary<vec2i, ChunkBase>.ValueCollection chunks = chunkMapping.Values();
            foreach (ChunkBase chunk in chunks)
            {
                if (!worldServer.Players.IsTherePlayerChunk(chunk.Position))
                {
                    DropChunk(chunk.Position);
                }
            }
        }
    }
}
