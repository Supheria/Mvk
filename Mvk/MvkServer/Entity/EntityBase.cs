using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkServer.Entity
{
    /// <summary>
    /// Базовый объект сущности
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Объект мира
        /// </summary>
        public WorldBase World { get; protected set; }
        /// <summary>
        /// Порядковый номер сущности на сервере, с момента запуска сервера
        /// </summary>
        public ushort Id { get; protected set; }

        /// <summary>
        /// Позиция объекта
        /// </summary>
        public vec3 Position { get; private set; }
        /// <summary>
        /// Позиция в чанке
        /// </summary>
        public vec2i PositionChunk { get; private set; }
        /// <summary>
        /// Позиция псевдо чанка
        /// </summary>
        public int PositionChunkY { get; private set; }
        /// <summary>
        /// Позиция на последнем тике для рендера, клиента
        /// </summary>
        public vec3 LastTickPos { get; set; }
        /// <summary>
        /// Координата объекта на предыдущем тике, используемая для расчета позиции во время процедур рендеринга
        /// </summary>
        public vec3 PositionPrev { get; protected set; }
        /// <summary>
        /// Позиция данных с сервера
        /// </summary>
        public vec3 PositionServer { get; protected set; }
        /// <summary>
        /// Перемещение объекта
        /// </summary>
        public vec3 Motion { get; protected set; }
        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround { get; protected set; } = true;
        /// <summary>
        /// Летает ли сущность
        /// </summary>
        public bool IsFlying { get; protected set; } = false;
        /// <summary>
        /// Будет ли эта сущность проходить сквозь блоки
        /// </summary>
        public bool NoClip { get; protected set; } = false;
        /// <summary>
        /// Ограничивающая рамка
        /// </summary>
        public AxisAlignedBB BoundingBox { get; protected set; }
        /// <summary>
        /// Пол ширина сущности
        /// </summary>
        public float Width { get; protected set; }
        /// <summary>
        /// Высота сущности
        /// </summary>
        public float Height { get; protected set; }
        /// <summary>
        /// Как высоко эта сущность может подняться при столкновении с блоком, чтобы попытаться преодолеть его
        /// </summary>
        public float StepHeight { get; protected set; }
        /// <summary>
        /// Сущность мертва, не активна
        /// </summary>
        public bool IsDead { get; protected set; } = false;
        /// <summary>
        /// Истинно, если после перемещения этот объект столкнулся с чем-то по оси X или Z. 
        /// </summary>
        public bool IsCollidedHorizontally { get; protected set; } = false;
        /// <summary>
        /// Истинно, если после перемещения этот объект столкнулся с чем-то по оси Y 
        /// </summary>
        public bool IsCollidedVertically { get; protected set; } = false;
        /// <summary>
        /// Истинно, если после перемещения эта сущность столкнулась с чем-то либо вертикально, либо горизонтально 
        /// </summary>
        public bool IsCollided { get; protected set; } = false;
        /// <summary>
        /// Был ли эта сущность добавлена в чанк, в котором он находится? 
        /// </summary>
        public bool AddedToChunk { get; set; } = false;
        /// <summary>
        /// Объект дополнительных данных
        /// </summary>
        public DataWatcher MetaData { get; protected set; }
        
        /// <summary>
        /// Генератор случайных чисел данной сущности
        /// </summary>
        protected Rand rand;
        /// <summary>
        /// Для отладки движения
        /// </summary>
        protected vec3 motionDebug = new vec3(0);
        /// <summary>
        /// Параметр долголетия сущности в огне в тактах
        /// </summary>
        protected int fire;
        /// <summary>
        /// Неуязвимость при уроне в тиках огня, лавы, кактуса и подобного
        /// </summary>
        protected byte invulnerable = 0;
        /// <summary>
        /// Стартавая неуязвимость при уроне в тиках огня, лавы, кактуса и подобного
        /// </summary>
        protected byte isInvulnerableBegin = 0;
        /// <summary>
        /// Должна ли эта сущность НЕ исчезать,
        /// true - не исчезнет
        /// </summary>
        protected bool persistenceRequired;
        /// <summary>
        /// Тип сущности
        /// </summary>
        protected EnumEntities type = EnumEntities.None;
        /// <summary>
        /// Находится ли этот объект в настоящее время в воде
        /// </summary>
        private bool inWater;
        /// <summary>
        /// Находится ли этот объект в настоящее время в лаве
        /// </summary>
        private bool inLava;
        /// <summary>
        /// Находится ли этот объект в настоящее время в нефте
        /// </summary>
        private bool inOil;
        /// <summary>
        /// Находится ли этот объект в настоящее время в блоке с замедлением
        /// </summary>
        private bool inSlow;

        public EntityBase(WorldBase world)
        {
            World = world;
            rand = World.Rnd;
            SetSize(.5f, 1f);
            if (world is WorldServer worldServer)
            {
                worldServer.Players.NewEntity(this);
            }
            MetaData = new DataWatcher(this);
            AddMetaData();
        }

        /// <summary>
        /// Тип сущности
        /// </summary>
        public virtual EnumEntities GetEntityType() => type;

        protected virtual void AddMetaData() { }

        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public virtual string GetName() => "";

        /// <summary>
        /// Заменить смещение сущьности
        /// </summary>
        public void SetMotion(vec3 motion) => Motion = motion;

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        public virtual bool HasCustomName() => false;

        /// <summary>
        /// В каком блоке находится
        /// </summary>
        public vec3i GetBlockPos() => new vec3i(Position);
        /// <summary>
        /// Получить координаты в каком чанке находится по текущей Position
        /// </summary>
        public vec2i GetChunkPos() => new vec2i(Mth.Floor(Position.x) >> 4, Mth.Floor(Position.z) >> 4);
        /// <summary>
        /// Получить координату псевдо чанка находится по текущей Position
        /// </summary>
        public int GetChunkY() => Mth.Floor(Position.y) >> 4;

        /// <summary>
        /// Получить ограничительную рамку на выбранной позиции
        /// </summary>
        public AxisAlignedBB GetBoundingBox(vec3 pos) => new AxisAlignedBB(pos - new vec3(Width, 0, Width), pos + new vec3(Width, Height, Width));

        /// <summary>
        /// Задать позицию
        /// </summary>
        public bool SetPosition(vec3 pos)
        {
            if (!Position.Equals(pos))
            {
                Position = pos;
                UpBoundingBox();
                ActionAddPosition();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Задать плоожение в чанке
        /// </summary>
        public void SetPositionChunk(int x, int y, int z)
        {
            PositionChunk = new vec2i(x, z);
            PositionChunkY = y;
        }

        /// <summary>
        /// Обновить значения позиции чанка по тикущим значениям
        /// </summary>
        public void UpPositionChunk()
        {
            PositionChunkY = GetChunkY();
            PositionChunk = GetChunkPos();
        }

        /// <summary>
        /// Задать действие для позиции
        /// </summary>
        protected virtual void ActionAddPosition() { }

        /// <summary>
        /// Заменить размер хитбокс сущности
        /// </summary>
        protected void SetSize(float width, float height)
        {
            Height = height;
            Width = width;
            UpBoundingBox();
        }

        /// <summary>
        /// Будет уничтожен следующим тиком
        /// </summary>
        public virtual void SetDead() => IsDead = true;

        /// <summary>
        /// Задать индекс
        /// </summary>
        public void SetEntityId(ushort id) => Id = id;

        /// <summary>
        /// Задать время сколько будет горет сущность в тактах
        /// </summary>
        public virtual void SetFire(int takt)
        {
            if (fire < takt)
            {
                int f = fire - (fire / 20) * 20;
                fire = takt + f;
            }
        }

        /// <summary>
        /// Нанесет указанное количество урона сущности, если сущность не защищена от огненного урона.
        /// </summary>
        protected void DealFireDamage(int amount)
        {
            if (AttackEntityFrom(EnumDamageSource.InFire, amount)) SetFire(160);
        }

        /// <summary>
        /// Поджечь от лавы
        /// </summary>
        protected void SetOnFireFromLava()
        {
            if (AttackEntityFrom(EnumDamageSource.Lava, 4f)) SetFire(300);
        }

        /// <summary>
        /// Сущности наносит урон только на сервере
        /// </summary>
        /// <param name="amount">сила урона</param>
        /// <returns>true - урон был нанесён</returns>
        public bool AttackEntityFrom(EnumDamageSource source, float amount, EntityLiving entityAttacks = null) 
            => AttackEntityFrom(source, amount, new vec3(0), entityAttacks);

        /// <summary>
        /// Сущности наносит урон только на сервере
        /// </summary>
        /// <param name="amount">сила урона</param>
        /// <returns>true - урон был нанесён</returns>
        public virtual bool AttackEntityFrom(EnumDamageSource source, float amount, vec3 motion, EntityLiving entityAttacks = null)
        {
            if (IsImmuneToAll() && source != EnumDamageSource.OutOfWorld) return false;
            if (IsImmuneToFall() && source == EnumDamageSource.Fall) return false;
            if (IsImmuneToFire() && (source == EnumDamageSource.InFire || source == EnumDamageSource.OnFire || source == EnumDamageSource.Lava)) return false;
            if (IsImmuneToLackOfAir() && source == EnumDamageSource.Drown) return false;
            return true;
        }

        /// <summary>
        /// Обновить ограничительную рамку
        /// </summary>
        protected void UpBoundingBox() => BoundingBox = GetBoundingBox(Position);

        public virtual vec3 GetPositionFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || Position.Equals(LastTickPos)) return Position;
            return LastTickPos + (Position - LastTickPos) * timeIndex;
        }

        

        /// <summary>
        /// Проверка перемещения со столкновением
        /// </summary>
        protected void MoveEntity(vec3 motion)
        {
            // Без проверки столкновения
            if (NoClip)
            {
                Motion = motion;
                UpPositionMotion();
                return;
            }

            AxisAlignedBB boundingBox = BoundingBox.Clone();
            AxisAlignedBB aabbEntity = boundingBox.Clone();
            List<AxisAlignedBB> aabbs;

            float x0 = motion.x;
            float y0 = motion.y;
            float z0 = motion.z;

            if (IsInSlow() && IsSpeed​​Limit())
            {
                // медленее чуть блоьше половины 
                x0 *= .67f;
                y0 *= .85f;
                z0 *= .67f;
            }

            float x = x0;
            float y = y0;
            float z = z0;

            bool isSneaking = false;
            if (this is EntityLiving entityLiving)
            {
                isSneaking = entityLiving.IsSneaking();
            }

            // Защита от падения с края блока если сидишь и являешься игроком
            if (OnGround && this is EntityPlayer entityPlayer && entityPlayer.Movement.Sneak)
            {
                // Уменьшаем размер рамки для погрешности флоат, Fix 2022-02-01 замечена бага, иногда падаешь! По Х на 50000
                AxisAlignedBB boundingBoxS = boundingBox.Expand(new vec3(-.01f, 0, -.01f));
                // Шаг проверки смещения
                float step = 0.05f;
                for (; x != 0f && World.Collision.GetCollidingBoundingBoxes(boundingBoxS.Offset(new vec3(x, -1, 0))).Count == 0; x0 = x)
                {
                    if (x < step && x >= -step) x = 0f;
                    else if (x > 0f) x -= step;
                    else x += step;
                }
                for (; z != 0f && World.Collision.GetCollidingBoundingBoxes(boundingBoxS.Offset(new vec3(0, -1, z))).Count == 0; z0 = z)
                {
                    if (z < step && z >= -step) z = 0f;
                    else if (z > 0f) z -= step;
                    else z += step;
                }
                for (; x != 0f && z0 != 0f && World.Collision.GetCollidingBoundingBoxes(boundingBoxS.Offset(new vec3(x0, -1, z0))).Count == 0; z0 = z)
                {
                    if (x < step && x >= -step) x = 0f;
                    else if (x > 0f) x -= step;
                    else x += step;
                    x0 = x;
                    if (z < step && z >= -step) z = 0f;
                    else if (z > 0f) z -= step;
                    else z += step;
                }
            }

            aabbs = World.Collision.GetCollidingBoundingBoxes(boundingBox.AddCoordBias(new vec3(x, y, z)));

            // Находим смещение по Y
            foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);
            aabbEntity = aabbEntity.Offset(new vec3(0, y, 0));

            // Не прыгаем (момент взлёта)
            bool isNotJump = OnGround || motion.y != y && motion.y < 0f;

            // Находим смещение по X
            foreach (AxisAlignedBB axis in aabbs) x = axis.CalculateXOffset(aabbEntity, x);
            aabbEntity = aabbEntity.Offset(new vec3(x, 0, 0));

            // Находим смещение по Z
            foreach (AxisAlignedBB axis in aabbs) z = axis.CalculateZOffset(aabbEntity, z);
            aabbEntity = aabbEntity.Offset(new vec3(0, 0, z));


            // Запуск проверки авто прыжка
            if (StepHeight > 0f && isNotJump && (x0 != x || z0 != z))
            {
                // Кэш для откада, если авто прыжок не допустим
                vec3 monCache = new vec3(x, y, z);

                float stepHeight = StepHeight;
                // Если сидим авто прыжок в двое ниже
                if (isSneaking) stepHeight *= 0.5f;

                y = stepHeight;
                aabbs = World.Collision.GetCollidingBoundingBoxes(boundingBox.AddCoordBias(new vec3(x0, y, z0)));
                AxisAlignedBB aabbEntity2 = boundingBox.Clone();
                AxisAlignedBB aabb = aabbEntity2.AddCoordBias(new vec3(x0, 0, z0));

                // Находим смещение по Y
                float y2 = y;
                foreach (AxisAlignedBB axis in aabbs) y2 = axis.CalculateYOffset(aabb, y2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(0, y2, 0));

                // Находим смещение по X
                float x2 = x0;
                foreach (AxisAlignedBB axis in aabbs) x2 = axis.CalculateXOffset(aabbEntity2, x2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(x2, 0, 0));

                // Находим смещение по Z
                float z2 = z0;
                foreach (AxisAlignedBB axis in aabbs) z2 = axis.CalculateZOffset(aabbEntity2, z2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(0, 0, z2));

                AxisAlignedBB aabbEntity3 = boundingBox.Clone();

                // Находим смещение по Y
                float y3 = y;
                foreach (AxisAlignedBB axis in aabbs) y3 = axis.CalculateYOffset(aabbEntity3, y3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(0, y3, 0));

                // Находим смещение по X
                float x3 = x0;
                foreach (AxisAlignedBB axis in aabbs) x3 = axis.CalculateXOffset(aabbEntity3, x3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(x3, 0, 0));

                // Находим смещение по Z
                float z3 = z0;
                foreach (AxisAlignedBB axis in aabbs) z3 = axis.CalculateZOffset(aabbEntity3, z3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(0, 0, z3));

                if (x2 * x2 + z2 * z2 > x3 * x3 + z3 * z3)
                {
                    x = x2;
                    z = z2;
                    aabbEntity = aabbEntity2;
                }
                else
                {
                    x = x3;
                    z = z3;
                    aabbEntity = aabbEntity3;
                }
                y = -stepHeight;

                // Находим итоговое смещение по Y
                foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);

                if (monCache.x * monCache.x + monCache.z * monCache.z >= x * x + z * z)
                {
                    // Нет авто прыжка, откатываем значение обратно
                    x = monCache.x;
                    y = monCache.y;
                    z = monCache.z;
                }
                else
                {
                    // Авто прыжок
                    SetPosition(Position + new vec3(0, y + stepHeight, 0));
                    y = 0;
                }
            }

            IsCollidedHorizontally = x0 != x || z0 != z;
            IsCollidedVertically = y0 != y;
            OnGround = IsCollidedVertically && y0 < 0.0f;
            IsCollided = IsCollidedHorizontally || IsCollidedVertically;

            Motion = new vec3(x0 != x ? 0 : x, y, z0 != z ? 0 : z);
            UpPositionMotion();

            // Определение дистанции падения, и фиксаия падения
            FallDetection(y);
        }

        /// <summary>
        /// Определяем дистанцию падения
        /// </summary>
        /// <param name="y">позиция Y</param>
        protected virtual void FallDetection(float y) { }

        private void UpPositionMotion()
        {
            motionDebug = Motion;
            SetPosition(Position + Motion);
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Обновление сущности в клиентской части
        /// </summary>
        public virtual void UpdateClient()
        {
            LastTickPos = PositionPrev = Position;
            SetPosition(PositionServer);
        }
        /// <summary>
        /// Задать позицию от сервера
        /// </summary>
        public void SetMotionServer(vec3 pos, bool onGround)
        {
            PositionServer = pos;
            OnGround = onGround;
        }

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public virtual bool CanBeCollidedWith() => false;
        /// <summary>
        /// Возвращает true, если этот объект должен толкать и толкать другие объекты при столкновении
        /// </summary>
        public virtual bool CanBePushed() => false;

        /// <summary>
        /// Получить размер границы столкновения 
        /// </summary>
        public float GetCollisionBorderSize() => .1f;

        /// <summary>
        /// Вытолкнуть из блоков
        /// </summary>
        /// <returns>true - в блоках, выталкиваем, false - не в блоках, выталкивать не надо</returns>
        protected bool PushOutOfBlocks(vec3 pos)
        {
            BlockPos blockPos = new BlockPos(pos);
            vec3 vecPos = pos - blockPos.ToVec3();
            BlockBase[] blocks = World.GetBlocksAABB(BoundingBox);

            if (blocks.Length == 0 && !World.GetAverageEdgeLengthBlock(blockPos))
            {
                return false;
            }

            byte index = 3;
            float value = 9999.0f;

            if (!World.GetAverageEdgeLengthBlock(blockPos.OffsetUp()) && 1 - vecPos.y < value)
            {
                value = 1 - vecPos.y;
                index = 3;
            }
            if (!World.GetAverageEdgeLengthBlock(blockPos.OffsetWest()) && vecPos.x < value)
            {
                value = vecPos.x;
                index = 0;
            }
            if (!World.GetAverageEdgeLengthBlock(blockPos.OffsetEast()) && 1 - vecPos.x < value)
            {
                value = 1 - vecPos.x;
                index = 1;
            }
            if (!World.GetAverageEdgeLengthBlock(blockPos.OffsetNorth()) && vecPos.z < value)
            {
                value = vecPos.z;
                index = 4;
            }
            if (!World.GetAverageEdgeLengthBlock(blockPos.OffsetSouth()) && 1 - vecPos.z < value)
            {
                value = 1 - vecPos.z;
                index = 5;
            }

            value = rand.NextFloat() * .1f + .1f;

            vec3 motion = Motion;
            if (index == 0) motion.x = -value;
            else if (index == 1) motion.x = value;
            else if (index == 3) motion.y = value;
            else if (index == 4) motion.z = -value;
            else if (index == 5) motion.z = value;
            Motion = motion;
            return true;
        }

        /// <summary>
        /// Обновлено знчение в метданных
        /// </summary>
        /// <param name="id">индекс значения какое данное было обнавлено</param>
        public virtual void UpdatedWatchedObjec(int id) { }

        /// <summary>
        /// Проверяет, находится ли этот объект внутри воды (если поле inWater имеет значение 
        /// true в результате HandleLiquidMovement()
        /// </summary>
        public virtual bool IsInWater() => inWater;
        /// <summary>
        /// Проверяет, находится ли этот объект внутри лавы (если поле inLava имеет значение 
        /// true в результате HandleLiquidMovement()
        /// </summary>
        public virtual bool IsInLava() => inLava;
        /// <summary>
        /// Проверяет, находится ли этот объект внутри нефти (если поле inOil имеет значение 
        /// true в результате HandleLiquidMovement()
        /// </summary>
        public virtual bool IsInOil() => inOil;
        /// <summary>
        /// Проверяет, находится ли этот объект внутри блоков которые тормозят (если поле inSlow имеет значение 
        /// true в результате HandleLiquidMovement()
        /// </summary>
        public virtual bool IsInSlow() => inSlow;

        /// <summary>
        /// Возвращает, если этот объект находится в воде, и в конечном итоге 
        /// добавляет скорость воды к объекту.
        /// </summary>
        protected void HandleLiquidMovement()
        {
            AxisAlignedBB axis = BoundingBox.Expand(new vec3(-.20001f, -.40001f, -.20001f)); // xz было 0.10001

            Liquid liquid = World.BeingInLiquid(axis);
            // Проверка в воде
            if (liquid.IsWater())
            {
                //if (!inWater && !this.firstUpdate)
                //{
                //    this.resetHeight();
                //}
                if (!inWater) EffectsFallingIntoWater();
                inWater = true;
                EffectsContactWithWater();
            }
            else 
            {
                if (inWater) EffectsGettingOutWater();
                inWater = false;
            }
            // Проверка в лаве
            inLava = liquid.IsLava();
            // Проверка в нефте
            inOil = liquid.IsOil();
            // Проверка замедления
            inSlow = liquid.IsSlow();

            if (inOil || inWater)
            {
                // толкаем сущность от течения
                if (IsPushedByLiquid() && liquid.IsPushedByLiquid())
                {
                    Motion = Motion + liquid.GetVec();
                }
            }
            // Проверка в огне
            if (liquid.IsFire())
            {
                // прикосновение с огнём
                DealFireDamage(1);
            }
            else if (!inLava)
            {
                // Активировать стартувую неуязвимость
                isInvulnerableBegin = 2;
            }
        }

        /// <summary>
        /// Проверка соприкосновений с другими блоками плюс прикосновение с огнём
        /// </summary>
        protected void DoBlockCollisions()
        {
            AxisAlignedBB aabb = BoundingBox.Expand(new vec3(.01f, .01f, .01f));
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();
            int minX = min.x;
            int maxX = max.x + 1;
            int minY = min.y;
            int maxY = max.y + 1;
            int minZ = min.z;
            int maxZ = max.z + 1;

            if (!World.IsAreaLoaded(minX, minY, minZ, maxX, maxY, maxZ)) return;

            BlockPos blockPos = new BlockPos();
            BlockState blockState;
            BlockBase block;
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        blockPos.X = x;
                        blockPos.Y = y;
                        blockPos.Z = z;
                        blockState = World.GetBlockState(blockPos);
                        block = blockState.GetBlock();
                        block.OnEntityCollidedWithBlock(World, blockPos, blockState, this);
                    }
                }
            }
        }

        /// <summary>
        /// Горит ли сущность
        /// </summary>
        public virtual bool InFire() => fire > 0;

        /// <summary>
        /// определяем горим ли мы, и раз в секунду %20, наносим урон
        /// </summary>
        protected bool UpdateFire()
        {
            if (fire > 1)
            {
                fire--;
                if (fire % 20 == 0)
                {
                    AttackEntityFrom(EnumDamageSource.OnFire, 1);
                    World.PlaySound(World.SampleSoundDamageFireHurt(), Position, 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Толкается ли сущность в жидкости
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPushedByLiquid() => true;

        /// <summary>
        /// Невидимый
        /// </summary>
        public virtual bool IsInvisible() => false;

        /// <summary>
        /// Воздействия при нахождении в воде
        /// </summary>
        protected virtual void EffectsContactWithWater() { }
        /// <summary>
        /// Эффект попадания в воду
        /// </summary>
        protected virtual void EffectsFallingIntoWater() { }
        /// <summary>
        /// Эффект выхода из воды
        /// </summary>
        protected virtual void EffectsGettingOutWater() { }

        /// <summary>
        /// Имеется ли у сущности иммунитет от горения в огне и лаве
        /// </summary>
        protected virtual bool IsImmuneToFire() => false;
        /// <summary>
        /// Имеется ли у сущности иммунитет к отсутствии воздуха
        /// </summary>
        protected virtual bool IsImmuneToLackOfAir() => false;
        /// <summary>
        /// Имеется ли у сущности иммунитет к падению
        /// </summary>
        protected virtual bool IsImmuneToFall() => false;
        /// <summary>
        /// Имеется ли у сущности иммунитет на всё
        /// </summary>
        protected virtual bool IsImmuneToAll() => false;
        /// <summary>
        /// Имеются ли ограничения по скорости, в воде и на блоках
        /// </summary>
        protected virtual bool IsSpeed​​Limit() => true;

        /// <summary>
        /// Получить яркость для рендера 0.0 - 1.0
        /// </summary>
        public vec2 GetBrightnessForRender()
        {
            BlockPos blockPos = new BlockPos(Position.x, Position.y + Height * .85f, Position.z);
            byte lightBlock = 0;
            byte lightSky = 0xF;
            if (blockPos.IsValid())
            {
                ChunkBase chunk = World.GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    ChunkStorage chunkStorage = chunk.StorageArrays[blockPos.Y >> 4];
                    int index = (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15);
                    lightBlock = chunkStorage.lightBlock[index];
                    lightSky = chunkStorage.lightSky[index];
                }
            }
            return new vec2(
                lightBlock / 16f + .03125f,
                lightSky / 16f + .03125f
            );
        }

        /// <summary>
        /// Высота глаз
        /// </summary>
        public virtual float GetEyeHeight() => Height * .85f;

        /// <summary>
        /// Возвращает квадрат расстояния до объекта
        /// </summary>
        public float GetDistanceSqToEntity(EntityBase entityIn)
        {
            float x = Position.x - entityIn.Position.x;
            float y = Position.y - entityIn.Position.y;
            float z = Position.z - entityIn.Position.z;
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Возвращает квадрат расстояния до координат
        /// </summary>
        public float GetDistanceSq(float x, float y, float z)
        {
            float vx = Position.x - x;
            float vy = Position.y - y;
            float vz = Position.z - z;
            return vx * vx + vy * vy + vz * vz;
        }

        /// <summary>
        /// Вызывается сущностью игрока при столкновении с сущностью 
        /// </summary>
        public virtual void OnCollideWithPlayer(EntityPlayer entityIn) { }

        public virtual void WriteEntityToNBT(TagCompound nbt)
        {
            nbt.SetTag("Pos", new TagList(new float[] { Position.x, Position.y, Position.z }));
        }

        /// <summary>
        /// Либо запишите эту сущность в указанный тег NBT и верните true, либо верните false, ничего не делая.
        /// Если это возвращает false объект не сохраняется на диске.
        /// </summary>
        public virtual bool WriteEntityToNBToptional(TagCompound nbt)
        {
            if (!IsDead && GetEntityType() != EnumEntities.None && persistenceRequired)
            {
                nbt.SetByte("Id", (byte)GetEntityType());
                WriteEntityToNBT(nbt);
                return true;
            }
            return false;
        }

        public virtual void ReadEntityFromNBT(TagCompound nbt)
        {
            TagList pos = nbt.GetTagList("Pos", 5);
            LastTickPos = PositionPrev = Position = new vec3(pos.GetFloat(0), pos.GetFloat(1) + .1f, pos.GetFloat(2));
        }

        public override string ToString() => string.Format("{0}-{1} XYZ {2}", Id, GetEntityType(), Position);
    }
}
