using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using MvkServer.World.Chunk.Light;
using System;
using System.Collections.Generic;

namespace MvkServer.World
{
    /// <summary>
    /// Базовый объект мира
    /// </summary>
    public abstract partial class WorldBase
    {
        /// <summary>
        /// Радиус активных чанков во круг игрока
        /// </summary>
        private const int RADIUS_ACTION_CHUNK = 9;
        

        /// <summary>
        /// Объект лога
        /// </summary>
        public Logger Log { get; protected set; }
        /// <summary>
        /// Объект работы со светом
        /// </summary>
        public WorldLight Light { get; private set; }

        /// <summary>
        /// Объект отладки по задержке в лог
        /// </summary>
        public Profiler profiler;

        /// <summary>
        /// Посредник чанков
        /// </summary>
        public ChunkProvider ChunkPr { get; protected set; }
        /// <summary>
        /// Объект проверки коллизии
        /// </summary>
        public CollisionBase Collision { get; protected set; }

        /// <summary>
        /// Список всех сущностей во всех загруженных в данный момент чанках 
        /// </summary>
        public MapListEntity LoadedEntityList { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Список сущностей которые надо выгрузить
        /// </summary>
        public MapListEntity UnloadedEntityList { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Список игроков в мире
        /// </summary>
        public MapListEntity PlayerEntities { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Объект генератора случайных чисел
        /// </summary>
        public Rand Rnd { get; protected set; }

        /// <summary>
        /// Это значение true для клиентских миров и false для серверных миров.
        /// </summary>
        public bool IsRemote { get; protected set; } = true;
        /// <summary>
        /// Не имеет неба, true. (Может и не пригодится, но для освещения флаг сделал)
        /// </summary>
        public bool HasNoSky { get; protected set; } = false;

        /// <summary>
        /// заполняется чанками, которые находятся в пределах 9 чанков от любого игрока
        /// </summary>
        protected ListMvk<vec2i> activeChunkSet = new ListMvk<vec2i>(1000);
        /// <summary>
        /// Содержит текущее начальное число линейного конгруэнтного генератора для обновлений блоков. 
        /// Используется со значением A, равным 3, и значением C, равным 0x3c6ef35f, 
        /// создавая очень плоский ряд значений, плохо подходящих для выбора случайных блоков в поле 16x128x16.
        /// </summary>
        protected int updateLCG;
        /// <summary>
        /// Объект взрыва
        /// </summary>
        protected Explosion explosion;

        protected WorldBase()
        {
            Collision = new CollisionBase(this);
            profiler = new Profiler(Log);
            Rnd = new Rand();
            Light = new WorldLight(this);
            updateLCG = new Rand().Next();
            explosion = new Explosion(this);
        }

        public Profiler TheProfiler() => profiler;

        /// <summary>
        /// Игровое время
        /// </summary>
        public virtual uint GetTotalWorldTime() => 0;

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public virtual void Tick()
        {

        }

        #region Entity

        /// <summary>
        /// Загрузить коллекцию сущностей
        /// </summary>
        public void LoadEntities(MapListEntity entityCollection)
        {
            LoadedEntityList.AddRange(entityCollection);
            for (int i = 0; i < entityCollection.Count; i++)
            {
                OnEntityAdded((EntityLiving)entityCollection.GetAt(i));
            }
        }

        /// <summary>
        /// Выгрузить коллекцию сущностей
        /// </summary>
        public void UnloadEntities(MapListEntity entities) => UnloadedEntityList.AddRange(entities);

        /// <summary>
        /// Обновляет (и очищает) объекты и объекты чанка 
        /// </summary>
        public virtual void UpdateEntities()
        {
            // колекция для удаления
            MapListEntity entityRemove = new MapListEntity();

            profiler.StartSection("EntitiesUnloadedList");

            // Выгружаем сущности которые имеются в списке выгрузки
            LoadedEntityList.RemoveRange(UnloadedEntityList);
            entityRemove.AddRange(UnloadedEntityList);
            UnloadedEntityList.Clear();

            profiler.EndStartSection("EntityTick");
            
            
            // Пробегаем по всем сущностям и обрабатываеи их такт
            for (int i = 0; i < LoadedEntityList.Count; i++)
            {
                EntityBase entity = LoadedEntityList.GetAt(i);

                if (entity != null)
                {
                    if (!entity.IsDead)
                    {
                        try
                        {
                            UpdateEntity(entity);
                        }
                        catch (Exception ex)
                        {
                            Logger.Crach(ex);
                            throw;
                        }
                    }
                    else
                    {
                        entityRemove.Add(entity);
                    }
                }
            }

            profiler.EndStartSection("EntityRemove");

            // Удаляем 
            while (entityRemove.Count > 0)
            {

                if (IsRemote)
                {
                    bool b = true;
                }
                EntityBase entity = entityRemove.FirstRemove();
                if (entity.AddedToChunk && ChunkPr.IsChunkLoaded(entity.PositionChunk))
                {
                    GetChunk(entity.PositionChunk).RemoveEntity(entity);
                }
                LoadedEntityList.Remove(entity);
                OnEntityRemoved(entity);
            }

            profiler.EndSection();
        }

        protected virtual void UpdateEntity(EntityBase entity)
        {
            
            //byte var5 = 32;

            // Проверка о наличии соседних чанков
            //if (IsAreaLoaded(x - var5, 0, z - var5, x + var5, 0, z + var5, true))
            {
                if (entity.AddedToChunk)
                {
                    entity.Update();
                }

                vec2i posCh = entity.GetChunkPos();

                //posCh = entity.GetChunkPos();
                float y = entity.GetChunkY(); 

                if (!entity.AddedToChunk || !posCh.Equals(entity.PositionChunk) || y != entity.PositionChunkY)
                {
                    if (entity.AddedToChunk && ChunkPr.IsChunkLoaded(entity.PositionChunk))
                    {
                        // Удаляем из старого псевдо чанка
                        GetChunk(entity.PositionChunk).RemoveEntityAtIndex(entity, entity.PositionChunkY);
                    }
                    if (ChunkPr.IsChunkLoaded(posCh))
                    {
                        // Добавляем в новый псевдочанк
                        entity.AddedToChunk = true;
                        GetChunk(posCh).AddEntity(entity);
                    }
                    else
                    {
                        // Если нет чанка помечаем что сущность без чанка
                        entity.AddedToChunk = false;
                    }
                }
            }
        }

        protected virtual void OnEntityAdded(EntityBase entity)
        {
            //for (int i = 0; i < this.worldAccesses.size(); ++i)
            //{
            //    ((IWorldAccess)this.worldAccesses.get(var2)).OnEntityAdded(entity);
            //}
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        protected virtual void OnEntityRemoved(EntityBase entity)
        {
            //for (int i = 0; i < this.worldAccesses.size(); ++i)
            //{
            //    ((IWorldAccess)this.worldAccesses.get(i)).OnEntityRemoved(entity);
            //}
        }

        /// <summary>
        /// Запланировать удаление сущности в следующем тике
        /// </summary>
        public virtual void RemoveEntity(EntityBase entity)
        {
            entity.SetDead();
            if (entity is EntityPlayer)
            {
                PlayerEntities.Remove(entity);
                // Флаг сна всех игроков
                //UpdateAllPlayersSleepingFlag();
                OnEntityRemoved(entity);
            }
        }

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        public virtual void SpawnEntityInWorld(EntityBase entity)
        {
            vec2i posCh = entity.GetChunkPos();
           // bool flagSpawn = entity.FlagSpawn;

           // if (entity is EntityPlayer) flagSpawn = true;

            //ChunkBase chunk = null;
           // bool isChunk = false;
            //if (this is WorldServer)
            //{
            //    chunk = ((WorldServer)this).ChunkPrServ.LoadChunk(posCh);
            //    isChunk = chunk != null;
            //}
            //else
            //{
               // isChunk = ChunkPr.IsChunk(posCh);
           // }
            
            //if (!flagSpawn && !isChunk)
            //{
            //    return false;
            //}
            //else
            {
                if (entity is EntityPlayer entityPlayer)
                {
                    PlayerEntities.Add(entityPlayer);
                    // Флаг сна всех игроков
                    //UpdateAllPlayersSleepingFlag();
                }

                ChunkBase chunk;
                //if (!isChunk)\
                //if (this is WorldServer worldServer)
                //{
                //    chunk = worldServer.ChunkPrServ.LoadChunk(posCh);
                //} else {
                    chunk = GetChunk(posCh);
                //}
                if (chunk != null) chunk.AddEntity(entity);
                else
                {
                    
                   // return false;
                }
                LoadedEntityList.Add(entity);
                OnEntityAdded(entity);
            }
        }

        /// <summary>
        /// Заспавнить частицы
        /// </summary>
        public virtual void SpawnParticle(EnumParticle particle, int count, vec3 pos, vec3 offset, float motion, params int[] items) { }

        /// <summary>
        /// Частички блока
        /// </summary>
        /// <param name="blockPos">позиция где рассыпаются частички</param>
        /// <param name="count">количество частичек</param>
        public void ParticleDiggingBlock(BlockBase block, BlockPos blockPos, int count)
        {
            if (block != null && block.IsParticle)
            {
                SpawnParticle(EnumParticle.Digging, count,
                    new vec3(blockPos.X + .5f, blockPos.Y + .625f, blockPos.Z + .5f), 
                    new vec3(1f, .75f, 1f), 0, (int)block.EBlock);
            }
        }

        #endregion

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public virtual string ToStringDebug()
        {
            return string.Format("{0}/{1}", LoadedEntityList.Count, UnloadedEntityList.Count);
        }

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(vec2i pos) => ChunkPr.GetChunk(pos);
        /// <summary>
        /// Получить чанк по координатам блока
        /// </summary>
        public ChunkBase GetChunk(BlockPos pos) => ChunkPr.GetChunk(new vec2i(pos.X >> 4, pos.Z >> 4));

        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        //public bool IsChunk(vec2i pos) => ChunkPr.IsChunk(pos);

        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="bpos">глобальная позиция блока</param>
       // public BlockBase GetBlock(BlockPos bpos) => GetBlock(bpos.X, bpos.Y, bpos.Z);
        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="pos">глобальная позиция блока</param>
        //public BlockBase GetBlock(int x, int y, int z)
        //{
        //    if (y >= 0 && y <= 255)
        //    {
        //        ChunkBase chunk = GetChunk(new vec2i(x >> 4, z >> 4));
        //        if (chunk != null)
        //        {
        //            return chunk.GetBlock0(new vec3i(x & 15, y, z & 15));
        //        }
        //    }
        //    return Blocks.CreateAir(new vec3i(x, y, z));
        //}

        /// <summary>
        /// Получить блок данных
        /// </summary>
        public BlockState GetBlockState(BlockPos blockPos)
        {
            if (blockPos.IsValid())
            {
                ChunkBase chunk = GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    return chunk.GetBlockState(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
                }
            }
            return new BlockState().Empty();
        }

        public bool IsAirBlock(BlockPos blockPos)
        {
            if (blockPos.IsValid())
            {
                ChunkBase chunk = GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    return chunk.GetBlockState(blockPos.X & 15, blockPos.Y, blockPos.Z & 15).id == 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Получить кэшовый блок
        /// </summary>
        //public BlockBase GetBlockCache(BlockPos bpos)
        //{
        //    EnumBlock enumBlock = GetEBlock(bpos);
        //    return Blocks.GetBlockCache(enumBlock);
        //}

        /// <summary>
        /// Получить тип блока
        /// </summary>
        /// <param name="bpos">глобальная позиция блока</param>
        //public EnumBlock GetEBlock(BlockPos bpos) => GetEBlock(bpos.X, bpos.Y, bpos.Z);
        ///// <summary>
        ///// Получить тип блока
        ///// </summary>
        ///// <param name="pos">глобальная позиция блока</param>
        //public EnumBlock GetEBlock(int x, int y, int z)
        //{
        //    if (y >= 0 && y <= 255)
        //    {
        //        ChunkBase chunk = GetChunk(new vec2i(x >> 4, z >> 4));
        //        if (chunk != null)
        //        {
        //            return chunk.GetEBlock(new vec3i(x & 15, y, z & 15));
        //        }
        //    }
        //    return EnumBlock.Air;
        //}

        /// <summary>
        /// Пересечения лучей с визуализируемой поверхностью для блока
        /// </summary>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public MovingObjectPosition RayCastBlock(vec3 a, vec3 dir, float maxDist, bool collidable)
        {
            float px = a.x;
            float py = a.y;
            float pz = a.z;

            float dx = dir.x;
            float dy = dir.y;
            float dz = dir.z;

            float t = 0.0f;
            int ix = Mth.Floor(px);
            int iy = Mth.Floor(py);
            int iz = Mth.Floor(pz);
            int stepx = (dx > 0.0f) ? 1 : -1;
            int stepy = (dy > 0.0f) ? 1 : -1;
            int stepz = (dz > 0.0f) ? 1 : -1;
            Pole sidex = (dx > 0.0f) ? Pole.West : Pole.East;
            Pole sidey = (dy > 0.0f) ? Pole.Down : Pole.Up;
            Pole sidez = (dz > 0.0f) ? Pole.North : Pole.South;

            float infinity = float.MaxValue;

            float txDelta = (dx == 0.0f) ? infinity : Mth.Abs(1.0f / dx);
            float tyDelta = (dy == 0.0f) ? infinity : Mth.Abs(1.0f / dy);
            float tzDelta = (dz == 0.0f) ? infinity : Mth.Abs(1.0f / dz);

            float xdist = (stepx > 0) ? (ix + 1 - px) : (px - ix);
            float ydist = (stepy > 0) ? (iy + 1 - py) : (py - iy);
            float zdist = (stepz > 0) ? (iz + 1 - pz) : (pz - iz);

            float txMax = (txDelta < infinity) ? txDelta * xdist : infinity;
            float tyMax = (tyDelta < infinity) ? tyDelta * ydist : infinity;
            float tzMax = (tzDelta < infinity) ? tzDelta * zdist : infinity;

            int steppedIndex = -1;

            BlockPos blockPos = new BlockPos();
            BlockState blockState;
            BlockBase block;
            Pole side = Pole.All;
            vec3i norm;
            vec3 end;

            while (t <= maxDist)
            {
                blockPos.X = ix;
                blockPos.Y = iy;
                blockPos.Z = iz;
                blockState = GetBlockState(blockPos);
                block = blockState.GetBlock();

                if ((!collidable || (collidable && block.IsCollidable)) && block.CollisionRayTrace(blockPos, blockState.met, a, dir, maxDist))
                {
                    end.x = px + t * dx;
                    end.y = py + t * dy;
                    end.z = pz + t * dz;
                    
                    norm.x = norm.y = norm.z = 0;
                    if (steppedIndex == 0)
                    {
                        side = sidex;
                        norm.x = -stepx;
                    }
                    else if (steppedIndex == 1)
                    {
                        side = sidey;
                        norm.y = -stepy;
                    }
                    else if (steppedIndex == 2)
                    {
                        side = sidez;
                        norm.z = -stepz;
                    }

                    return new MovingObjectPosition(blockState, blockPos, side, blockPos.ToVec3() - end, norm, end);
                }
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ix += stepx;
                        t = txMax;
                        txMax += txDelta;
                        steppedIndex = 0;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        iy += stepy;
                        t = tyMax;
                        tyMax += tyDelta;
                        steppedIndex = 1;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
            }
            return new MovingObjectPosition();
        }

        /// <summary>
        /// Изменить метданные блока
        /// </summary>
        public void SetBlockStateMet(BlockPos blockPos, ushort met, bool isRender = true)
        {
            if (!blockPos.IsValid()) return;
            ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            if (chunk == null) return;
            int yc = blockPos.Y >> 4;
            int index = (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15);
            chunk.StorageArrays[yc].NewMetBlock(index, met);
            if (isRender) MarkBlockForRenderUpdate(blockPos.X, blockPos.Y, blockPos.Z);
        }

        /// <summary>
        /// Задать блок неба, с флагом по умолчанию 14 (уведомление соседей, modifyRender, modifySave)
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 8 modifySave, 16 sound break</param>
        public bool SetBlockToAir(BlockPos blockPos, int flag = 14) => SetBlockState(blockPos, new BlockState(EnumBlock.Air), flag);

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="blockState">данные блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 8 modifySave, 16 sound break</param>
        /// <returns>true смена была</returns>
        public virtual bool SetBlockState(BlockPos blockPos, BlockState blockState, int flag)
        {
            if (!blockPos.IsValid()) return false;

            ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            if (chunk == null) return false;

            BlockState blockStateTrue = chunk.SetBlockState(blockPos, blockState, (flag & 8) != 0, (flag & 4) != 0, (flag & 16) != 0);
            if (blockStateTrue.IsEmpty()) return false;

            if (!IsRemote)
            {
                // Частички блока, только на сервере чтоб всем разослать
                if ((flag & 1) != 0) ParticleDiggingBlock(blockStateTrue.GetBlock(), blockPos, 50);
            }
            // Уведомление соседей и на сервере и на клиенте
            if ((flag & 2) != 0) NotifyNeighborsOfStateChange(blockPos, blockState.GetBlock());
            
            //BlockBase block = blockState.GetBlock();
            //BlockBase blockNew = blockStateTrue.GetBlock();

            //MarkBlockForUpdate(blockPos);

            //if (block.LightOpacity != blockNew.LightOpacity || block.LightValue != blockNew.LightValue)
            //{
            //    TheProfiler().StartSection("checkLight");
            //    //Light.CheckLight(blockPos);
            //    if (block.LightValue != blockNew.LightValue)
            //    {
            //        Light.CheclLightBlock(blockPos);
            //    }
            //    TheProfiler().EndSection();
            //    //chunk.Light.ResetRelightChecks();
            //}

            return true;


            //chunk.StorageArrays[chy].SetData(pos.x, by, pos.z, blockState.GetData());
            //BlockBase blockNew = Blocks.CreateBlock(blockState.GetEBlock(),
            //                new BlockPos(Position.x << 4 | pos.x, pos.y, Position.y << 4 | pos.z));
            //BlockBase blockOld = World.GetBlock(blockNew.Position);


            //if (blockPos.Y >= 0 && blockPos.Y < 256)
            //{
            //    ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            //    if (chunk != null)
            //    {
            //        //if (!IsRemote)
            //        //{
            //        //    // сервер
            //        //    BlockState blockStateOld = chunk.GetBlockState(blockPos.GetPosition0());
            //        //    //breakBlock()
            //        //}
            //        if (chunk.SetBlockState(blockPos.GetPosition0(), blockState))
            //        {
            //            MarkBlockForUpdate(blockPos);
            //            return true;
            //        }
            //    }
            //}
            //return false;
        }

        /// <summary>
        /// Задать тик блока
        /// </summary>
        public virtual void SetBlockTick(BlockPos blockPos, uint timeTackt, bool priority = false) { }

        /// <summary>
        /// Уведомить соседей об изменении состояния
        /// </summary>
        private void NotifyNeighborsOfStateChange(BlockPos pos, BlockBase block)
        {
            NotifyBlockOfStateChange(pos.OffsetWest(), block);
            NotifyBlockOfStateChange(pos.OffsetEast(), block);
            NotifyBlockOfStateChange(pos.OffsetDown(), block);
            NotifyBlockOfStateChange(pos.OffsetUp(), block);
            NotifyBlockOfStateChange(pos.OffsetNorth(), block);
            NotifyBlockOfStateChange(pos.OffsetSouth(), block);
        }

        /// <summary>
        /// Уведомить о блок об изменения состоянии соседнего блока
        /// </summary>
        private void NotifyBlockOfStateChange(BlockPos pos, BlockBase blockIn)
        {
            try
            {
                BlockState blockState = GetBlockState(pos);
                blockState.GetBlock().NeighborBlockChange(this, pos, blockState, blockIn);
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
        }

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="eBlock">тип блока</param>
        /// <returns>true смена была</returns>
        public void SetBlockDebug(BlockPos blockPos, EnumBlock enumBlock)
        {
            if (/*IsRemote && */blockPos.Y >= 0 && blockPos.Y < 256)
            {
                ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    chunk.SetEBlock(blockPos.GetPosition0(), enumBlock);
                    MarkBlockForRenderUpdate(blockPos.X, blockPos.Y, blockPos.Z);
                }
            }
        }

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        public virtual void MarkBlockForRenderUpdate(int x, int y, int z) { }
        /// <summary>
        /// Отметить блоки для обновления
        /// </summary>
        public virtual void MarkBlockRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1) { }
        /// <summary>
        /// Отметить блоки для изминения
        /// </summary>
        public virtual void MarkBlockRangeForModified(int x0, int z0, int x1, int z1) { }

        /// <summary>
        /// Возвращает все объекты указанного типа класса, которые пересекаются с AABB кроме переданного в него
        /// </summary>
        public MapListEntity GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB type, AxisAlignedBB aabb, int entityId)
        {
            MapListEntity list = new MapListEntity();
            int minX = Mth.Floor((aabb.Min.x - 2f) / 16f);
            int maxX = Mth.Floor((aabb.Max.x + 2f) / 16f);
            int minZ = Mth.Floor((aabb.Min.z - 2f) / 16f);
            int maxZ = Mth.Floor((aabb.Max.z + 2f) / 16f);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    ChunkBase chunk = GetChunk(new vec2i(x, z));
                    if (chunk != null)
                    {
                        chunk.GetEntitiesAABB(entityId, aabb, list, type);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Получить список блоков которые входят в рамку и пересекаются по коллизии
        /// </summary>
        public BlockBase[] GetBlocksAABB(AxisAlignedBB aabb)
        {
            List<BlockBase> blocks = new List<BlockBase>();

            int minX = Mth.Floor(aabb.Min.x);
            int maxX = Mth.Floor(aabb.Max.x + 1f);
            int minY = Mth.Floor(aabb.Min.y);
            int maxY = Mth.Floor(aabb.Max.y + 1f);
            int minZ = Mth.Floor(aabb.Min.z);
            int maxZ = Mth.Floor(aabb.Max.z + 1f);

            for (int x = minX; x < maxX; x++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    if (ChunkPr.IsChunkLoaded(new vec2i(x >> 4, z >> 4)))
                    {
                        for (int y = minY - 1; y < maxY; y++)
                        {
                            BlockPos blockPos = new BlockPos(x, y, z);
                            BlockState blockState = GetBlockState(blockPos);
                            BlockBase block = blockState.GetBlock();
                            AxisAlignedBB mask = block.GetCollision(blockPos, blockState.met);
                            if (mask != null && mask.IntersectsWith(aabb))
                            {
                                blocks.Add(block);
                            }
                        }
                    }
                }
            }

            return blocks.ToArray();
        }

        /// <summary>
        /// Получить список блоков которые входят в рамку и пересекаются по коллизии
        /// </summary>
        //public Dictionary<vec3i, BlockState> GetBlocks(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        //{
        //    Dictionary<vec3i, BlockState> blocks = new Dictionary<vec3i, BlockState>();

        //    for (int x = minX; x <= maxX; x++)
        //    {
        //        for (int z = minZ; z <= maxZ; z++)
        //        {
        //            if (ChunkPr.IsChunk(new vec2i(x >> 4, z >> 4)))
        //            {
        //                for (int y = minY; y <= maxY; y++)
        //                {
        //                    blocks.Add(new vec3i(x, y, z), GetBlockState(new BlockPos(x, y, z)));
        //                }
        //            }
        //        }
        //    }

        //    return blocks;
        //}

        /// <summary>
        /// Возвращает среднюю длину краев ограничивающей рамки блока больше или равна 1
        /// </summary>
        public bool GetAverageEdgeLengthBlock(BlockPos blockPos)
        {
            BlockState blockState = GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            AxisAlignedBB axis = block.GetCollision(blockPos, blockState.met);
            return axis != null && axis.GetAverageEdgeLength() >= 1f;
        }

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public virtual void SendBlockBreakProgress(int breakerId, BlockPos pos, int progress) { }

        /// <summary>
        /// Проверить облость загруженных чанков по XZ, координат в блока и радиус в блоках
        /// </summary>
        public bool IsAreaLoaded(BlockPos blockPos, int radius) => IsAreaLoaded(blockPos.X - radius, blockPos.Y, blockPos.Z - radius,
                blockPos.X + radius, blockPos.Y, blockPos.Z + radius);

        /// <summary>
        /// Проверить облость загруженных чанков, координаты в блоках
        /// </summary>
        public bool IsAreaLoaded(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            if (maxY >= 0 && minY < ChunkBase.COUNT_HEIGHT * 16)
            {
                minX >>= 4;
                minZ >>= 4;
                maxX >>= 4;
                maxZ >>= 4;

                for (int x = minX; x <= maxX; ++x)
                {
                    for (int z = minZ; z <= maxZ; ++z)
                    {
                        if (!ChunkPr.IsChunkLoaded(new vec2i(x, z))) return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Возвращает, если какой-либо из блоков внутри aabb является жидкостью
        /// </summary>
        /// <returns>true - есть вода</returns>
        public bool IsAnyLiquid(AxisAlignedBB aabb)
        {
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        BlockBase block = GetBlockState(new BlockPos(x, y, z)).GetBlock();
                        if (block.Material == EnumMaterial.Water) return true;
                    }
                }
            }
            return false;
        }


        //public BlockPos GetHorizon(BlockPos pos)
        //{
        //    int hy;

        //    if (pos.X >= -30000000 && pos.Z >= -30000000 && pos.X < 30000000 && pos.Z < 30000000)
        //    {
        //        vec2i chPos = new vec2i(pos.X >> 4, pos.Z >> 4);
        //        if (ChunkPr.IsChunk(chPos))
        //        {
        //            ChunkBase chunk = GetChunk(chPos);
        //            hy = chunk.Light.GetHeight(pos.X & 15, pos.Z & 15);
        //        }
        //        else hy = 0;
        //    }
        //    else hy = 64;

        //    return new BlockPos(pos.X, hy, pos.Z);
        //}

        /// <summary>
        /// Получает наименьшую высоту фрагмента, куда непосредственно попадает солнечный свет.
        /// </summary>
        /// <param name="x">глобальная координата блока X</param>
        /// <param name="z">глобальная координата блока Z</param>
        //public int GetChunksLowestHorizon(int x, int z)
        //{
        //    if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000)
        //    {
        //        vec2i chPos = new vec2i(x >> 4, z >> 4);
        //        if (!ChunkPr.IsChunk(chPos)) return 0;
        //        else
        //        {
        //            ChunkBase chunk = GetChunk(chPos);
        //            return chunk.Light.GetLowestHeight();
        //        }
        //    }
        //    return 64;
        //}

        /// <summary>
        /// Проверка нахождения в жидкости в рамке AABB
        /// </summary>
        public Liquid BeingInLiquid(AxisAlignedBB aabb)
        {
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();
            int minX = min.x;
            int maxX = max.x + 1;
            int minY = min.y;
            int maxY = max.y + 1;
            int minZ = min.z;
            int maxZ = max.z + 1;

            Liquid liquid = new Liquid();
            if (!IsAreaLoaded(minX, minY, minZ, maxX, maxY, maxZ)) return liquid;

            BlockPos blockPos = new BlockPos();
            BlockBase block;
            EnumMaterial material;
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        blockPos.X = x;
                        blockPos.Y = y;
                        blockPos.Z = z;
                        block = GetBlockState(blockPos).GetBlock();
                        material = block.Material;
                        if (material == EnumMaterial.Water)
                        {
                            liquid.Water(block.ModifyAcceleration(this, blockPos, liquid.GetVecWater()));
                        }
                        else if (material == EnumMaterial.Lava) liquid.Lava();
                        else if (material == EnumMaterial.Oil)
                        {
                            liquid.Oil(block.ModifyAcceleration(this, blockPos, liquid.GetVecOil()));
                        }
                    }
                }
            }
            liquid.VecSpeed();
            return liquid;
        }

        /// <summary>
        /// Собрать активные чанки игроков
        /// </summary>
        protected void SetActivePlayerChunksAndCheckLight()
        {
            activeChunkSet.Clear();
            TheProfiler().StartSection("buildList");
            int x, z, dist, overviewChunk;
            EntityPlayer entityPlayer;
            vec2i chPos;
            for (int i = 0; i < PlayerEntities.Count; i++)
            {
                entityPlayer = PlayerEntities.GetAt(i) as EntityPlayer;
                overviewChunk = entityPlayer.OverviewChunk;
                chPos = entityPlayer.PositionChunk;
                dist = Mth.Min(overviewChunk, RADIUS_ACTION_CHUNK); // радиус активных чанков

                for (x = -dist; x <= dist; x++)
                {
                    for (z = -dist; z <= dist; z++)
                    {
                        activeChunkSet.Add(new vec2i(x + chPos.x, z + chPos.y));
                    }
                }
            }
            TheProfiler().EndSection();

            //TheProfiler().StartSection("playerCheckLight");

            //if (PlayerEntities.Count > 0)
            //{
            //    int indexRand = Rand.Next(PlayerEntities.Count);
            //    EntityBase entity = PlayerEntities.GetAt(indexRand);
            //   // Light.CheckLight(new BlockPos(entity.GetBlockPos()));
            //}

            //TheProfiler().EndSection();
        }

        /// <summary>
        /// Проиграть звуковой эффект
        /// </summary>
        public virtual void PlaySound(EntityLiving entity, AssetsSample key, vec3 pos, float volume, float pitch) { }

        /// <summary>
        /// Проиграть звуковой эффект только для клиента
        /// </summary>
        public virtual void PlaySound(AssetsSample key, vec3 pos, float volume, float pitch) { }

        /// <summary>
        /// Блок имеет твердую непрозрачную поверхность
        /// </summary>
        public bool DoesBlockHaveSolidTopSurface(BlockPos blockPos)
        {
            BlockState blockState = GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return block.IsNotTransparent() && block.FullBlock;
        }

        /// <summary>
        /// Получить биом по позиции облка
        /// </summary>
        public EnumBiome GetBiome(BlockPos blockPos)
        {
            ChunkBase chunk = GetChunk(blockPos);
            if (chunk == null) return EnumBiome.Plain;
            int bx = blockPos.X & 15;
            int bz = blockPos.Z & 15;
            return chunk.biome[bx << 4 | bz];
        }

        /// <summary>
        /// Создать взрыв
        /// </summary>
        /// <param name="pos">позиция эпицентра</param>
        /// <param name="strength">сила</param>
        /// <param name="distance">дистанция</param>
        public virtual void CreateExplosion(vec3 pos, float strength, float distance) { }

        public virtual void DebugString(string logMessage, params object[] args) { }
    }
}
