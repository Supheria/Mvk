using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Management;
using MvkServer.Network;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.TileEntity;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;

namespace MvkServer.World
{
    /// <summary>
    /// Серверный объект мира
    /// </summary>
    public class WorldServer : WorldBase
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }
        /// <summary>
        /// Информация мира
        /// </summary>
        public WorldInfo Info { get; private set; }
        /// <summary>
        /// Объект работы с файлами мира для сохранения
        /// </summary>
        public WorldFile File { get; private set; }
        /// <summary>
        /// Объект клиентов
        /// </summary>
        public PlayerManager Players { get; protected set; }
        /// <summary>
        /// Трекер сущностей
        /// </summary>
        public EntityTracker Tracker { get; protected set; }
        /// <summary>
        /// Регионы для хранения данных чанков
        /// </summary>
        public RegionProvider Regions { get; private set; }

        /// <summary>
        /// Счётчик для обновления сущностей
        /// </summary>
        private int updateEntityTick = 0;

        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        public ChunkProviderServer ChunkPrServ => ChunkPr as ChunkProviderServer;
        
        public WorldServer(Server server, byte slot, long seed, string name) : base()
        {
            IsRemote = false;
            ServerMain = server;
            File = new WorldFile(this, slot);
            Info = new WorldInfo(this, seed, name);
            Rnd = new Rand(Info.Seed);
            ChunkPr = new ChunkProviderServer(this, slot);
            Players = new PlayerManager(this);
            Tracker = new EntityTracker(this);
            Log = ServerMain.Log;
            profiler = new Profiler(Log);
            Regions = new RegionProvider(this);
        }

        /// <summary>
        /// Остановка мира, начинаем сохранять
        /// </summary>
        public void WorldStoping()
        {
            Info.WriteInfo();
            Players.PlayersRemoveStopingServer();
            Players.Update();
        }

        /// <summary>
        /// Игровое время
        /// </summary>
        public override uint GetTotalWorldTime() => ServerMain.TickCounter;

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            CalculateInitialYear();
            CalculateInitialSkylight();

            profiler.StartSection("PlayersTick");
            Players.Update();

            profiler.EndStartSection("WorldTick");
            base.Tick();

            profiler.EndStartSection("MobSpawner");

            profiler.EndStartSection("ChunkSource");
            ChunkPrServ.UnloadQueuedChunks();
            //profiler.EndStartSection("CheckAddGeneration");
            //ChunkPrServ.UpdateCheckAddGeneration();
            profiler.EndStartSection("TickBlocks");
            TickBlocks();
            profiler.EndStartSection("ChunkMap");
            Players.UpdatePlayerInstances();
            profiler.EndStartSection("Village");
            //this.villageCollectionObj.tick();
            //this.villageSiege.tick();
            profiler.EndSection();
        }

        /// <summary>
        /// Тикание блоков и чанков
        /// </summary>
        private void TickBlocks()
        {
            SetActivePlayerChunksAndCheckLight();
            // цикл активных чанков
            int count = activeChunkSet.Count;
            int randomTickSpeed = MvkGlobal.RANDOM_TICK_SPEED;
            ChunkBase chunk;
            ChunkStorage chunkStorage;
            int yc, i, j, x, y, z, k, xc0, yc0;
            BlockPos blockPos = new BlockPos();
            BlockState blockState;
            BlockBase block;

            for (i = 0; i < count; i++) 
            {
                profiler.StartSection("GetChunk");
                chunk = GetChunk(activeChunkSet[i]);
                
                if (chunk != null && chunk.IsSendChunk)
                {
                    xc0 = chunk.Position.x << 4;
                    yc0 = chunk.Position.y << 4;
                    profiler.EndStartSection("TickChunk");
                    chunk.UpdateServer();
                    profiler.EndStartSection("TickBlocks");

                    if (randomTickSpeed > 0)
                    {
                        for (yc = 0; yc < ChunkBase.COUNT_HEIGHT; yc++)
                        {
                            chunkStorage = chunk.StorageArrays[yc];
                            if (!chunkStorage.IsEmptyData() && chunkStorage.GetNeedsRandomTick())
                            {
                                for (j = 0; j < randomTickSpeed; j++)
                                {
                                    updateLCG = updateLCG * 3 + 1013904223;
                                    k = updateLCG >> 2;
                                    x = k & 15;
                                    z = k >> 8 & 15;
                                    y = k >> 16 & 15;
                                    blockPos.X = x + xc0;
                                    blockPos.Y = y + chunkStorage.GetYLocation();
                                    blockPos.Z = z + yc0;
                                    blockState = chunkStorage.GetBlockState(x, y, z);
                                    block = blockState.GetBlock();
                                    if (block.NeedsRandomTick)
                                    {
                                        block.RandomTick(this, blockPos, blockState, Rnd);
                                        if (chunkStorage.IsEmptyData() || !chunkStorage.GetNeedsRandomTick())
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                profiler.EndSection();
            }

        }

        //private void UpdateActiveChunks()
        //{
        //    for (int i = 0; i < PlayerEntities.Count; i++)
        //    {
        //        EntityBase entity = PlayerEntities.GetAt(i);
        //        vec3i pos = entity.GetBlockPos();
        //        if (IsAreaLoaded(pos.x - 16, pos.y - 16, pos.z - 16, pos.x + 16, pos.y + 16, pos.z + 16))
        //        {
        //            ChunkBase chunk = GetChunk(entity.GetChunkPos());
        //            chunk.Update();
        //        }

        //    }

        //    //ChunkPrServ.
        //}

        /// <summary>
        /// Задать тик блока
        /// </summary>
        public override void SetBlockTick(BlockPos blockPos, uint timeTackt, bool priority = false)
        {
            if (!blockPos.IsValid()) return;
            ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            if (chunk != null) chunk.SetBlockTick(blockPos.X & 15, blockPos.Y, blockPos.Z & 15, timeTackt, priority);
        }

        /// <summary>
        /// Отметить блок для обновления 
        /// </summary>
        public override void MarkBlockForRenderUpdate(int x, int y, int z) => Players.FlagBlockForUpdate(x, y, z);

        /// <summary>
        /// Отметить блоки на обновления, сервер. Координаты чанков
        /// </summary>
        //public void ServerMarkBlockRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        //{
        //    int x, y, z;
        //    for (x = x0; x <= x1; x++)
        //    {
        //        for (z = z0; z <= z1; z++)
        //        {
        //            for (y = y0; y <= y1; y++)
        //            {
        //                Players.FlagBlockForUpdate(x, y, z);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Отметить псевдочанки на обновления, сервер. Координаты чанков
        /// </summary>
        public void ServerMarkChunkRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int x, y, z;
            vec2i ch;
            for (x = x0; x <= x1; x++)
            {
                for (z = z0; z <= z1; z++)
                {
                    ch = new vec2i(x, z);
                    for (y = y0; y <= y1; y++)
                    {
                        Players.FlagChunkForUpdate(ch, y);
                    }
                }
            }
        }
        /// <summary>
        /// Отметить  блоки для обновления
        /// </summary>
        //public override void MarkBlockRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        //{
        //    int x, y, z;
        //    for (x = x0; x <= x1; x++)
        //    {
        //        for (z = z0; z <= z1; z++)
        //        {
        //            for (y = y0; y <= y1; y++)
        //            {
        //                Players.FlagBlockForUpdate(x, y, z);
        //            }
        //        }
        //    }

        //    //int c0x = (x0) >> 4;
        //    //int c0y = (y0) >> 4;
        //    //if (c0y < 0) c0y = 0;
        //    //int c0z = (z0) >> 4;
        //    //int c1x = (x1) >> 4;
        //    //int c1y = (y1) >> 4;
        //    //if (c1y > ChunkBase.COUNT_HEIGHT15) c1y = ChunkBase.COUNT_HEIGHT15;
        //    //int c1z = (z1) >> 4;
        //    //vec2i ch;
        //    //int x, y, z;
        //    //for (x = c0x; x <= c1x; x++)
        //    //{
        //    //    for (z = c0z; z <= c1z; z++)
        //    //    {
        //    //        ch = new vec2i(x, z);
        //    //        for (y = c0y; y <= c1y; y++)
        //    //        {
        //    //            Players.FlagBlockForUpdate(ch, y);
        //    //        }
        //    //    }
        //    //}
        //}

        /// <summary>
        /// Отметить блоки для изминения
        /// </summary>
        public override void MarkBlockRangeForModified(int x0, int z0, int x1, int z1)
        {
            int c0x = (x0) >> 4;
            int c0z = (z0) >> 4;
            int c1x = (x1) >> 4;
            int c1z = (z1) >> 4;
            ChunkBase chunk;
            int x, z;
            for (x = c0x; x <= c1x; x++)
            {
                for (z = c0z; z <= c1z; z++)
                {
                    chunk = ChunkPr.GetChunk(new vec2i(x, z));
                    if (chunk != null) chunk.Modified();
                }
            }
        }

        #region Entity

        /// <summary>
        /// Обновляет (и очищает) объекты и объекты чанка 
        /// </summary>
        public override void UpdateEntities()
        {
            // Для мира где нет игроков или в перспективе если только сервер, 
            // чтоб не запускать обработчик после минуты
            if (Players.IsEmpty())
            {
                if (updateEntityTick++ >= 1200) return;
            }
            else
            {
                updateEntityTick = 0;
            }

            base.UpdateEntities();
        }

        /// <summary>
        /// Обновить трекер сущностей
        /// </summary>
        public void UpdateTrackedEntities()
        {
            profiler.StartSection("Tracker");
            Tracker.UpdateTrackedEntities();
            profiler.EndSection();
        }

        protected override void OnEntityAdded(EntityBase entity)
        {
            base.OnEntityAdded(entity);
            Tracker.EntityAdd(entity);
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        protected override void OnEntityRemoved(EntityBase entity)
        {
            base.OnEntityRemoved(entity);
            Tracker.UntrackEntity(entity);
        }

        #endregion

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public override void SendBlockBreakProgress(int breakerId, BlockPos pos, int progress) 
            => Players.SendBlockBreakProgress(breakerId, pos, progress);

        /// <summary>
        /// Проиграть звуковой эффект, глобальная координата
        /// </summary>
        public override void PlaySound(EntityLiving entity, AssetsSample key, vec3 pos, float volume, float pitch)
        {
            Tracker.SendToAllTrackingEntity(entity, new PacketS29SoundEffect(key, pos, volume, pitch));
        }

        /// <summary>
        /// Проиграть звуковой эффект, глобальная координата
        /// </summary>
        public override void PlaySound(AssetsSample key, vec3 pos, float volume, float pitch)
        {
            if (key != AssetsSample.None)
            {
                Tracker.SendToAllEntityDistance(pos, 48, new PacketS29SoundEffect(key, pos, volume, pitch));
            }
        }

        /// <summary>
        /// Заспавнить частицу
        /// </summary>
        public override void SpawnParticle(EnumParticle particle, int count, vec3 pos, vec3 offset, float motion,  params int[] items)
        {
            Tracker.SendToAllEntityDistance(pos, 32f, new PacketS2AParticles(particle, count, pos, offset, motion, items));
        }

        /// <summary>
        /// Создать взрыв
        /// </summary>
        /// <param name="pos">позиция эпицентра</param>
        /// <param name="strength">сила</param>
        /// <param name="distance">дистанция</param>
        public override void CreateExplosion(vec3 pos, float strength, float distance)
        {
            explosion.SetExplosion(pos, strength, distance);
            profiler.StartSection("Explosion");
            explosion.DoExplosion();
            profiler.EndSection();

            foreach (KeyValuePair<EntityPlayer, vec3> entity in explosion.MotionPlayer)
            {
                ((EntityPlayerServer)entity.Key).SendPacket(new PacketS27Explosion(pos, strength, distance, entity.Value));
            }
        }

        /// <summary>
        /// Удалить сущность плитки
        /// </summary>
        public override void RemoveTileEntity(BlockPos pos)
        {
            TileEntityBase tileEntity = GetTileEntity(pos);
            if (tileEntity != null)
            {
                tileEntity.SpawnAsEntityOnBreakBlock();
                GetChunk(pos).RemoveTileEntity(pos.X & 15, pos.Y, pos.Z & 15);
                // надо закрыть окна у игроков которые смотрят этот блок
                Players.SendToAllPlayersUseTileEntity(
                    new PacketS31WindowProperty(PacketS31WindowProperty.EnumAction.CloseWindow),
                    //TODO::2023-08-03 можно подумать окно оповещения типа блок был разрушен! Чтоб курсор не менялся.
                    //new PacketS2DOpenWindow(EnumWindowType.CraftFirst),
                    pos, true);
            }
        }

        /// <summary>
        /// Пометка что чанк надо будет перезаписать
        /// </summary>
        public override void ChunkModified(BlockPos blockPos)
        {
            ChunkBase chunk = GetChunk(blockPos);
            if (chunk != null)
            {
                chunk.Modified();
            }
        }

        /// <summary>
        /// Отправить всем игрокам пакет, которые используют этот TileEntity
        /// </summary>
        public override void SendToAllPlayersUseTileEntity(IPacket packet, BlockPos blockPos) 
            => Players.SendToAllPlayersUseTileEntity(packet, blockPos);

        /// <summary>
        /// Отправить всем игрокам список крафта, которые используют этот TileEntity
        /// </summary>
        public override void SendToAllPlayersUseTileEntityListCraft(BlockPos blockPos)
            => Players.SendToAllPlayersUseTileEntityListCraft(blockPos);

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            try
            {
                int chBt = 0;
                EntityPlayerServer entityPlayerServer = Players.GetEntityPlayerMain();
                ChunkBase chunk = GetChunk(entityPlayerServer.PositionChunk);
                chBt = chunk.GetTickBlockCount();
                string tracker = "";// Tracker.ToString(); 
                EnumMoonPhase moonPhase = GetMoonPhase();
                string moon = moonPhase == EnumMoonPhase.AgingMoon ? "(|" : moonPhase == EnumMoonPhase.GrowingMoon
                    ? "|)" : moonPhase == EnumMoonPhase.FullMoon ? "()" : "|";
                return string.Format("R {6} Ch {0}-{2} EPl {1} E: {4} ChBt: {7}\r\n{3} {5}\r\nYear:{9} {10} {11} {12} Sky:{8}  ",
                    ChunkPr.Count, Players.PlayerCount, Players.CountPlayerInstances(), Players.ToStringDebug() // 0 - 3
                    , base.ToStringDebug(), tracker, Regions.Count(), // 4 - 6
                    chBt, skylightSubtracted, // 7 - 8
                    GetYear(), GetTimeYear(), moon, GetDay() // 9 - 11
                    );

            }
            catch(Exception e)
            {
                return "error: " + e.Message;
            }
        }
    }
}
