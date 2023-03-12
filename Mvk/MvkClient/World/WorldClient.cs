﻿using MvkClient.Actions;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Entity;
using MvkClient.Setitings;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MvkClient.World
{
    /// <summary>
    /// Клиентский объект мира
    /// </summary>
    public class WorldClient : WorldBase
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Посредник клиентоского чанка
        /// </summary>
        public ChunkProviderClient ChunkPrClient => ChunkPr as ChunkProviderClient;
        /// <summary>
        /// Мир для рендера и прорисовки
        /// </summary>
        public WorldRenderer WorldRender { get; protected set; }
        /// <summary>
        /// Менеджер прорисовки сущностей
        /// </summary>
        public RenderManager RenderEntityManager { get; protected set; }
        /// <summary>
        /// Список все объекты для этого клиента, как порожденные, так и непорожденные
        /// </summary>
        public MapListEntity EntityList { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Содержит все объекты для этого клиента, которые не были созданы из-за отсутствия фрагмента. 
        /// Игра будет пытаться создать до 10 ожидающих объектов с каждым последующим тиком, 
        /// пока очередь появления не опустеет. 
        /// </summary>
        public MapListEntity EntitySpawnQueue { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Список сущностей игроков
        /// </summary>
      //  public Hashtable PlayerEntities { get; protected set; } = new Hashtable();
        /// <summary>
        /// Объект нажатия клавиатуры
        /// </summary>
        public Keyboard Key { get; protected set; }

        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
        // protected InterpolationTime interpolation = new InterpolationTime();
        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;
        /// <summary>
        /// Количество прорисованных сущностей, для отладки
        /// </summary>
        protected int entitiesCountShow = 0;

        /// <summary>
        /// Флаг потока пакетов чанка
        /// </summary>
        //private bool packetChunkLoopRunning = false;
        /// <summary>
        /// Массив очередей чанков для рендера
        /// </summary>
        //private DoubleList<ChunkQueue> packetChunkQueues = new DoubleList<ChunkQueue>();

        public WorldClient(Client client) : base()
        {
            ChunkPr = new ChunkProviderClient(this);
            ClientMain = client;
           // interpolation.Start();
            WorldRender = new WorldRenderer(this);
            RenderEntityManager = new RenderManager(this);
            ClientMain.PlayerCreate(this);
            ClientMain.Player.SetOverviewChunk(Setting.OverviewChunk);
            Key = new Keyboard(this);

            Log = new Logger("Client");
            Log.Log("client.start");

            // Запускаем отдельный поток для чанков
            //packetChunkLoopRunning = true;
            //Thread myThread = new Thread(PacketChunkLoop);
            //myThread.Start();
        }

        /// <summary>
        /// Остановить поток рендера чанков
        /// </summary>
        //public void StopChunkLoop() => packetChunkLoopRunning = false;

        //public void AddPacketChunkQueue(PacketS21ChunkData packet)
        //{
        //    packetChunkQueues.Add(new ChunkQueue()
        //    {
        //        pos = packet.GetPos(),
        //        buffer = packet.GetBuffer(),
        //        flagsYAreas = packet.GetFlagsYAreas(),
        //        biom = packet.IsBiom()
        //    });
        //}

        /// <summary>
        /// Поток пакетов чанка
        /// </summary>
        //private void PacketChunkLoop()
        //{
        //    try
        //    {
        //        int count, i;
        //        while (packetChunkLoopRunning)
        //        {
        //            packetChunkQueues.Step();
        //            count = packetChunkQueues.CountBackward;
        //            for (i = 0; i < count; i++)
        //            {
        //                ChunkPrClient.PacketChunckData(packetChunkQueues.GetNext());
        //                if (!packetChunkLoopRunning) break;
        //            }
        //            Thread.Sleep(1);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Crach(ex);
        //    }
        //}

        /// <summary>
        /// Игровое время
        /// </summary>
        public override uint GetTotalWorldTime() => ClientMain.TickCounter;

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            try
            {
               // interpolation.Restart();
                uint time = ClientMain.TickCounter;

                ClientMain.Sample.Tick();
                Debug.strSound = ClientMain.Sample.StrDebug;

                base.Tick();
                // Добавляем спавн новых сущностей
                while (EntitySpawnQueue.Count > 0) // count < 10 сделать до 10 сущностей в такт
                {
                    EntityBase entity = EntitySpawnQueue.FirstRemove();

                    if (!LoadedEntityList.ContainsValue(entity))
                    {
                        SpawnEntityInWorld(entity);
                    }
                }


                // Дополнительная чистка, если какие-то чанки не почистились!
                //if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
                //{
                //    previousTotalWorldTime = time;
                //    ChunkPrClient.FixOverviewChunk(ClientMain.Player);
                //}

                // Выгрузка чанков в тике
                ChunkPrClient.ChunksTickUnloadLoad();
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Проверить загружены ли все ближ лижащие чанки кроме центра
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        public bool IsChunksSquareLoaded(vec2i pos)
        {
            for (int i = 0; i < MvkStatic.AreaOne8.Length; i++)
            {
                ChunkRender chunk = ChunkPrClient.GetChunkRender(pos + MvkStatic.AreaOne8[i]);
                if (chunk == null || !chunk.IsChunkPresent) return false;
            }
            return true; 
        }

        /// <summary>
        /// Остановка мира, удаляем все элементы
        /// </summary>
        public void StopWorldDelete()
        {
            Log.Log("client.stoped");
            Log.Close();
            ChunkPrClient.ClearAllChunks(false);
        }

        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// где 0 это начало, 1 финиш
        /// </summary>
        public float Interpolation() => ClientMain.Interpolation();

        /// <summary>
        /// Получить объект игрока по сети, по имени
        /// </summary>
        //public EntityPlayerMP GetPlayerMP(ushort id) => PlayerEntities.Get(id) as EntityPlayerMP;

        /// <summary>
        /// Возвращает сущностьь с заданным идентификатором или null, если он не существует в этом мире.
        /// </summary>
        public EntityBase GetEntityByID(ushort id)
        {
            if (id == ClientMain.Player.Id) return ClientMain.Player;
            return LoadedEntityList.Get(id);
        }

        /// <summary>
        /// Возвращает сущностьь с заданным идентификатором или null, если он не существует в этом мире.
        /// </summary>
        public EntityLiving GetEntityLivingByID(ushort id)
        {
            EntityBase entity = GetEntityByID(id);
            if (entity != null && entity is EntityLiving)
            {
                return (EntityLiving)entity;
            }
            return null;
        }

        /// <summary>
        /// Добавить сопоставление идентификатора сущности с entityHashSet
        /// </summary>
        public void AddEntityToWorld(ushort id, EntityBase entity)
        {
            EntityBase entityId = GetEntityByID(id);
            if (entityId == null)
            {
                SpawnEntityInWorld(entity);
            }

            //if (entityId != null) RemoveEntity(entityId);

            ////EntityList.Add(entity);
            ////entity.SetEntityId(id);

            //SpawnEntityInWorld(entity);
            //if (!SpawnEntityInWorld(entity))
            //{
            //    EntitySpawnQueue.Add(entity);
            //}
            //LoadedEntityList.Add(entity);
        }

        public EntityBase RemoveEntityFromWorld(ushort id)
        {
            EntityBase entity = GetEntityByID(id);
            if (entity != null)
            {
                EntityList.Remove(entity);
                RemoveEntity(entity);
            }

            return entity;
        }

        public void MouseDown(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                ClientMain.Player.HandAction();
            }
            else if(button == MouseButton.Right)
            {
                ClientMain.Player.HandActionRight();
            }
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button)
        {
            ClientMain.Player.UndoHandAction();
            if (button == MouseButton.Right)
            {
                ClientMain.Player.OnStoppedUsingItem();
            }
        }

        /// <summary>
        /// Сменить блок, без проверок, прямой доступ с сервера
        /// </summary>
        public void SetBlockStateClient(BlockPos blockPos, BlockState blockState)
        {
            if (blockPos.IsValid())
            {
                SetBlockState(blockPos, blockState, 4);
                /*ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    chunk.SetBlockStateClient(blockPos, blockState);
                    // Для рендера, проверка соседнего чанка, если блок крайний,
                    // то будет доп рендер чанков рядом
                    vec3i min = blockPos.ToVec3i() - 1;
                    vec3i max = blockPos.ToVec3i() + 1;
                    AreaModifiedToRender(min.x >> 4, min.y >> 4, min.z >> 4, max.x >> 4, max.y >> 4, max.z >> 4);
                }*/
            }
        }

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="eBlock">тип блока</param>
        /// <returns>true смена была</returns>
        public override bool SetBlockState(BlockPos blockPos, BlockState blockState, int flag)
        {
            Debug.DStart = true;
            //ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            //if (chunk != null)
            //{
            //    int bx = blockPos.X & 15;
            //    int by = blockPos.Y;
            //    int bz = blockPos.Z & 15;

            //    chunk.StorageArrays[by >> 4].Set(bx, by & 15, bz, blockState);

            //    MarkBlockForUpdate(blockPos);
            //}

            long l = GLWindow.stopwatch.ElapsedTicks;
            bool result = base.SetBlockState(blockPos, blockState, flag);
            long l2 = GLWindow.stopwatch.ElapsedTicks;
            Debug.DFloat = (l2 - l) / (float)MvkStatic.TimerFrequency;
            // Это если обновлять сразу!
            //if (result)
            //{
            //    //blockState.GetBlock().PlayPut(this, blockPos);
            //    // Для рендера, проверка соседнего чанка, если блок крайний,
            //    // то будет доп рендер чанков рядом
            //    vec3i min = blockPos.ToVec3i() - 1;
            //    vec3i max = blockPos.ToVec3i() + 1;

            //    AreaModifiedToRender(min.x >> 4, min.y >> 4, min.z >> 4, max.x >> 4, max.y >> 4, max.z >> 4);
            //}

            return true;
            //return false;
        }

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        public override void MarkBlockForRenderUpdate(int x, int y, int z) 
            => AreaModifiedToRender((x - 1) >> 4, (y - 1) >> 4, (z - 1) >> 4,
                (x + 1) >> 4, (y + 1) >> 4, (z + 1) >> 4);

        /// <summary>
        /// Отметить блоки для обновления
        /// </summary>
        public override void MarkBlockRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1) 
            => AreaModifiedToRender((x0 - 1) >> 4, (y0 - 1) >> 4, (z0 - 1) >> 4,
                (x1 + 1) >> 4, (y1 + 1) >> 4, (z1 + 1) >> 4);

        /// <summary>
        /// Сделать запрос перерендера выбранной облости псевдочанков
        /// </summary>
        public void AreaModifiedToRender(int c0x, int c0y, int c0z, int c1x, int c1y, int c1z)
        {
            int x, y, z;
            if (c0y < 0) c0y = 0;
            if (c1y > ChunkBase.COUNT_HEIGHT15) c1y = ChunkBase.COUNT_HEIGHT15;
            for (x = c0x; x <= c1x; x++)
            {
                for (z = c0z; z <= c1z; z++)
                {
                    ChunkRender chunk = ChunkPrClient.GetChunkRender(new vec2i(x, z));
                    if (chunk != null)
                    {
                        chunk.ModifiedToRender();
                    }
                }
            }
        }

        /// <summary>
        /// Пометить псевдо чанка на перегенерацию
        /// </summary>
        //public void ModifiedToRender(vec3i pos)
        //{
        //    if (pos.y >= 0 && pos.y < ChunkRender.COUNT_HEIGHT)
        //    {
        //        ChunkRender chunk = ChunkPrClient.GetChunkRender(new vec2i(pos.x, pos.z));
        //        if (chunk != null) chunk.ModifiedToRender(pos.y);
        //    }
        //}

        /// <summary>
        /// Получить попадает ли в луч сущность, выбрать самую близкую
        /// </summary>
        //public MovingObjectPosition RayCastEntity()
        //{
        //    float timeIndex = Interpolation();
        //    MovingObjectPosition moving = new MovingObjectPosition();
        //    if (ClientMain.Player.EntitiesLook.Length > 0)
        //    {
        //        EntityPlayerMP[] entities = ClientMain.Player.EntitiesLook.Clone() as EntityPlayerMP[];
        //        vec3 pos = ClientMain.Player.GetPositionFrame(timeIndex);
        //        float dis = 1000f;
        //        foreach (EntityPlayerMP entity in entities)
        //        {
        //            float disR = glm.distance(pos, entity.GetPositionFrame(timeIndex));
        //            if (dis > disR)
        //            {
        //                dis = disR;
        //                moving = new MovingObjectPosition(entity);
        //            }
        //        }
        //    }
        //    return moving;
        //}

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public override void SendBlockBreakProgress(int breakerId, BlockPos blockPos, int progress)
        {
            ChunkRender chunk = ChunkPrClient.GetChunkRender(blockPos.GetPositionChunk());
            if (chunk != null)
            {
                if (progress >= 0 && progress < 10)
                {
                    chunk.DestroyBlockSet(breakerId, blockPos, progress);
                    ParticleDiggingBlock(GetBlockState(blockPos).GetBlock(), blockPos, 1);
                }
                else
                {
                    if (progress == -2)
                    {
                        // Блок сломан
                       // ParticleDiggingBlock(blockPos, 50);
                        //SpawnEntityInWorld(new EntityItem(this, blockPos.ToVec3(), new MvkServer.Item.ItemStack()
                    }
                    chunk.DestroyBlockRemove(breakerId);
                }
                chunk.ModifiedToRender();
            }
        }

        ///// <summary>
        ///// Частички блока
        ///// </summary>
        ///// <param name="blockPos">позиция где рассыпаются частички</param>
        ///// <param name="count">количество частичек</param>
        //public void ParticleDiggingBlock(BlockPos blockPos, int count)
        //{
        //    BlockBase block = GetBlockState(blockPos).GetBlock();
        //    if (block != null && block.IsParticle)
        //    {
        //        vec3 pos = blockPos.ToVec3() + new vec3(.5f);
        //        for (int i = 0; i < count; i++)
        //        {
        //            SpawnParticle(EnumParticle.Digging,
        //                pos + new vec3((Rand.Next(16) - 8) / 16f, (Rand.Next(12) - 6) / 16f, (Rand.Next(16) - 8) / 16f),
        //                new vec3(0),
        //                (int)block.EBlock);
        //        }
        //    }
        //}

        /// <summary>
        /// Помечаем на перерендер всех псевдочанков обзора
        /// </summary>
        public void RerenderAllChunks()
        {
            if (ClientMain.Player.DistSqrt != null)
            {
                for (int i = 0; i < ClientMain.Player.DistSqrt.Length; i++)
                {
                    vec2i coord = new vec2i(Mth.Floor(ClientMain.Player.Position.x) >> 4, Mth.Floor(ClientMain.Player.Position.z) >> 4)
                        + ClientMain.Player.DistSqrt[i];
                    ChunkRender chunk = ChunkPrClient.GetChunkRender(coord);
                    if (chunk != null)
                    {
                        chunk.ModifiedToRender();
                    }
                }
            }
        }

        #region Entity

        protected override void OnEntityAdded(EntityBase entity)
        {
            base.OnEntityAdded(entity);
            EntitySpawnQueue.Remove(entity);
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        protected override void OnEntityRemoved(EntityBase entity)
        {
            base.OnEntityRemoved(entity);
            if (EntityList.ContainsValue(entity))
            {
                if (!entity.IsDead)
                {
                    EntitySpawnQueue.Add(entity);
                    LoadedEntityList.Remove(entity);
                }
                else
                {
                    EntityList.Remove(entity);
                }
            }
        }

        /// <summary>
        /// Запланировать удаление сущности в следующем тике
        /// </summary>
        public override void RemoveEntity(EntityBase entity)
        {
            base.RemoveEntity(entity);
            EntityList.Remove(entity);
        }

        /// <summary>
        /// Удалить всех сущностей
        /// </summary>
        private void RemoveAllEntities()
        {
            LoadedEntityList.RemoveRange(UnloadedEntityList);
            while (UnloadedEntityList.Count > 0)
            {
                EntityBase entity = UnloadedEntityList.FirstRemove();
                if (entity.AddedToChunk)
                {
                    ChunkBase chunk = ChunkPrClient.GetChunk(entity.PositionChunk);
                    if (chunk != null) chunk.RemoveEntity(entity);
                }
                OnEntityRemoved(entity);
            }
            while(LoadedEntityList.Count > 0)
            {
                EntityBase entity = LoadedEntityList.FirstRemove();
                if (entity.IsDead && entity.AddedToChunk)
                {
                    ChunkBase chunk = ChunkPrClient.GetChunk(entity.PositionChunk);
                    if (chunk != null) chunk.RemoveEntity(entity);
                }
                OnEntityRemoved(entity);
            }
        }

        /// <summary>
        /// Респавн игрока
        /// </summary>
        public void Respawn()
        {
            RemoveAllEntities();
            ClientMain.TrancivePacket(new PacketC16ClientStatus(PacketC16ClientStatus.EnumState.Respawn));
        }

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        public override void SpawnEntityInWorld(EntityBase entity)
        {
            base.SpawnEntityInWorld(entity);
            EntityList.Add(entity);
            //EntitySpawnQueue.Add(entity);
        }

        /// <summary>
        /// Заспавнить частицу
        /// </summary>
        public override void SpawnParticle(EnumParticle particle, int count, vec3 pos, vec3 offset, float motion, params int[] items)
        {
            if (count == 1)
            {
                ClientMain.EffectRender.SpawnParticle(particle, pos, offset * motion, items);
            }
            else
            {
                vec3 of;
                vec3 m = new vec3(0);
                for (int i = 0; i < count; i++)
                {
                    of = new vec3(
                        (Rnd.NextFloat() - .5f) * offset.x,
                        (Rnd.NextFloat() - .5f) * offset.y,
                        (Rnd.NextFloat() - .5f) * offset.z);
                    if (motion > 0)
                    {
                        m = new vec3(
                            (Rnd.NextFloat() - .5f) * motion,
                            (Rnd.NextFloat() - .5f) * motion,
                            (Rnd.NextFloat() - .5f) * motion);
                    }
                    ClientMain.EffectRender.SpawnParticle(particle, pos + of, m, items);
                }
            }
        }

        protected override void UpdateEntity(EntityBase entity)
        {
            entity.UpdateClient();
            base.UpdateEntity(entity);
        }

        #endregion

        #region Debug

        public void CountEntitiesShowBegin() => entitiesCountShow = 0;
        public void CountEntitiesShowAdd() => entitiesCountShow++;

        #endregion

        /// <summary>
        /// Проиграть звуковой эффект, глобальная координата
        /// </summary>
        public override void PlaySound(EntityLiving entity, AssetsSample key, vec3 pos, float volume, float pitch) 
            => PlaySound(key, pos, volume, pitch);

        /// <summary>
        /// Проиграть звуковой эффект только для клиента
        /// </summary>
        public override void PlaySound(AssetsSample key, vec3 pos, float volume, float pitch)
        {
            if (key != AssetsSample.None)
            {
                pos -= ClientMain.Player.Position;
                pos = pos.rotateYaw(ClientMain.Player.RotationYawHead);
                // Слышимость в 4 раза лучше
                pos *= .25f;
                ClientMain.Sample.PlaySound(key, pos, volume, pitch);
            }
        }

        /// <summary>
        /// Обработка ближайших блоков для эффектов звуков и анимации, только у клиента
        /// </summary>
        /// <param name="playerPos">Позиция игрока</param>
        public void DoVoidFogParticles(vec3i playerPos)
        {
            int distance = 24;
            Rand random = new Rand();
            BlockPos blockPos = new BlockPos();
            BlockState blockState;
            for (int i = 0; i < 1000; i++)
            {
                blockPos.X = playerPos.x + random.Next(distance) - random.Next(distance);
                blockPos.Y = playerPos.y + random.Next(distance) - random.Next(distance);
                blockPos.Z = playerPos.z + random.Next(distance) - random.Next(distance);
                blockState = GetBlockState(blockPos);
                blockState.GetBlock().RandomDisplayTick(this, blockPos, blockState, random);
            }
        }

        public override void DebugString(string logMessage, params object[] args) => Debug.DebugString = string.Format(logMessage, args);

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            return string.Format("t {2} {0} E:{4}/{5} P:{8}\r\n{1}",
                ChunkPrClient.ToString(), // 0
                ClientMain.Player,  // 1
                ClientMain.TickCounter / 20,  // 2
                "",//ClientMain.Player.Inventory, // 3
                PlayerEntities.Count + 1, // 4
                entitiesCountShow, // 5
                "",//EntityList.Count, // 6
                "",//base.ToStringDebug(), // 7
                ClientMain.EffectRender.CountParticles() // 8
            );
        }
    }
}
