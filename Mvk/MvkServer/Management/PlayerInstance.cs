using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkServer.Management
{
    /// <summary>
    /// Ячейка чанка которую могут видеть игроки
    /// </summary>
    public class PlayerInstance
    {
        /// <summary>
        /// Количество блоков отправляемые за так изменённые в чанке
        /// </summary>
        private const int COUNT_MULTY_BLOCKS = 4096;
        /// <summary>
        /// Позиция текущего чанка
        /// </summary>
        public vec2i CurrentChunk { get; private set; }
        /// <summary>
        /// Объект игроков
        /// </summary>
        private PlayerManager playerManager;
        /// <summary>
        /// Список игроков которые имеют этот чанк в обзоре
        /// </summary>
        private List<EntityPlayerServer> playersWatchingChunk = new List<EntityPlayerServer>();
        /// <summary>
        /// Время сколько игроков провели времени на этом чанке
        /// </summary>
        private uint previousWorldTime;
        /// <summary>
        /// Сколько блоков надо обновить до COUNT_MULTY_BLOCKS шт
        /// </summary>
        private int numBlocksToUpdate;
        /// <summary>
        /// Массив каких блоков надо обновить
        /// </summary>
        private vec3i[] locationOfBlockChange = new vec3i[COUNT_MULTY_BLOCKS];
        /// <summary>
        /// Флаг какие псевдо чанки надо обновлять
        /// </summary>
        private int flagsYAreasToUpdate;

        public PlayerInstance(PlayerManager playerManager, vec2i pos)
        {
            this.playerManager = playerManager;
            CurrentChunk = pos;
        }

        /// <summary>
        /// Проверить имеется ли игрок в этом чанке
        /// </summary>
        public bool Contains(EntityPlayerServer player) => playersWatchingChunk.Contains(player);

        /// <summary>
        /// Добавить игрока в конкретный чанк
        /// </summary>
        /// <param name="isLoading">Нужно ли добавить в список загрузки чанков, для пометки сервера какие чанки проверить</param>
        /// <param name="isLoading">Нужно ли добавить в список загруженых чанков, для обзора чанка для клиента</param>
        public void AddPlayer(EntityPlayerServer player, bool isLoading, bool isLoaded)
        {
            if (!playersWatchingChunk.Contains(player))
            {
                if (playersWatchingChunk.Count == 0) previousWorldTime = playerManager.World.ServerMain.TickCounter;
                playersWatchingChunk.Add(player);
                if (isLoaded) player.LoadedChunks.Add(CurrentChunk);
                if (isLoading) player.LoadingChunks.Add(CurrentChunk);
            }
        }

        /// <summary>
        /// Убрать игрока с конкретного чанка
        /// </summary>
        public void RemovePlayer(EntityPlayerServer player, bool isServer)
        {
            if (playersWatchingChunk.Contains(player))
            {
                ChunkBase chunk = playerManager.World.GetChunk(CurrentChunk);

                // отправить игроку что чанк удалить
                //if (chunk.IsPopulated()) 
                if (chunk != null)
                {
                    //playerManager.World.Log.Log("Remov: {0}", CurrentChunk);
                    player.SendPacket(new PacketS21ChunkData(chunk, false, 0));
                }

                player.LoadedChunks.Remove(CurrentChunk);

                if (isServer)
                {
                    playersWatchingChunk.Remove(player);

                    if (playersWatchingChunk.Count == 0)
                    {
                        if (chunk != null) IncreaseInhabitedTime(chunk);
                        playerManager.PlayerInstancesRemove(CurrentChunk, this);
                        if (numBlocksToUpdate > 0)
                        {
                            playerManager.PlayerInstancesToUpdateRemove(this);
                        }
                        playerManager.World.ChunkPrServ.DropChunk(CurrentChunk);
                    }
                }
            }
        }

        /// <summary>
        /// Флаг блока который был изменён
        /// </summary>
        /// <param name="pos">локальные координаты xz 0..15, y 0..255</param>
        public void FlagBlockForUpdate(vec3i pos)
        {
            if (numBlocksToUpdate == 0)
            {
                playerManager.PlayerInstancesToUpdateAdd(this);
            }

            flagsYAreasToUpdate |= 1 << (pos.y >> 4);

            if (numBlocksToUpdate < COUNT_MULTY_BLOCKS)
            {
                for (int i = 0; i < numBlocksToUpdate; i++)
                {
                    if (locationOfBlockChange[i] == pos) return;
                }
                locationOfBlockChange[numBlocksToUpdate++] = pos;
            }
            else
            {
                // TODO::2022-09-27 надо отладить, добираемся ли мы до этого количества
                throw new System.Exception("PlayerInstance вышел за предел COUNT_MULTY_BLOCKS");
            }
        }

        /// <summary>
        /// Флаг псевдочанка который был изменён
        /// </summary>
        /// <param name="y"></param>
        //public void FlagChunkForUpdate(int y)
        //{
        //    if (numBlocksToUpdate == 0)
        //    {
        //        playerManager.PlayerInstancesToUpdateAdd(this);
        //    }

        //    numBlocksToUpdate = COUNT_MULTY_BLOCKS;
        //    flagsYAreasToUpdate |= 1 << y;
        //}

        /// <summary>
        /// Перепроверить чанки игроков в попадание в обзоре, если нет, убрать
        /// </summary>
        //public void FixOverviewChunk()
        //{
        //    List<EntityPlayerServer> list = playersWatchingChunk.GetRange(0, playersWatchingChunk.Count);
        //    foreach (EntityPlayerServer entityPlayer in list)
        //    {
        //        int radius = entityPlayer.OverviewChunk + 1;
        //        vec2i min = entityPlayer.ChunkPosManaged - radius;
        //        vec2i max = entityPlayer.ChunkPosManaged + radius;
        //        if (CurrentChunk.x < min.x || CurrentChunk.x > max.x || CurrentChunk.y < min.y || CurrentChunk.y > max.y)
        //        {
        //            RemovePlayer(entityPlayer);
        //        }
        //    }
        //}

        /// <summary>
        /// Отправить всем игрокам пакет, которые имеется этот чанк
        /// </summary>
        public void SendToAllPlayersWatchingChunk(IPacket packet)
        {
            for (int i = 0; i < playersWatchingChunk.Count; i++)
            {
                playersWatchingChunk[i].SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить всем игрокам пакет, которые имеется этот чанк, с проверкой что он уже загружен
        /// </summary>
        public void SendToAllPlayersWatchingChunkCheck(IPacket packet, vec2i pos)
        {
            for (int i = 0; i < playersWatchingChunk.Count; i++)
            {
                if (!playersWatchingChunk[i].LoadedChunks.Contains(pos))
                {
                    playersWatchingChunk[i].SendPacket(packet);
                }
            }
        }

        /// <summary>
        /// Получить позицию блока по локальной позиции 0..15
        /// </summary>
        private BlockPos GetBlockPos(vec3i pos0) => new BlockPos(pos0.x + (CurrentChunk.x << 4), pos0.y, pos0.z + (CurrentChunk.y << 4));

        /// <summary>
        /// Обновление в игровом такте
        /// </summary>
        public void Update()
        {
            if (numBlocksToUpdate != 0)
            {
                if (numBlocksToUpdate == 1)
                {
                    // Заменён один блок
                    BlockPos blockPos = GetBlockPos(locationOfBlockChange[0]);
                    SendToAllPlayersWatchingChunk(new PacketS23BlockChange(playerManager.World, blockPos));

                    // Тайлы
                    //if (playerManager.World.GetBlock(blockPos).HasTileEntity())
                    //{
                    //    SendTileToAllPlayersWatchingChunk(playerManager.World.GetTileEntity(blockPos));
                    //}
                }
                else if (numBlocksToUpdate < COUNT_MULTY_BLOCKS)
                {
                    // Заменён 2 - COUNT_MULTY_BLOCKS блока
                    SendToAllPlayersWatchingChunk(new PacketS22MultiBlockChange(numBlocksToUpdate, locationOfBlockChange, playerManager.World.GetChunk(CurrentChunk)));

                    // Тайлы
                    //for (int i = 0; i < numBlocksToUpdate; i++)
                    //{
                    //    BlockPos blockPos = GetBlockPos(locationOfBlockChange[i]);
                    //    if (playerManager.World.GetBlock(blockPos).HasTileEntity())
                    //    {
                    //        SendTileToAllPlayersWatchingChunk(playerManager.World.GetTileEntity(blockPos));
                    //    }
                    //}
                }
                else
                {
                    // больше COUNT_MULTY_BLOCKS
                    SendToAllPlayersWatchingChunkCheck(new PacketS21ChunkData(playerManager.World.GetChunk(CurrentChunk), false, flagsYAreasToUpdate), CurrentChunk);

                    // Тайлы
                    //int x0 = CurrentChunk.x << 4;
                    //int z0 = CurrentChunk.y << 4;
                    //for (int y0 = 0; y0 < 16; y0++)
                    //{
                    //    if ((flagsYAreasToUpdate & 1 << y0) != 0)
                    //    {
                    //        int y = y0 << 4;
                    //        List var5 = playerManager.World.func_147486_a(x0, y, z0, x0 + 16, y + 16, z0 + 16);

                    //        for (int var6 = 0; var6 < var5.size(); ++var6)
                    //        {
                    //            SendTileToAllPlayersWatchingChunk((TileEntity)var5.get(var6));
                    //        }
                    //    }
                    //}
                }
                numBlocksToUpdate = 0;
                flagsYAreasToUpdate = 0;
            }
        }

        public void ProcessChunk()
        {
            IncreaseInhabitedTime(playerManager.World.GetChunk(CurrentChunk));
            //    FixOverviewChunk();
            //    if (CountPlayers() == 0)
            //    {
            //        playerInstances.Remove(ccp.Position);
            //        world.ChunkPrServ.DroppedChunks.Add(ccp.Position);
            //    }
            //// Добавить в список удаляющих чанков которые не полного статуса
            //    world.ChunkPrServ.DroopedChunkStatusMin(players);
        }

        private void IncreaseInhabitedTime(ChunkBase chunk)
        {
            if (chunk != null)
            {
                uint time = playerManager.World.ServerMain.TickCounter;
                chunk.SetInhabitedTime(chunk.InhabitedTime + time - previousWorldTime);
                previousWorldTime = time;
            }
        }

        /// <summary>
        /// Количество игроков в этом чанке
        /// </summary>
        public int CountPlayers() => playersWatchingChunk.Count;

        /// <summary>
        /// Отправить пакет тайлов
        /// </summary>
        //private void SendTileToAllPlayersWatchingChunk(TileEntity tile)
        //{
        //    if (tile != null)
        //    {
        //        IPacket packet = tile.GetDescriptionPacket();
        //        if (packet != null)
        //        {
        //            SendToAllPlayersWatchingChunk(packet);
        //        }
        //    }
        //}
    }
}
