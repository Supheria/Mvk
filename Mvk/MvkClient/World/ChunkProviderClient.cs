using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.Network.Packets.Client;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkClient.World
{
    /// <summary>
    /// Объект Клиент который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderClient : ChunkProvider
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }

        /// <summary>
        /// Список чанков Для удаления сетки основного потока
        /// </summary>
        private DoubleList<vec2i> remoteMeshChunks = new DoubleList<vec2i>();
        /// <summary>
        /// Список чанков на удаление
        /// </summary>
        private DoubleList<vec2i> remoteChunks = new DoubleList<vec2i>();
        /// <summary>
        /// Список чанков на дабавление
        /// </summary>
        private DoubleList<ChunkRender> addChunks = new DoubleList<ChunkRender>();
        /// <summary>
        /// Перезапуск чанки по итоку удаления (смена обзора к примеру)
        /// </summary>
        private bool isResetLoadingChunk = false;

        public ChunkProviderClient(WorldClient worldIn)
        {
            ClientWorld = worldIn;
            world = worldIn;
        }

        public void SetOverviewChunk()
        {
            isResetLoadingChunk = true;
        }

        /// <summary>
        /// Очистить все чанки, ТОЛЬКО для клиента
        /// </summary>
        /// <param name="reset">надо ли перезапустить чанки по итоку удаления (смена обзора к примеру)</param>
        public void ClearAllChunks(bool reset)
        {
            isResetLoadingChunk = reset;
            Dictionary<vec2i, ChunkBase>.ValueCollection chunks = chunkMapping.Values();
            foreach (ChunkRender chunk in chunks)
            {
                if ((reset && !chunk.Position.Equals(ClientWorld.ClientMain.Player.GetChunkPos())) || !reset)
                {
                    UnloadChunk(chunk);
                }
            }
        }

        /// <summary>
        /// Загрузить, если нет такого создаём для клиента
        /// </summary>
        public ChunkRender GetChunkRender(vec2i pos) => !(chunkMapping.Get(pos) is ChunkRender chunk) ? null : chunk;

        /// <summary>
        /// Заносим данные с пакета сервера
        /// </summary>
        public void PacketChunckData(ChunkQueue packet)
        {
            vec2i pos = packet.GetPos();
            //if ((pos.x == 2 && pos.y ==2) 
            //    || (pos.x == 1 && pos.y == 1) || (pos.x == -1 && pos.y == -4)) world.Log.Log("pc {0} {1} {2}", pos, packet.biom, packet.flagsYAreas);
            ChunkRender chunkRender;
            ChunkRender chunk = GetChunkRender(pos);
            if (chunk == null)
            {
                // Если чанка нет, проверяем в загрузке
                int i;
                ListMvk<ChunkRender> list;
                // lock не нужен, так как если прозеваем значение, оно уже будет в GetChunkRender
                // Достаточно пробежаться только по массиву Add
                list = addChunks.GetForward();
                for (i = 0; i < list.Count; i++)
                {
                    chunkRender = list[i];
                    if (chunkRender.Position.x == pos.x && chunkRender.Position.y == pos.y)
                    {
                        chunk = chunkRender;
                        break;
                    }
                }
            }

            // Ещё раз проверяем, чтоб не испльзовать lock на всю проверку
            if (chunk == null)
            {
                chunk = GetChunkRender(pos);
                if (chunk != null)
                {
                    // 2022-08-10 Ни разу покуда не зафиксировано, возможно ненужная проверка, надо проверить
                    world.Log.Log("ChunkProviderClient #104 !!! {0}", pos);
                }
            }
            bool isNew = chunk == null;

            if (packet.IsRemoved())
            {
                if (!isNew)
                {
                    // TODO::2022-08-09 не помню зя чем это, но с этим чанк не выгружается
                    //if (pos.x == 0 && pos.y == 0) return;
                    UnloadChunk(chunk);
                }
            }
            else
            {
                if (isNew) 
                {
                    chunk = new ChunkRender(ClientWorld, pos);
                }

                chunk.SetBinary(packet.GetBuffer(), packet.IsBiom(), packet.GetFlagsYAreas());

                if (isNew)
                {
                    // Для нового чанка у клиента, генерируем высотную карту
                    chunk.Light.GenerateHeightMap();
                    addChunks.Add(chunk);
                }
                else
                {
                    if (packet.biom) chunk.Light.GenerateHeightMap();
                }
            }
        }

        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(ChunkRender chunk)
        {
            if (chunk != null)
            {
                vec2i pos = chunk.Position;
                if (chunk.IsChunkLoaded)
                {
                    chunk.OnChunkUnload();
                    // заносим в массив чистки чанков по сетки для основного потока
                    remoteMeshChunks.Add(pos);
                }
                else
                {
                    remoteChunks.Add(pos);
                }
            }
        }

        /// <summary>
        /// Выгрузка загрузка чанков в тике
        /// </summary>
        public void ChunksTickUnloadLoad()
        {
            // Выгрузка чанков
            remoteChunks.Step();
            int count = remoteChunks.CountBackward;
            int i;
            for (i = 0; i < count; i++)
            {
                chunkMapping.Remove(remoteChunks.GetNext());
            }
           
            if (isResetLoadingChunk)
            {
                isResetLoadingChunk = false;
                //lock (locker) addChunks.Clear(); // 2022-08-09 зачем?! вроде без неё работает точно, скорее всего были лаги в потоках
                //ClientWorld.Player.UpFrustumCulling();
                ClientWorld.ClientMain.TrancivePacket(new PacketC15ClientSetting(ClientWorld.ClientMain.Player.OverviewChunk));
            }
            // Загрузка чанков
            ChunkRender chunkRender;
            addChunks.Step();
            count = addChunks.CountBackward;
            for (i = 0; i < count; i++)
            {
                chunkRender = addChunks.GetNext();
                if (chunkRender.IsChunkLoaded)
                {
                    chunkMapping.Set(chunkRender);
                    vec2i pos = chunkRender.Position;
                    // надо соседние чанки попросить перерендерить
                    ClientWorld.AreaModifiedToRender(pos.x - 1, 0, pos.y - 1, pos.x + 1, ChunkBase.COUNT_HEIGHT15, pos.y + 1);
                }
            }
        }

        /// <summary>
        /// Чистка сетки опенгл
        /// </summary>
        public void RemoteMeshChunks()
        {
            remoteMeshChunks.Step();
            int count = remoteMeshChunks.CountBackward;
            int i;
            vec2i pos;
            for (i = 0; i < count; i++)
            {
                pos = remoteMeshChunks.GetNext();
                remoteChunks.Add(pos);
                ChunkRender chunk = GetChunkRender(pos);
                if (chunk != null)
                {
                    if (!chunk.IsChunkLoaded) chunk.MeshDelete();
                }
            }
        }

        /// <summary>
        /// Перепроверить чанки игроков в попадание в обзоре, если нет, убрать
        /// для клиента
        /// Tick
        /// </summary>
        //public void FixOverviewChunk(EntityPlayer entity)
        //{
        //    // дополнительно к обзору для кэша из-за клона обработки, разных потоков
        //    int additional = 6;
        //    vec2i chunkCoor = entity.GetChunkPos();
        //    vec2i min = chunkCoor - (entity.OverviewChunk + additional);
        //    vec2i max = chunkCoor + (entity.OverviewChunk + additional);

        //    Hashtable ht = chunkMapping.CloneMap();
        //    foreach (ChunkRender chunk in ht.Values)
        //    {
        //        if (chunk.Position.x < min.x || chunk.Position.x > max.x || chunk.Position.y < min.y || chunk.Position.y > max.y)
        //        {
        //            UnloadChunk(chunk);
        //        }
        //    }
        //}

        /// <summary>
        /// Сделать запрос на обновление близ лежащих псевдо чанков для альфа блоков
        /// </summary>
        /// <param name="x">координата чанка X</param>
        /// <param name="y">координата псевдо чанка Y</param>
        /// <param name="z">координата чанка Z</param>
        public void ModifiedToRenderAlpha(int x, int y, int z)
        {
            ChunkRender chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x, z));
            if (chunk != null)
            {
                chunk.ModifiedToRenderAlpha(y);
                chunk.ModifiedToRenderAlpha(y - 1);
                chunk.ModifiedToRenderAlpha(y + 1);
            }
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x + 1, z));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x - 1, z));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x, z + 1));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x, z - 1));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
        }

        /**
     * Unloads chunks that are marked to be unloaded. This is not guaranteed to unload every such chunk.
     */
        //public bool UnloadQueuedChunks()
        //{
        //    long var1 = System.currentTimeMillis();

        //    Hashtable ht = chunkListing.CloneMap();
        //    foreach (ChunkRender chunk in ht.Values)
        //    {
        //        chunk.
        //        if ((reset && !chunk.Position.Equals(ClientWorld.ClientMain.Player.GetChunkPos())) || !reset)
        //        {
        //            UnloadChunk(chunk);
        //        }
        //    }


        //    Iterator var3 = chunkListing.iterator();

        //    while (var3.hasNext())
        //    {
        //        Chunk var4 = (Chunk)var3.next();
        //        var4.func_150804_b(System.currentTimeMillis() - var1 > 5L);
        //    }

        //    if (System.currentTimeMillis() - var1 > 100L)
        //    {
        //        logger.info("Warning: Clientside chunk ticking took {} ms", new Object[] { Long.valueOf(System.currentTimeMillis() - var1) });
        //    }

        //    return false;
        //}

        public override string ToString()
        {
            return "Ch:" + chunkMapping.Count + " RM:" + remoteMeshChunks.CountBackward + " R:" + remoteChunks.CountBackward + " A:" + addChunks.CountBackward;
        }
    }
}
