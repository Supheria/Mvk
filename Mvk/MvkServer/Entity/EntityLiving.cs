using MvkServer.Entity.AI;
using MvkServer.Entity.AI.PathFinding;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.NBT;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект жизни сущьности, отвечает за движение вращение и прочее
    /// </summary>
    public abstract class EntityLiving : EntityBase
    {
        /// <summary>
        /// Поворот вокруг своей оси тела
        /// </summary>
        public float RotationYawBody { get; protected set; }
        /// <summary>
        /// Вращение головы
        /// </summary>
        public float RotationYawHead { get; protected set; }
        /// <summary>
        /// Поворот вверх вниз
        /// </summary>
        public float RotationPitch { get; protected set; }
        /// <summary>
        /// Значение на предыдущем тике поворота вокруг своей оси тела
        /// </summary>
        public float RotationYawBodyPrev { get; protected set; }
        /// <summary>
        /// Вращение головы на предыдущем тике
        /// </summary>
        public float RotationYawHeadPrev { get; protected set; }
        /// <summary>
        /// Значение на предыдущем тике поворота вверх вниз
        /// </summary>
        public float RotationPitchPrev { get; protected set; }
        /// <summary>
        /// Счётчик движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwing { get; protected set; } = 0;
        /// <summary>
        /// Скорость движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwingAmount { get; protected set; } = 0;
        /// <summary>
        /// Прыгаем
        /// </summary>
        public bool IsJumping { get; protected set; } = false;
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как «мертвая», то есть иметь в мире труп.
        /// </summary>
        public int DeathTime { get; protected set; } = 0;
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как травмированная, то есть маргает красным
        /// </summary>
        public int DamageTime { get; protected set; } = 0;
        /// <summary>
        /// Анимация боли
        /// </summary>
        public bool IsHurtAnimation { get; protected set; } = false;
        /// <summary>
        /// Значение поворота вокруг своей оси с сервера
        /// </summary>
        public float RotationYawServer { get; protected set; }
        /// <summary>
        /// Значение поворота вверх вниз с сервера
        /// </summary>
        public float RotationPitchServer { get; protected set; }
        /// <summary>
        /// Сколько тиков эта сущность прожила
        /// </summary>
        public int TicksExisted { get; private set; } = 0;
        /// <summary>
        /// Пометка что было движение и подобное для сервера, чтоб отправлять пакеты
        /// </summary>
        public EnumActionChanged ActionChanged { get; set; } = EnumActionChanged.None;
        /// <summary>
        /// Объект скорости сущности
        /// </summary>
        public float Speed { get; protected set; } = .2f;
        /// <summary>
        /// Объект параметров перемещения для игрока
        /// </summary>
        public MovementInput Movement { get; protected set; } = new MovementInput();
        /// <summary>
        /// Специфический возраст этой сущности, чем ближе к игроку возраст обнуляется, чем дальше растёт, для вероятности диспавна
        /// </summary>
        public int EntityAge { get; protected set; } = 0;

        /// <summary>
        /// Имя
        /// </summary>
        protected string name = "";
        /// <summary>
        /// Скорость движения на предыдущем тике. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        protected float limbSwingAmountPrev = 0;
        /// <summary>
        /// Анимация движения руки 0..1
        /// </summary>
        public float swingProgress = 0;
        /// <summary>
        /// Анимация движения руки предыдущего такта
        /// </summary>
        private float swingProgressPrev = 0;
        /// <summary>
        /// Счётчик тактов для анимации руки 
        /// </summary>
        private int swingProgressInt = 0;
        /// <summary>
        /// Запущен ли счётчик анимации руки
        /// </summary>
        private bool isSwingInProgress = false;
        
        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        private int jumpTicks = 0;
        /// <summary>
        /// Дистанция падения
        /// </summary>
        private float fallDistance = 0;
        /// <summary>
        /// Результат падения, отладка!!!
        /// </summary>
        protected float fallDistanceResult = 0;
        /// <summary>
        /// Оборудование, которое этот моб ранее носил, использовалось для синхронизации
        /// </summary>
        protected ItemStack[] previousEquipment = new ItemStack[0];
        

        #region Параметры для AI

        /// <summary>
        /// Объект для вращения сущности AI
        /// </summary>
        private EntityLookHelper lookHelper;
        /// <summary>
        /// Объект для перемещения сущности до координаты AI
        /// </summary>
        private EntityMoveHelper moveHelper;
        /// <summary>
        /// Объект навигации сущности
        /// </summary>
        protected PathNavigate navigator;
        /// <summary>
        /// Пасивные задачи (бродить, смотреть, бездельничать, ...)
        /// </summary>
        protected EntityAITasks tasks;
        /// <summary>
        /// Боевые задачи (используются монстрами, волками, оцелотами)
        /// </summary>
        //protected EntityAITasks targetTasks;

        /// <summary>
        /// Активная цель, которую система задач использует для отслеживания
        /// </summary>
        private EntityLiving attackTarget;

        /// <summary>
        /// Последний атакующий
        /// </summary>
        private EntityLiving lastAttacker;
        /// <summary>
        /// Содержит значение TicksExisted при последнем вызове SetLastAttacker
        /// </summary>
        private int lastAttackerTime;

        #endregion

        /// <summary>
        /// Расстояние, которое необходимо преодолеть, чтобы вызвать новый звук шага
        /// </summary>
        private int nextStepDistance = 1;
        /// <summary>
        /// Пройденное расстояние умножается на 0,3, для звукового эффекта
        /// </summary>
        private float distanceWalkedOnStepModified = 0;
        /// <summary>
        /// Семплы хотьбы
        /// </summary>
        protected AssetsSample[] samplesStep = new AssetsSample[0];
        /// <summary>
        /// Семплы сказать
        /// </summary>
        protected AssetsSample[] samplesSay = new AssetsSample[0];
        /// <summary>
        /// Семплы повреждения
        /// </summary>
        protected AssetsSample[] samplesHurt = new AssetsSample[0];

        public EntityLiving(WorldBase world) : base(world)
        {
            Standing();
            if (IsLivingUpdate())
            {
                tasks = new EntityAITasks(world);
                //targetTasks = new EntityAITasks(world);
                lookHelper = new EntityLookHelper(this);
                moveHelper = new EntityMoveHelper(this);
                navigator = InitNavigate(World);
            }
            //SpeedSurvival();
        }

        protected override void AddMetaData()
        {
            // 2 - string = пользовательский тег имени для этого объекта
            // 3 - byte = 1 / 0 Всегда отображать тег имени
            // 4 - byte =  1 / 0 Истинно, если этот объект не будет воспроизводить звуки
            // 9 - byte = количество стрел, застрявших в объекте. используется для рендеринга тех
            MetaData.Add(0, (byte)0); // флаги 0-горит; 1-крадется; 2-едет на чем-то; 3-бегает; 4-ест; 5-невидимый
            MetaData.Add(1, (short)300); // кислород
            MetaData.Add(6, GetHelathMax()); // здоровье
        }

        #region Flag

        /// <summary>
        /// Возвращает true, если флаг активен для сущности.Известные флаги:
        /// 0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый
        /// </summary>
        /// <param name="flag">0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый</param>
        protected bool GetFlag(int flag) => (MetaData.GetWatchableObjectByte(0) & 1 << flag) != 0;

        /// <summary>
        /// Включите или отключите флаг сущности
        /// 0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый; 6) спит
        /// </summary>
        /// <param name="flag">0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый</param>
        protected void SetFlag(int flag, bool set)
        {
            byte var3 = MetaData.GetWatchableObjectByte(0);
            if (set) MetaData.UpdateObject(0, (byte)(var3 | 1 << flag));
            else MetaData.UpdateObject(0, (byte)(var3 & ~(1 << flag)));
        }

        /// <summary>
        /// Ускорение
        /// </summary>
        public bool IsSprinting() => GetFlag(3);
        /// <summary>
        /// Задать значение ускоряется ли
        /// </summary>
        protected void SetSprinting(bool sprinting) => SetFlag(3, sprinting);

        /// <summary>
        /// Крадется
        /// </summary>
        public bool IsSneaking() => GetFlag(1);
        /// <summary>
        /// Задать значение крадется ли
        /// </summary>
        protected void SetSneaking(bool sneaking) => SetFlag(1, sneaking);

        /// <summary>
        /// Получить параметр кислорода
        /// </summary>
        public int GetAir() => MetaData.GetWatchableObjectShort(1);

        /// <summary>
        /// Задать параметр кислорода
        /// </summary>
        public void SetAir(int air) => MetaData.UpdateObject(1, (short)air);

        /// <summary>
        /// Горит ли сущность
        /// </summary>
        public override bool InFire() => GetFlag(0);
        /// <summary>
        /// Задать горит ли сущность
        /// </summary>
        public void SetInFire(bool fire) => SetFlag(0, fire);

        /// <summary>
        /// Принимает ли пищу
        /// </summary>
        public bool IsEating() => GetFlag(4);

        /// <summary>
        /// Задать приём пищи
        /// </summary>
        public void SetEating(bool eating) => SetFlag(4, eating);

        /// <summary>
        /// Спит ли сущность
        /// </summary>
        public bool IsSleep() => GetFlag(6);

        /// <summary>
        /// Задать спит сущность
        /// </summary>
        public void SetSleep(bool sleep) => SetFlag(6, sleep);

        /// <summary>
        /// Уровень здоровья
        /// </summary>
        public float GetHealth() => MetaData.GetWatchableObjectFloat(6);

        /// <summary>
        /// Задать уровень здоровья
        /// </summary>
        public void SetHealth(float health) => MetaData.UpdateObject(6, Mth.Clamp(health, 0, GetHelathMax()));

        #endregion

        #region Методы для AI

        /// <summary>
        /// Получить объект вращении моба AI
        /// </summary>
        public EntityLookHelper GetLookHelper() => lookHelper;
        /// <summary>
        /// Получить объект перемещения моба AI
        /// </summary>
        public EntityMoveHelper GetMoveHelper() => moveHelper;
        /// <summary>
        /// Получить объект навигации моба AI
        /// </summary>
        public PathNavigate GetNavigator() => navigator;
        /// <summary>
        /// Инициализация навигации
        /// </summary>
        protected virtual PathNavigate InitNavigate(WorldBase worldIn) => new PathNavigateGround(this, worldIn);

        /// <summary>
        /// Определяет, может ли объект исчезнуть, использовать его на бездействующих удаленных объектах.
        /// </summary>
        protected virtual bool CanDespawn() => true;

        /// <summary>
        /// Дополнительные задачи для моба
        /// </summary>
        protected virtual void UpdateAITasks() { }

        /// <summary>
        /// Сущность которая последняя атаковала
        /// </summary>
        public EntityLiving GetAITarget() => lastAttacker != null && TicksExisted - lastAttackerTime < 100 ? lastAttacker : null;

        public void SetLastAttacker(EntityLiving entity)
        {
            lastAttacker = entity;
            lastAttackerTime = TicksExisted;
        }

        /// <summary>
        /// Получает активную цель, которую система задач использует для отслеживания
        /// </summary>
        public EntityLiving GetAttackTarget() => attackTarget;

        /// <summary>
        /// Устанавливает активную цель, которую система задач использует для отслеживания
        /// </summary>
        public void SetAttackTarget(EntityLiving entityLiving) => attackTarget = entityLiving;

        #endregion

        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public override string GetName() => name == "" ? "entity." + Type.ToString() : name;

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        public override bool HasCustomName() => name != "";

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected virtual float GetHelathMax() => 10;

        /// <summary>
        /// Может дышать под водой
        /// </summary>
        public virtual bool CanBreatheUnderwater() => false;

        /// <summary>
        /// Увеличить хп
        /// </summary>
        public bool HealthAdd(int amount)
        {
            float health = GetHealth();
            if (health > 0)
            {
                float healthNew = health + amount;
                if (healthNew > GetHelathMax()) healthNew = GetHelathMax();
                if (health != healthNew)
                {
                    SetHealth(healthNew);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Нет управления
        /// </summary>
        public virtual void MovementNone() { }

        #region Action

        /// <summary>
        /// Нет действий
        /// </summary>
        public void ActionNone() => ActionChanged = EnumActionChanged.None;
        /// <summary>
        /// Добавить действие
        /// </summary>
        public void ActionAdd(EnumActionChanged input) => ActionChanged |= input;
        /// <summary>
        /// Убрать действие
        /// </summary>
        public void ActionRemove(EnumActionChanged input)
        {
            if (ActionChanged.HasFlag(input)) ActionChanged ^= input;
        }

        /// <summary>
        /// Задать действие для позиции
        /// </summary>
        protected override void ActionAddPosition() => ActionAdd(EnumActionChanged.Position);
        /// <summary>
        /// Задать действие для вращения
        /// </summary>
        protected void ActionAddLook() => ActionAdd(EnumActionChanged.Look);

        #endregion

        /// <summary>
        /// Проверяет, жив ли целевой объект
        /// </summary>
        public bool IsEntityAlive() => !IsDead && GetHealth() > 0f;

        /// <summary>
        /// Надо ли обрабатывать LivingUpdate, для мобов на сервере, и игроки у себя локально
        /// </summary>
        protected virtual bool IsLivingUpdate() => !World.IsRemote && !(this is EntityPlayer);

        /// <summary>
        /// Обновления предметов которые могут видеть игроки, что в руке, броня
        /// </summary>
        protected void UpdateItems()
        {
            if (!World.IsRemote)
            {
                // Проверка изменения ячеке и если они изменены, то отправляем видящим клиентам
                for (int slot = 0; slot < previousEquipment.Length; ++slot)
                {
                    ItemStack itemStackPrev = previousEquipment[slot];
                    ItemStack itemStackNew = GetEquipmentInSlot(slot);

                    if (!ItemStack.AreItemStacksEqual(itemStackNew, itemStackPrev))
                    {
                        ((WorldServer)World).Tracker.SendToAllTrackingEntity(this, new PacketS04EntityEquipment(Id, slot, itemStackNew));
                        previousEquipment[slot] = itemStackNew?.Copy();
                    }
                }
            }
        }

        public override void Update()
        {
            TicksExistedMore();

            base.Update();

            Movement.UpdatePlayerMoveState();

            EntityUpdate();

            if (!IsDead)
            {
                // Обновления предметов которые могут видеть игроки, что в руке, броня
                UpdateItems();

                if (IsLivingUpdate())
                {
                    // Если надо управление физики
                    LivingUpdate();
                }

                // Расчёт амплитуды движения 
                UpLimbSwing();

                // Просчёт взмаха руки
                UpdateArmSwingProgress();
            }

            // Для вращении головы
            HeadTurn();
        }

        /// <summary>
        /// Обновление сущности на сервер
        /// </summary>
        protected void EntityUpdateServer()
        {
            if (!World.IsRemote)
            {
                if (invulnerable > 0) invulnerable--;

                // определяем горим ли мы, и раз в секунду %20, наносим урон
                if (UpdateFire()) SetInFire(fire > 1);

                // определяем тонем ли мы
                DrownServer();
            }
        }

        /// <summary>
        /// Задать время сколько будет горет сущность в тактах
        /// </summary>
        public override void SetFire(int takt)
        {
            //TODO::EntityLiving Тут можно сделать проверку на броню игрока, которая будет сокращать время
            if (fire < takt)
            {
                int f = fire - (fire / 20) * 20;
                fire = takt + f;
                SetInFire(fire > 0);
            }
        }

        /// <summary>
        /// Погасить
        /// </summary>
        public void Extinguish()
        {
            if (InFire())
            {
                World.PlaySound(AssetsSample.FireFizz, Position, .7f, 1.6f + (rand.NextFloat() - rand.NextFloat()) * .4f);
                
                World.SpawnParticle(EnumParticle.Smoke, 20, new vec3(Position.x, Position.y + Height / 2, Position.z),
                    new vec3(Width * 2, Height, Width * 2), 0, 40);
            }
            fire = 0;
            SetInFire(false);
        }

        /// <summary>
        /// Определение местоположение сущности
        /// </summary>
        protected void EntityUpdateLocation()
        {
            // метод проверки нахождения по кализии в жидкостях
            HandleLiquidMovement();
            // соприкосновение с блоками, и огнём
            DoBlockCollisions();

            // определяем в лаве ли мы по кализии
            if (IsInLava())
            {
                // надо поджечь
                SetOnFireFromLava();
                fallDistance *= .5f;
            }
            if (IsInOil())
            {
                fallDistance *= .5f;
            }

            // если мы ниже -128 по Y убиваем игрока
            if (Position.y < -128)
            {
                AttackEntityFrom(EnumDamageSource.OutOfWorld, 100f);
            }
        }

        protected void EntityUpdate()
        {
            EntityUpdateLocation();

            // Только на сервере
            EntityUpdateServer();

            // Частички при беге блока
            ParticleBlockSprinting();

            // Было ли падение
            Fall();

            // если нет хп обновлям смертельную картинку
            if (GetHealth() <= 0f && DeathTime != -1) DeathUpdate();

            // Счётчик получения урона для анимации
            if (DamageTime > 0)
            {
                if (--DamageTime <= 0)
                {
                    IsHurtAnimation = false;
                }
            }
        }

        /// <summary>
        /// Уменьшает подачу воздуха сущности под водой
        /// </summary>
        protected int DecreaseAirSupply(int oldAir, bool isOil)
        {
            // TODO::EntityLiving тут чара, броня и прочее в будущем для воды
            //int air = 1; 
            return oldAir - (isOil ? 2 : 1);
            //return air > 0 && rand.Next(air + 1) > 0 ? oldAir : oldAir - 1;
        }

        /// <summary>
        /// Определяем тонем ли мы
        /// </summary>
        protected void DrownServer()
        {
            if (IsEntityAlive())
            {
                bool isWater = IsInsideOfMaterial(EnumMaterial.Water);
                bool isOil = IsInsideOfMaterial(EnumMaterial.Oil);

                if (isWater || isOil)
                {
                    if (!CanBreatheUnderwater())// && !this.isPotionActive(Potion.waterBreathing.id) && !var7)
                    {
                        SetAir(DecreaseAirSupply(GetAir(), isOil));

                        if (GetAir() == -20)
                        {
                            SetAir(0);

                            // эффект раз в секунду, и урон
                            float health = GetHealth();
                            World.SpawnParticle(EnumParticle.Bubble, 8,
                                new vec3(Position.x, Position.y + health / 2, Position.z), new vec3(Width * 2f, health, Width * 2f), 0);
                            World.PlaySound(World.SampleSoundDamageDrown(), Position, 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                            AttackEntityFrom(EnumDamageSource.Drown, 2f);
                        }
                    }

                    //if (!World.IsRemote && this.IsRiding() && this.ridingEntity instanceof EntityLivingBase)
                    //{
                    //    this.mountEntity((Entity)null);
                    //}
                }
                else
                {
                    int air = GetAir();
                    if (air < 300)
                    {
                        // воссстановление кислорода в 2 раза быстрее
                        air += 2;
                        if (air > 300) air = 300;
                        SetAir(air);
                    }
                }
            }
        }

        
        /// <summary>
        /// Сущности наносит урон только на сервере
        /// </summary>
        /// <param name="amount">сила урона</param>
        /// <returns>true - урон был нанесён</returns>
        public override bool AttackEntityFrom(EnumDamageSource source, float amount, vec3 motion, EntityLiving entityAttacks = null)
        {
            if (World.IsRemote) return false;
            EntityAge = 0;
            float health = GetHealth();
            if (health <= 0f) return false;
            if (!base.AttackEntityFrom(source, amount, motion, entityAttacks)) return false;

            // Сущность атакующего
            SetLastAttacker(entityAttacks);

            bool result = true;
            bool isLavaOrFireOrCactus = source == EnumDamageSource.Lava || source == EnumDamageSource.InFire
                || source == EnumDamageSource.Cactus || source == EnumDamageSource.CauseMobDamage;
            // иммунка на огонь и лаву в тиках
            if (isLavaOrFireOrCactus)
            {
                if (invulnerable > 0) return false;
                invulnerable = (byte)(source == EnumDamageSource.CauseMobDamage ? 10 : 5);

                if (isInvulnerableBegin > 0 && source != EnumDamageSource.Cactus && source != EnumDamageSource.CauseMobDamage)
                {
                    // При соприкосновении с огнём или лавой урон первый раз не будет наноситься, но все оповещения будут
                    amount = 0;
                    result = false;
                    isInvulnerableBegin--;
                }
            }

            health -= amount;
            if (!World.IsRemote && World is WorldServer worldServer)
            {
                SetHealth(health);
                // Анимация урона
                worldServer.Tracker.SendToAllTrackingEntityCurrent(this, new PacketS0BAnimation(Id, 
                    PacketS0BAnimation.EnumAnimation.Hurt));

                // Смещаем сущность при определённых уронах
                bool isMotion = !motion.IsZero();
                if (isLavaOrFireOrCactus)
                {
                    motion = GetRay(RotationYawHead, RotationPitch);
                    motion.y = 0;
                    motion = motion * -.2f;
                    isMotion = true;
                }

                if (health <= 0f)
                {
                    // Звук смерти
                    World.PlaySound(GetDeathSound(), Position, 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                }
                else
                {
                    // Звук урона
                    if (source != EnumDamageSource.OnFire && source != EnumDamageSource.Drown
                        && source != EnumDamageSource.Fall)
                    {
                        //PlaySoundHurt(1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                        World.PlaySound(SampleHurt(), Position, 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
                    }
                }

                if (this is EntityPlayerServer entityPlayerServer)
                {
                    if (health <= 0f)
                    {
                        worldServer.ServerMain.Log.Log("{1} {0}", source, this.name);

                        // Начало смерти
                        worldServer.Tracker.SendToAllTrackingEntity(this, new PacketS19EntityStatus(Id,
                            PacketS19EntityStatus.EnumStatus.Die));
                    }
                    else
                    {
                        // Смещаем сущность при определённых уронах
                        if (isMotion)
                        {
                            entityPlayerServer.SendPacket(new PacketS12EntityVelocity(Id, motion));
                        }
                    }
                }
                else if (isMotion) 
                {
                    SetMotion(Motion + motion);
                }
            }
            return result;
        }

        /// <summary>
        /// Возвращает звук, издаваемый этим мобом при смерти
        /// </summary>
        protected virtual AssetsSample GetDeathSound() => SampleHurt();

        /// <summary>
        /// Заставляет сущность исчезать, если требования выполнены
        /// </summary>
        private void DespawnEntity()
        {
            if (persistenceRequired)
            {
                EntityAge = 0;
            }
            else
            {
                EntityPlayer entityPlayer = World.GetClosestPlayerToEntity(this, -1f);
                if (entityPlayer != null)
                {
                    float x = entityPlayer.Position.x - Position.x;
                    float y = entityPlayer.Position.y - Position.y;
                    float z = entityPlayer.Position.z - Position.z;
                    float k = x * x + y * y + z * z;

                    if (CanDespawn() && k > 16384f)
                    {
                        SetDead();
                    }

                    if (EntityAge > 600 && rand.Next(800) == 0 && k > 1024f && CanDespawn())
                    {
                        SetDead();
                    }
                    else if (k < 1024.0D)
                    {
                        EntityAge = 0;
                    }
                }
            }
        }

       

        /// <summary>
        /// Обновляет активное действие сущности, только для мобов на сервере
        /// </summary>
        protected void UpdateEntityActionState()
        {
            EntityAge++;

            // TODO::2022-11-30 тут надо вливать перемещение моба

            // Диспавн моба.
            DespawnEntity();
            // Ощущение. (senses)

            // Определение цели, боевых задач
            //targetTasks.OnUpdateTasks();
            // Определение цели, пассивных задач
            tasks.OnUpdateTasks();
            // Навигация. Расчёт следующего шага до точки прибытия в навигации
            navigator.OnUpdateNavigation();
            // Задачи моба. 
            UpdateAITasks();
            // Перемещение. Вращение моба по вектору перемещения, и определяется длинна шага
            moveHelper.OnUpdateMove();
            // Смотрит.
            lookHelper.OnUpdateLook();
            // Прыжок. (jumpHelper.doJump)
        }

        /// <summary>
        /// Метод отвечает за жизнь сущности, точнее её управление, перемещения, мобы Ai
        /// должен работать у клиента для EntityPlayerSP и на сервере для мобов
        /// так же может работать у клиента для всех сущностей эффектов вне сервера.
        /// </summary>
        protected virtual void LivingUpdate()
        {
            // счётчик прыжка
            if (jumpTicks > 0) jumpTicks--;

            // Если нет перемещения по тактам, запускаем трение воздуха
            Motion = new vec3(Motion.x * .98f, Motion.y, Motion.z * .98f);

            // Если мелочь убираем
            Motion = new vec3(
                Mth.Abs(Motion.x) < 0.005f ? 0 : Motion.x,
                Mth.Abs(Motion.y) < 0.005f ? 0 : Motion.y,
                Mth.Abs(Motion.z) < 0.005f ? 0 : Motion.z
            );

            float strafe = 0f;
            float forward = 0f;

            if (!IsMovementBlocked())
            {
                if (!World.IsRemote)
                {
                    World.profiler.StartSection("Ai");
                    UpdateEntityActionState();
                    World.profiler.EndSection();
                }

                // Если нет блокировки
                if (IsFlying)
                {
                    float vertical = Movement.GetMoveVertical();
                    float y = Motion.y;
                    y += vertical * Speed * 1.5f;
                    Motion = new vec3(Motion.x, y, Motion.z);
                    IsJumping = false;
                }

                // Прыжок, только выживание
                else if (IsJumping)
                {
                    // для воды свои правила, плыть вверх
                    if (IsInWater()) WaterUp();
                    else if (IsInLava()) LavaUp();
                    else if (IsInOil()) OilUp();
                    // Для прыжка надо стоять на земле, и чтоб счётик прыжка был = 0
                    else if (OnGround && jumpTicks == 0)
                    {
                        Jump();
                        jumpTicks = 10;
                    }
                }
                else
                {
                    jumpTicks = 0;
                    // для воды свои правила, плыть вниз
                    if (Movement.Sneak)
                    {
                        if (IsInWater()) WaterDown();
                        else if (IsInLava()) LavaDown();
                        else if (IsInOil()) OilDown();
                    }
                }

                strafe = Movement.GetMoveStrafe();
                forward = Movement.GetMoveForward();

                // Обновить положение сидя
                bool isSneaking = IsSneaking();
                if (!IsFlying && (OnGround || IsInWater()) && Movement.Sneak && !isSneaking)
                {
                    // Только в выживании можно сесть
                    SetSneaking(true);
                    Sitting();
                }
                // Если хотим встать
                if ( !Movement.Sneak && IsSneaking())
                {
                    // Проверка коллизии вверхней части при положении стоя
                    Standing();
                    // Хочется как-то ловить колизию положение встать в MoveCheckCollision
                    if (NoClip || !World.Collision.IsCollisionBody(this, new vec3(Position)))
                    {
                        SetSneaking(false);
                    }
                    else
                    {
                        Sitting();
                    }
                }
                if (isSneaking != IsSneaking())
                {
                    ActionAdd(EnumActionChanged.IsSneaking);
                }

                // Sprinting
                bool isSprinting = Movement.Sprinting && Movement.Forward && !IsSneaking();
                if (IsSprinting() != isSprinting)
                {
                    SetSprinting(isSprinting);
                    ActionAdd(EnumActionChanged.IsSprinting);
                }

                // Jumping
                IsJumping = Movement.Jump;

                if (IsEating())
                {
                    // Если едим
                    strafe *= .4f;
                    forward *= .4f;
                    IsJumping = false;
                }
            }

            if (IsFlying)
            {
                float y = Motion.y;
                MoveWithHeading(strafe, forward, .5951f * Speed * (IsSprinting() ? 2.0f : 1f));
                Motion = new vec3(Motion.x, y * .6f, Motion.z);
            }
            else
            {
                MoveWithHeading(strafe, forward, .04f);
            }

            // Push
            CollideWithNearbyEntities();
        }

        /// <summary>
        /// Проверка колизии по вектору движения
        /// </summary>
        /// <param name="motion">вектор движения</param>
        public void UpMoveCheckCollision(vec3 motion)
        {
            MoveEntity(motion);
        }

        /// <summary>
        /// Мертвые и спящие существа не могут двигаться
        /// </summary>
        protected bool IsMovementBlocked() => GetHealth() <= 0f;

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        private void HeadTurn()
        {
            float yawOffset = RotationYawBody;

            if (swingProgress > 0)
            {
                // Анимация движении руки
                yawOffset = RotationYawHead;
            }
            else
            {
                float xDis = Position.x - PositionPrev.x;
                float zDis = Position.z - PositionPrev.z;
                float movDis = xDis * xDis + zDis * zDis;
                if (movDis > 0.0025f)
                {
                    // Движение, высчитываем угол направления
                    yawOffset = glm.atan2(zDis, xDis) + glm.pi90;
                    // Реверс для бега назад
                    float yawRev = glm.wrapAngleToPi(yawOffset - RotationYawBody);
                    if (yawRev < -1.8f || yawRev > 1.8f) yawOffset += glm.pi;
                }
            }

            float yaw2 = glm.wrapAngleToPi(yawOffset - RotationYawBody);
            RotationYawBody += yaw2 * .3f;
            float yaw3 = glm.wrapAngleToPi(RotationYawHead - RotationYawBody);

            float angleR = glm.pi45;
            if (yaw3 < -angleR) yaw3 = -angleR;
            if (yaw3 > angleR) yaw3 = angleR;

            RotationYawBody = RotationYawHead - yaw3;

            // Смещаем тело если дельта выше 60 градусов
            if (yaw3 * yaw3 > 1.1025f) RotationYawBody += yaw3 * .2f;

            RotationYawBody = glm.wrapAngleToPi(RotationYawBody);

            CheckRotation();
        }

        /// <summary>
        /// Возвращает элемент, который держит в руке
        /// </summary>
        public virtual ItemStack GetHeldItem() => null;

        /// <summary>
        /// Конвертация от направления движения в XYZ координаты с кооректировками скоростей
        /// </summary>
        protected void MoveWithHeading(float strafe, float forward, float jumpMovementFactor)
        {
            vec3 motion = new vec3();

            // трение
            float study;
            // делим на три части, вода, лава, остальное
            if (IsInWater())
            {
                float posY = Position.y;
                study = .8f; // .8f;
                // скорость
                float speed = .02f;// .02f;
                float speedEnch = 1f;//  (float)EnchantmentHelper.func_180318_b(this); чара
                //if (speedEnch > 3f) speedEnch = 3f;

                if (!OnGround) speedEnch *= .5f;

                if (speedEnch > 0f)
                {
                    study += (0.54600006f - study) * speedEnch / 3f;
                    speed += (GetAIMoveSpeed(strafe, forward) - speed) * speedEnch / 3f;
                }

                motion = MotionAngle(strafe, forward, speed);
                MoveEntity(motion);
                motion = Motion;
                motion.x *= study;
                motion.y *= .800001f;
                motion.z *= study;
                // Настроено на падение 0,4 м/с
                motion.y -= .008f;// .02f;

                // дополнительный прыжок, возле обрыва, если хотим вылести с воды на берег
                if (IsCollidedHorizontally && IsOffsetPositionInLiquid(new vec3(motion.x, motion.y + .6f - Position.y + posY, motion.z)))
                {
                    // при 0.6 запрыгиваем на блок над водой
                    // при 0.3 запрыгиваем на блок в ровень с водой
                    motion.y = 0.600001f; 
                }
            }
            else if (IsInLava() || IsInOil())
            {
                // Lava или нефть
                float posY = Position.y;
                motion = MotionAngle(strafe, forward, .02f);
                MoveEntity(motion);
                motion *= .5f;
                motion.y -= .008f;
                // дополнительный прыжок, возле обрыва, если хотим вылести с воды на берег
                if (IsCollidedHorizontally && IsOffsetPositionInLiquid(new vec3(motion.x, motion.y + .6f - Position.y + posY, motion.z)))
                {
                    // при 0.6 запрыгиваем на блок над водой
                    // при 0.3 запрыгиваем на блок в ровень с водой
                    motion.y = 0.600001f;
                }
            }
            else
            // расматриваем остальное
            {
                // Коэффициент трения блока
                //float study = .954f;// 0.91f; // для воздух
                //if (OnGround) study = 0.739F;// 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f
                study = 0.91f; // для воздух
                if (OnGround)
                {
                    study = World.GetBlockState(new BlockPos(Mth.Floor(Position.x), Mth.Floor(BoundingBox.Min.y) - 1, Mth.Floor(Position.z))).GetBlock().Slipperiness * .91f;
                }
                //if (OnGround) study = 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f

                //float param = 0.403583419f / (study * study * study);
                float param = 0.16277136f / (study * study * study);

                // трение, по умолчанию параметр ускорения падения вниз 
                float friction = jumpMovementFactor;
                if (OnGround)
                {
                    // корректировка скорости, с трением
                    friction = GetAIMoveSpeed(strafe, forward) * param;
                }

                motion = MotionAngle(strafe, forward, friction);

                // Тут надо корректировать леcтницу, вверх вниз Motion.y
                // ...

                // Проверка столкновения
                MoveEntity(motion);

                motion = Motion;
                // если есть горизонтальное столкновение и это лестница, то 
                // ... Motion.y = 0.2f;

                // Параметр падение 
                motion.y -= .16f;
                //motion.y -= .08f;

                motion.y *= .98f;
                motion.x *= study;
                motion.z *= study;
            }

            Motion = motion;
        }

        /// <summary>
        /// Скорость перемещения
        /// </summary>
        protected virtual float GetAIMoveSpeed(float strafe, float forward)
        {
            bool isSneaking = IsSneaking();
            float speed = Mth.Max(Speed * Mth.Abs(strafe), Speed * Mth.Abs(forward));
            if (IsSprinting() && forward < 0 && !isSneaking)
            {
                // Бег 
                speed *= 1.3f;
            }
            else if (!IsFlying && (forward != 0 || strafe != 0) && isSneaking)
            {
                // Крадёмся
                speed *= .3f;
            }
            return speed;
        }

        /// <summary>
        /// Значения для првжка
        /// </summary>
        protected void Jump()
        {
            // Стартовое значение прыжка, чтоб на 6 так допрыгнут наивысшую точку в 2,5 блока
            vec3 motion = new vec3(0, .84f, 0);
            if (IsSprinting())
            {
                // Если прыжок с бегом, то скорость увеличивается
                motion.x += glm.sin(RotationYawBody) * 0.4f;
                motion.z -= glm.cos(RotationYawBody) * 0.4f;
            }
            Motion = new vec3(Motion.x + motion.x, motion.y, Motion.z + motion.z);
        }
        
        /// <summary>
        /// Плыввёем вверх в воде
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void WaterUp() => Motion = new vec3(Motion.x, Motion.y + .048f, Motion.z);
        /// <summary>
        /// Плывём вниз в воде
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void WaterDown() => Motion = new vec3(Motion.x, Motion.y - .032f, Motion.z);
        /// <summary>
        /// Плыввёем вверх в лаве
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void LavaUp() => Motion = new vec3(Motion.x, Motion.y + .048f, Motion.z);
        /// <summary>
        /// Плывём вниз в лаве
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void LavaDown() => Motion = new vec3(Motion.x, Motion.y - .032f, Motion.z);
        /// <summary>
        /// Плыввёем вверх в нефте
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void OilUp() => Motion = new vec3(Motion.x, Motion.y + .048f, Motion.z);
        /// <summary>
        /// Плывём вниз в нефте
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void OilDown() => Motion = new vec3(Motion.x, Motion.y - .032f, Motion.z);

        /// <summary>
        /// Определение вращения
        /// </summary>
        protected vec3 MotionAngle(float strafe, float forward, float friction)
        {
            vec3 motion = Motion;

            float sf = strafe * strafe + forward * forward;
            if (sf >= 0.0001f)
            {
                sf = Mth.Sqrt(sf);
                if (sf < 1f) sf = 1f;
                sf = friction / sf;
                strafe *= sf;
                forward *= sf;
                float yaw = RotationYawHead;
                float ysin = glm.sin(yaw);
                float ycos = glm.cos(yaw);
                motion.x += ycos * strafe - ysin * forward;
                motion.z += ycos * forward + ysin * strafe;
            }
            return motion;
        }

        #region Frame

        /// <summary>
        /// Скорость движения для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public float GetLimbSwingAmountFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || LimbSwingAmount.Equals(limbSwingAmountPrev)) return LimbSwingAmount;
            return limbSwingAmountPrev + (LimbSwingAmount - limbSwingAmountPrev) * timeIndex;
        }

        /// <summary>
        /// Получить анимацию руки для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public virtual float GetSwingProgressFrame(float timeIndex)
        {
            if (isSwingInProgress)
            {
                if (timeIndex >= 1.0f || swingProgress.Equals(swingProgressPrev)) return swingProgress;
                return swingProgressPrev + (swingProgress - swingProgressPrev) * timeIndex;
            }
            return 0;
        }

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public virtual float GetEyeHeightFrame() => GetEyeHeight();

        /// <summary>
        /// Получить угол YawBody для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public float GetRotationYawBodyFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationYawBodyPrev == RotationYawBody) return RotationYawBody;
            return RotationYawBodyPrev + (RotationYawBody - RotationYawBodyPrev) * timeIndex;
        }

        /// <summary>
        /// Получить угол Pitch для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public float GetRotationPitchFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationPitchPrev == RotationPitch) return RotationPitch;
            return RotationPitchPrev + (RotationPitch - RotationPitchPrev) * timeIndex;
        }

        /// <summary>
        /// Получить вектор направления камеры тела
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual vec3 GetLookBodyFrame(float timeIndex)
            => GetRay(GetRotationYawFrame(timeIndex), GetRotationPitchFrame(timeIndex));

        /// <summary>
        /// Получить угол Yaw для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual float GetRotationYawFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationYawHeadPrev == RotationYawHead) return RotationYawHead;
            return RotationYawHeadPrev + (RotationYawHead - RotationYawHeadPrev) * timeIndex;
        }

        /// <summary>
        /// Получить вектор направления камеры от головы
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual vec3 GetLookFrame(float timeIndex)
            => GetRay(GetRotationYawFrame(timeIndex), GetRotationPitchFrame(timeIndex));

        #endregion

        /// <summary>
        /// Расчёт амплитуды конечностей, при движении
        /// </summary>
        protected void UpLimbSwing()
        {
            limbSwingAmountPrev = LimbSwingAmount;
            float xx = Position.x - PositionPrev.x;
            float zz = Position.z - PositionPrev.z;
            float xxzz = xx * xx + zz * zz;
            float qxxzz = Mth.Sqrt(xxzz);
            float xz = qxxzz * 1.4f;
            if (xz > 1.0f) xz = 1.0f;
            LimbSwingAmount += (xz - LimbSwingAmount) * 0.4f;
            LimbSwing += LimbSwingAmount;

            distanceWalkedOnStepModified += qxxzz * .3f;
            if (distanceWalkedOnStepModified > nextStepDistance)
            {
                BlockBase blockDown = World.GetBlockState(new BlockPos(Position.x, Position.y - 0.20002f, Position.z)).GetBlock();
                if (!blockDown.IsAir)
                {
                    nextStepDistance = (int)distanceWalkedOnStepModified + 1;
                    SoundStep(blockDown);
                }
            }
        }

        public void PlaySound(AssetsSample key, float volume, float pitch)
            => World.PlaySound(this, key, Position, volume, pitch);

        /// <summary>
        /// Звуковой эффект шага
        /// </summary>
        protected void SoundStep(BlockBase blockDown)
        {
            if (IsInWater())
            {
                // Звук в воде
                SoundEffectWater(World.SampleSoundInTheWater(), .35f);
            }
            else if (!IsSneaking() && blockDown.Material != EnumMaterial.Water && IsSampleStep(blockDown))
            {
                // Звук шага
                World.PlaySound(this, SampleStep(World, blockDown), Position, .25f, 1f);
            }
        }

        private void SoundEffectWater(AssetsSample assetsSample, float volumeFX)
        {
            // Звук в воде
            float xx = Position.x - PositionPrev.x;
            float yy = Position.y - PositionPrev.y;
            float zz = Position.z - PositionPrev.z;
            // В зависимости от скорости перемещения вводе меняем звук
            float volume = Mth.Sqrt(xx * xx * .2f + yy * yy + zz * zz * .2f) * volumeFX;
            // 0.03 - 0.2 когда плыву, 0.4 - 0.7 когда ныряю
            if (volume > 1f) volume = 1f;

            World.PlaySound(this, assetsSample, Position, volume, 1f + (rand.NextFloat() - rand.NextFloat()) * .4f);
        }

        /// <summary>
        /// Эффект попадания в воду
        /// </summary>
        protected override void EffectsFallingIntoWater()
        {
            SoundEffectWater(World.SampleFallingIntoWater(), .2f);
            vec3 pos = new vec3(0, BoundingBox.Min.y + 1f, 0);
            vec3 motion = Motion;
            float width = Width * 2f;
            World.SpawnParticle(EnumParticle.Bubble, 10,
                    new vec3(Position.x, Position.y - .125f, Position.z), new vec3(Width * 2f, .25f, Width * 2f), 0);

            //for(int i = 0; i < 10; i++)
            //{
            //    motion.y = Motion.y - (float)rand.NextDouble() * .2f;
            //    pos.x = Position.x + ((float)rand.NextDouble() * 2f - 1f) * width;
            //    pos.z = Position.z + ((float)rand.NextDouble() * 2f - 1f) * width;
            //    World.SpawnParticle(EnumParticle.Suspend, pos, motion);
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    motion.y = Motion.y - (float)rand.NextDouble() * .2f;
            //    pos.x = Position.x + ((float)rand.NextDouble() * 2f - 1f) * width;
            //    pos.z = Position.z + ((float)rand.NextDouble() * 2f - 1f) * width;
            //    World.SpawnParticle(EnumParticle.Digging, pos, motion, 1);
            //}
        }

        /// <summary>
        /// Воздействия при нахождении в воде
        /// </summary>
        protected override void EffectsContactWithWater()
        {
            fallDistance = 0f;
            Extinguish();
        }

        #region Sound Samples

        /// <summary>
        /// Есть ли звуковой эффект шага
        /// </summary>
        public virtual bool IsSampleStep(BlockBase blockDown) => samplesStep.Length > 0;
        /// <summary>
        /// Семпл хотьбы
        /// </summary>
        public virtual AssetsSample SampleStep(WorldBase worldIn, BlockBase blockDown) => samplesStep[worldIn.Rnd.Next(samplesStep.Length)];

        /// <summary>
        /// Есть ли звуковой эффект сказать
        /// </summary>
        protected bool IsSampleSay() => samplesSay.Length > 0;
        /// <summary>
        /// Семпл сказать
        /// </summary>
        protected AssetsSample SampleSay() => samplesSay[World.Rnd.Next(samplesSay.Length)];

        /// <summary>
        /// Есть ли звуковой эффект повреждения
        /// </summary>
        protected bool IsSampleHurt() => samplesHurt.Length > 0;
        /// <summary>
        /// Семпл повреждения
        /// </summary>
        protected AssetsSample SampleHurt() => samplesHurt[World.Rnd.Next(samplesHurt.Length)];

        /// <summary>
        /// Проиграть звук сказать
        /// </summary>
        public void PlaySoundSay(float volume = 1, float pitch = 1)
        {
            if (IsSampleSay()) PlaySound(SampleSay(), volume, pitch);
        }

        /// <summary>
        /// Проиграть звук повреждения
        /// </summary>
        public void PlaySoundHurt(float volume = 1, float pitch = 1)
        {
            if (IsSampleHurt()) PlaySound(SampleHurt(), volume, pitch);
        }

        #endregion

        /// <summary>
        /// Скакой скоростью анимируется удар рукой, в тактах, менять можно от инструмента, чар и навыков
        /// </summary>
        public int GetArmSwingAnimationEnd() => 6; 

        /// <summary>
        /// Размахивает предметом, который держит игрок
        /// </summary>
        public virtual void SwingItem()
        {
            if (!isSwingInProgress || swingProgressInt >= GetArmSwingAnimationEnd() / 2 || swingProgressInt == 0)
            {
                swingProgressInt = 0; /// -1;
                isSwingInProgress = true;

                if (World is WorldServer)
                {
                    ((WorldServer)World).Tracker.SendToAllTrackingEntity(this, new PacketS0BAnimation(Id, PacketS0BAnimation.EnumAnimation.SwingItem));
                    //((WorldServer)World).Players.ResponsePacketAll(new PacketS0BAnimation(Id, PacketS0BAnimation.EnumAnimation.SwingItem), Id);
                }
            }
        }

        /// <summary>
        /// Обновляет счетчики прогресса взмаха руки и прогресс анимации. 
        /// </summary>
        protected void UpdateArmSwingProgress()
        {
            swingProgressPrev = swingProgress;

            int asa = GetArmSwingAnimationEnd();

            if (isSwingInProgress)
            {
                swingProgressInt++;
                if (swingProgressInt >= asa)
                {
                    swingProgressInt = 0;
                    isSwingInProgress = false;
                }
            }
            else
            {
                swingProgressInt = 0;
            }

            swingProgress = (float)swingProgressInt / (float)asa;
        }

        ///// <summary>
        ///// Задать положение сидя и бега
        ///// </summary>
        //public virtual void SetSneakingSprinting(bool sneaking, bool sprinting)
        //{
        //    if (IsSneaking() != sneaking)
        //    {
        //        SetSneaking(sneaking);
        //        if (sneaking) Sitting(); else Standing();
        //    }
        //    SetSprinting(sprinting);
        //}

        /// <summary>
        /// Задать позицию от сервера
        /// </summary>
        public void SetMotionServer(vec3 pos, float yaw, float pitch, bool onGround)
        {
            //if (IsSneaking != sneaking)
            //{
            //    IsSneaking = sneaking;
            //    if (IsSneaking) Sitting(); else Standing();
            //}
            //IsSprinting = sprinting;
            RotationYawServer = yaw;
            RotationPitchServer = pitch;
            SetMotionServer(pos, onGround);
        }

        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public virtual void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            SetPosition(pos);
            SetLook(yaw, pitch);
        }

        private void SetLook(float yaw, float pitch)
        {
            SetRotation(yaw, pitch);
            RotationYawHeadPrev = RotationYawHead = RotationYawServer = RotationYawBodyPrev = RotationYawBody;
            RotationPitchServer = RotationPitchPrev = RotationPitch;
            PositionServer = PositionPrev = LastTickPos = Position;
        }

        /// <summary>
        /// Дополнительное обновление сущности в клиентской части в зависимости от сущности
        /// </summary>
        protected virtual void UpdateEntityRotation()
        {
            RotationYawHeadPrev = RotationYawHead;
            SetRotationHead(RotationYawServer, RotationPitchServer);
            //SetRotation(RotationYawServer, RotationPitchServer);
        }

        /// <summary>
        /// Управляет таймером смерти сущности, сферой опыта и созданием частиц
        /// </summary>
        protected void DeathUpdate()
        {
            DeathTime++;

            if (DeathTime >= 20)
            {
                World.SpawnParticle(EnumParticle.Test, 100, Position, new vec3(Width, .25f, Width), 0);
                DeathTime = -1;
                SetDead();

                //Health = 20;
                //    int var1;

                //    if (!this.worldObj.isRemote && (this.recentlyHit > 0 || this.isPlayer()) && this.func_146066_aG() && this.worldObj.getGameRules().getGameRuleBooleanValue("doMobLoot"))
                //    {
                //        var1 = this.getExperiencePoints(this.attackingPlayer);

                //        while (var1 > 0)
                //        {
                //            int var2 = EntityXPOrb.getXPSplit(var1);
                //            var1 -= var2;
                //            this.worldObj.spawnEntityInWorld(new EntityXPOrb(this.worldObj, this.posX, this.posY, this.posZ, var2));
                //        }
                //    }

                //    this.setDead();

                //    for (var1 = 0; var1 < 20; ++var1)
                //    {
                //        double var8 = this.rand.nextGaussian() * 0.02D;
                //        double var4 = this.rand.nextGaussian() * 0.02D;
                //        double var6 = this.rand.nextGaussian() * 0.02D;
                //        this.worldObj.spawnParticle(EnumParticleTypes.EXPLOSION_NORMAL, this.posX + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width, this.posY + (double)(this.rand.nextFloat() * this.height), this.posZ + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width, var8, var4, var6, new int[0]);
                //    }
            }
        }

        /// <summary>
        /// Возобновить сущность
        /// </summary>
        public virtual void Respawn()
        {
            SetHealth(GetHelathMax());
            DeathTime = 0;
            fallDistanceResult = 0f;
            fallDistance = 0f;
            Motion = new vec3(0);
            invulnerable = 100;
            //immunityLava = 0;
            //immunityFire = 0;
            //immunity = false;
            fire = 0;
            SetInFire(false);
            IsDead = false;
        }

        /// <summary>
        /// Начать анимацию боли
        /// </summary>
        public void PerformHurtAnimation()
        {
            DamageTime = 5; // количество тактов
            IsHurtAnimation = true;
        }
        /// <summary>
        /// Начать анимацию выздоровления
        /// </summary>
        public void PerformRecoveryAnimation() => DamageTime = 5; // количество тактов

        /// <summary>
        /// Определяем дистанцию падения
        /// </summary>
        /// <param name="y">позиция Y</param>
        protected override void FallDetection(float y)
        {
            if (!IsInWater())
            {
                HandleLiquidMovement();
            }

            if (IsFlying) fallDistance = 0;
            else if (y < 0f) fallDistance -= y;
            if (OnGround)
            {
                if (IsFlying)
                {
                    ModeSurvival();
                    fallDistance = 0f;
                }
                else if (fallDistance > 0f)
                {
                    // Упал
                    fallDistanceResult = fallDistance;
                    fallDistance = 0f;
                }
            }
        }

        /// <summary>
        /// Падение
        /// </summary>
        protected virtual void Fall() { }

        /// <summary>
        /// Добавить тик жизни к сущности
        /// </summary>
        protected void TicksExistedMore() => TicksExisted++;

        /// <summary>
        /// Обновление сущности в клиентской части
        /// </summary>
        public override void UpdateClient()
        {
            base.UpdateClient();
            RotationPitchPrev = RotationPitch;
            RotationYawBodyPrev = RotationYawBody;
            UpdateEntityRotation();
        }

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected virtual void Standing() => SetSize(.6f, 3.6f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected virtual void Sitting() => SetSize(.6f, 2.99f);

        /// <summary>
        /// Активация режима полёта
        /// </summary>
        public void ModeFly()
        {
            if (!IsFlying)
            {
                IsFlying = true;
                Standing();
            }
        }

        /// <summary>
        /// Активация режима выживания
        /// </summary>
        public virtual void ModeSurvival() { }

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public override bool CanBeCollidedWith() => !IsDead;

        /// <summary>
        /// Получить стак что в правой руке 0 или броня 1-4
        /// </summary>
        public virtual ItemStack GetEquipmentInSlot(int slot) => null;
        /// <summary>
        /// Получить слот брони 0-3
        /// </summary>
        public virtual ItemStack GetCurrentArmor(int slot) => null;
        /// <summary>
        /// Задать стак в слот, что в правой руке 0, или 1-4 слот брони
        /// </summary>
        public virtual void SetCurrentItemOrArmor(int slot, ItemStack itemStack) { }

        /// <summary>
        /// Вызывается всякий раз, когда предмет подбирается
        /// </summary>
        public void ItemPickup(EntityBase entity, int amount)
        {
            if (!entity.IsDead && !World.IsRemote)
            {
                EntityTracker tracker = ((WorldServer)World).Tracker;
                if (entity is EntityItem)
                {
                    tracker.SendToAllTrackingEntityCurrent(this, new PacketS0DCollectItem(Id, entity.Id));
                }
            }
        }

        /// <summary>
        /// Частички при беге блока
        /// </summary>
        private void ParticleBlockSprinting()
        {
            if (IsSprinting() && !IsInWater())
            {
                ParticleBlockDown(Position, 1);
                // TODO::звук при беге
            }
        }
        /// <summary>
        /// Частички при падении
        /// </summary>
        public void ParticleFall(float distance)
        {
            if (distance > 3)
            {
                int count = (int)distance + 2;
                if (count > 20) count = 20;

                BlockBase blockDown = World.GetBlockState(new BlockPos(Position.x, Position.y - 0.20002f, Position.z)).GetBlock();
                if (blockDown.IsParticle)
                {
                    World.SpawnParticle(EnumParticle.BlockPart, count * 5, Position,
                        new vec3(Width * 2, 0, Width * 2), 0, (int)blockDown.EBlock);
                }

                // Звук при падении
                if (distance > 12)
                {
                    World.PlaySound(this, AssetsSample.DamageFallBig, Position, 1, 1);
                }
                else if(distance > 4)
                {
                    World.PlaySound(this, AssetsSample.DamageFallSmall, Position, .5f, 1);
                }

                // Звук блока под нагами
                if (IsSampleStep(blockDown))
                {
                    World.PlaySound(this, SampleStep(World, blockDown), Position, .5f, .75f);
                }
            }
        }

        /// <summary>
        /// Частички под ногами, бег или падение
        /// </summary>
        private void ParticleBlockDown(vec3 pos, int count)
        {
            BlockBase block = World.GetBlockState(new BlockPos(pos.x, pos.y - 0.20002f, pos.z)).GetBlock();
            if (block.IsParticle)
            {
                World.SpawnParticle(EnumParticle.BlockPart, count, pos, 
                    new vec3(Width * 2, 0, Width * 2), 0, (int)block.EBlock);
            }
        }

        

        /// <summary>
        /// Проверяет, находится ли смещенная позиция от текущей позиции объекта внутри жидкости
        /// </summary>
        /// <param name="vec">вектор смещения</param>
        private bool IsOffsetPositionInLiquid(vec3 vec)
        {
            AxisAlignedBB aabb = BoundingBox.Offset(vec);
            return World.Collision.GetCollidingBoundingBoxes(aabb).Count == 0
                && !World.IsAnyLiquid(aabb);
        }

        /// <summary>
        /// Проверяет, относится ли текущий блок объекта находящий на глазах к указанному типу материала
        /// </summary>
        /// <param name="materialIn"></param>
        public bool IsInsideOfMaterial(EnumMaterial materialIn)
        {
            float y = Position.y + GetEyeHeight();
            BlockPos blockPos = new BlockPos(Position.x, y, Position.z);
            BlockState blockState = World.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();

            if (block.Material == materialIn)
            {
                // нужна проверка течении воды, у неё блок не целый
                if (block.EBlock == EnumBlock.WaterFlowing || block.EBlock == EnumBlock.OilFlowing || block.EBlock == EnumBlock.LavaFlowing)
                {
                    float h = blockState.met / 15f;
                    float h2 = blockPos.Y + h;
                    return y < h2;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Создает частицу взрыва вокруг местоположения Сущности
        /// </summary>
        public void SpawnExplosionParticle()
        {
            //if (this.worldObj.isRemote)
            //{
            //    for (int var1 = 0; var1 < 20; ++var1)
            //    {
            //        double var2 = this.rand.nextGaussian() * 0.02D;
            //        double var4 = this.rand.nextGaussian() * 0.02D;
            //        double var6 = this.rand.nextGaussian() * 0.02D;
            //        double var8 = 10.0D;
            //        this.worldObj.spawnParticle(EnumParticleTypes.EXPLOSION_NORMAL, this.posX + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width - var2 * var8, this.posY + (double)(this.rand.nextFloat() * this.height) - var4 * var8, this.posZ + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width - var6 * var8, var2, var4, var6, new int[0]);
            //    }
            //}
            //else
            //{
            //    this.worldObj.setEntityState(this, (byte)20);
            //}
        }

        /// <summary>
        /// Визуализирует частицы сломанных предметов, используя заданный ItemStack
        /// </summary>
        /// <param name="p_70669_1_"></param>
        public void RenderBrokenItemStack(EnumBlock p_70669_1_)
        {
        //    this.playSound("random.break", 0.8F, 0.8F + this.worldObj.rand.nextFloat() * 0.4F);

        //    for (int var2 = 0; var2< 5; ++var2)
        //    {
        //        Vec3 var3 = new Vec3(((double)this.rand.nextFloat() - 0.5D) * 0.1D, Math.random() * 0.1D + 0.1D, 0.0D);
        //var3 = var3.rotatePitch(-this.rotationPitch* (float) Math.PI / 180.0F);
        //var3 = var3.rotateYaw(-this.rotationYaw* (float) Math.PI / 180.0F);
        //double var4 = (double)(-this.rand.nextFloat()) * 0.6D - 0.3D;
        //Vec3 var6 = new Vec3(((double)this.rand.nextFloat() - 0.5D) * 0.3D, var4, 0.6D);
        //var6 = var6.rotatePitch(-this.rotationPitch* (float) Math.PI / 180.0F);
        //var6 = var6.rotateYaw(-this.rotationYaw* (float) Math.PI / 180.0F);
        //var6 = var6.addVector(this.posX, this.posY + (double)this.getEyeHeight(), this.posZ);
        //        this.worldObj.spawnParticle(EnumParticleTypes.ITEM_CRACK, var6.xCoord, var6.yCoord, var6.zCoord, var3.xCoord, var3.yCoord + 0.05D, var3.zCoord, new int[] { Item.getIdFromItem(p_70669_1_.getItem()) });
        //    }
        }

        // Визуализирует частицы сломанных предметов, используя заданный ItemStack
        // renderBrokenItemStack

        /// <summary>
        /// Максимальная высота, с которой объекту разрешено прыгать (используется в навигаторе)
        /// </summary>
        public virtual int GetMaxFallHeight() => 2;

        

        /// <summary>
        /// Получить вектор направления по поворотам
        /// </summary>
        public static vec3 GetRay(float yaw, float pitch)
        {
            //float var3 = glm.cos(-yaw - glm.pi);
            //float var4 = glm.sin(-yaw - glm.pi);
            //float var5 = -glm.cos(-pitch);
            //float var6 = glm.sin(-pitch);
            //return new vec3(var4 * var5, var6, var3 * var5);

            float pitchxz = glm.cos(pitch);
            return new vec3(glm.sin(yaw) * pitchxz, glm.sin(pitch), -glm.cos(yaw) * pitchxz);
        }

        /// <summary>
        /// Задать вращение
        /// </summary>
        protected void SetRotation(float yaw, float pitch)
        {
            RotationYawBody = yaw;
            RotationPitch = pitch;
            CheckRotation();
            ActionAddLook();
        }

        /// <summary>
        /// Задать вращение
        /// </summary>
        public void SetRotationHead(float yawHead, float pitch)
        {
            RotationYawHead = yawHead;
            SetRotation(RotationYawBody, pitch);
        }

        /// <summary>
        /// Проверить градусы
        /// </summary>
        private void CheckRotation()
        {
            while (RotationYawHead - RotationYawHeadPrev < -glm.pi) RotationYawHeadPrev -= glm.pi360;
            while (RotationYawHead - RotationYawHeadPrev >= glm.pi) RotationYawHeadPrev += glm.pi360;
            while (RotationYawBody - RotationYawBodyPrev < -glm.pi) RotationYawBodyPrev -= glm.pi360;
            while (RotationYawBody - RotationYawBodyPrev >= glm.pi) RotationYawBodyPrev += glm.pi360;
            while (RotationPitch - RotationPitchPrev < -glm.pi) RotationPitchPrev -= glm.pi360;
            while (RotationPitch - RotationPitchPrev >= glm.pi) RotationPitchPrev += glm.pi360;
        }

        /// <summary>
        /// Возвращает true, если этот объект должен толкать и толкать другие объекты при столкновении
        /// </summary>
        public override bool CanBePushed() => true;

        /// <summary>
        /// Столкновение с ближайшими объектами
        /// </summary>
        protected virtual void CollideWithNearbyEntities()
        {
            List<EntityBase> list = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving,
               BoundingBox.Expand(new vec3(.2f, 0, .2f)), Id);

            if (list.Count > 0)
            {
                AxisAlignedBB aabb = BoundingBox.Clone();
                for (int i = 0; i < list.Count; i++)
                {
                    EntityBase entity = list[i];
                    if (entity.CanBePushed() && entity is EntityLiving entityLiving)
                    {
                        entityLiving.ApplyEntityCollision(this);
                    }
                }
            }
        }

        /// <summary>
        /// Применяет скорость к каждому из объектов, отталкивая их друг от друга.
        /// </summary>
        public void ApplyEntityCollision(EntityLiving entityIn)
        {
            //if (entityIn.riddenByEntity != this && entityIn.ridingEntity != this)
            {
                if (!entityIn.NoClip && !NoClip)
                {
                    float x = entityIn.Position.x - Position.x;
                    float z = entityIn.Position.z - Position.z;
                    float k = Mth.Max(Mth.Abs(x), Mth.Abs(z));

                    if (k >= 0.00999999f)
                    {
                        k = Mth.Sqrt(k);
                        x /= k;
                        z /= k;
                        float k2 = 1f / k;
                        if (k2 > 1f) k2 = 1f;

                        x *= k2;
                        z *= k2;
                        x *= .05f;
                        z *= .05f;

                        //if (this.riddenByEntity == null)
                        {
                            AddVelocity(new vec3(-x, 0, -z));
                        }

                        //if (entityIn.riddenByEntity == null)
                        {
                            entityIn.AddVelocity(new vec3(x, 0, z));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Изменить вектор смещения
        /// </summary>
        public void AddVelocity(vec3 motion) => Motion += motion;

        public override void WriteEntityToNBT(TagCompound nbt)
        {
            base.WriteEntityToNBT(nbt);
            nbt.SetTag("Rotation", new TagList(new float[] { RotationYawHead, RotationPitch }));
            nbt.SetShort("Health", (short)(GetHealth() * 10));
            nbt.SetShort("DeathTime", (short)DeathTime);
            //nbt.SetString("Name", GetName());
        }

        public override void ReadEntityFromNBT(TagCompound nbt)
        {
            base.ReadEntityFromNBT(nbt);
            TagList rotation = nbt.GetTagList("Rotation", 5);
            SetLook(rotation.GetFloat(0), rotation.GetFloat(1));
            SetHealth(nbt.GetShort("Health") / 10f);
            DeathTime = nbt.GetShort("DeathTime");
            //name = nbt.GetString("Name");
        }

        public override string ToString()
        {
            vec3 m = motionDebug;
            m.y = 0;
            vec3 my = new vec3(0, motionDebug.y, 0);
            float rotationYawHead = glm.degrees(RotationYawHead);
            Pole pole = EnumFacing.FromAngle(RotationYawHead);
            return string.Format("{15}-{16} XYZ {7} ch:{12}\r\n{0:0.000} | {13:0.000} м/c\r\nHealth: {14:0.00} Air: {17}\r\nyaw:{8:0.00} H:{9:0.00} pitch:{10:0.00} {18}-{22} \r\n{1}{2}{6}{4}{19}{20}{21} boom:{5:0.00}\r\nMotion:{3}\r\n{11}",
                glm.distance(m) * 10f, // 0
                OnGround ? "__" : "", // 1
                IsSprinting() ? "[Sp]" : "", // 2
                Motion, // 3
                IsJumping ? "[J]" : "", // 4
                fallDistanceResult, // 5
                IsSneaking() ? "[Sn]" : "", // 6
                Position, // 7
                glm.degrees(RotationYawBody), // 8
                rotationYawHead, // 9
                glm.degrees(RotationPitch), // 10
                IsCollidedHorizontally, // 11
                GetChunkPos(), // 12
                glm.distance(my) * 10f, // 13
                GetHealth(), // 14
                Id, // 15
                Type, // 16
                GetAir(), // 17
                pole, // 18
                IsInWater() ? "[W]" : "", // 19
                IsInLava() ? "[L]" : "", // 20
                IsInOil() ? "[O]" : "", // 21
                EnumFacing.IsFromAngleLeft(RotationYawHead, pole) // 22
                );
        }
    }
}
