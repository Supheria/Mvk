using MvkServer.Entity.AI;
using MvkServer.Entity.AI.PathFinding;
using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность курицы
    /// </summary>
    public class EntityChicken : EntityLiving
    {
        /// <summary>
        /// Время до следующего яйца
        /// </summary>
        private int timeUntilNextEgg;
        /// <summary>
        /// высиживаем яйцо
        /// </summary>
        private bool hatchingAnEgg = false;
        /// <summary>
        /// Сколько раз поела после высиживания яйца
        /// </summary>
        public int countEat = 0;

        /// <summary>
        /// Задача по поиску гнезда для того чтоб высесть яйцо
        /// </summary>
        private EntityAIFindNestEgg entityAIFindNest;

        public EntityChicken(WorldBase world) : base(world)
        {
            Type = EnumEntities.Chicken;
            StepHeight = 1.01f;
            samplesStep = new AssetsSample[] { AssetsSample.MobChickenStep1, AssetsSample.MobChickenStep2 };
            samplesSay = new AssetsSample[] { AssetsSample.MobChickenSay1, AssetsSample.MobChickenSay2, AssetsSample.MobChickenSay3 };
            samplesHurt = new AssetsSample[] { AssetsSample.MobChickenHurt1, AssetsSample.MobChickenHurt2 };
            Speed = .05f;
            //IsFlying = true;
            persistenceRequired = true;
            if (!world.IsRemote)
            {
                tasks.AddTask(0, new EntityAISwimming(this));
                tasks.AddTask(1, new EntityAIPanic(this, 1.6f));
                tasks.AddTask(2, new EntityAIFindNestSleep(this)); 
                //tasks.AddTask(2, new EntityAIFollowPlayer(this, 1.2f));
                tasks.AddTask(3, new EntityAIFindSaplingEat(this, .004f));
                tasks.AddTask(3, entityAIFindNest = new EntityAIFindNestEgg(this, 1.2f));
                tasks.AddTask(4, new EntityAIFollowYour(this, .002f, 1.4f));
                tasks.AddTask(4, new EntityAIWander(this, .002f));
                tasks.AddTask(5, new EntityAIWatchClosest(this, 10f));
                tasks.AddTask(6, new EntityAILookIdle(this));
            }
            timeUntilNextEgg = World.Rnd.Next(6000) + 1000;
            SetPosHomeNull();
        }

        protected override void AddMetaData()
        {
            base.AddMetaData();
            MetaData.AddByDataType(10, 0); // 1 знаем где гнездо, 0 незнаем где гнездо
            MetaData.Add(11, new BlockPos(0, 0, 0)); // положение гнезда
        }

        public void SetPosHome(BlockPos blockPos)
        {
            MetaData.UpdateObject(10, (byte)1);
            MetaData.UpdateObject(11, blockPos);
        }

        public void SetPosHomeNull()
        {
            MetaData.UpdateObject(10, (byte)0);
            MetaData.UpdateObject(11, new BlockPos(0, 0, 0));
        }

        /// <summary>
        /// Имеется ли у курицы дом
        /// </summary>
        public bool IsPosHome() => MetaData.GetWatchableObjectByte(10) == 1;

        /// <summary>
        /// Позиция дома
        /// </summary>
        public BlockPos GetPosHome() => MetaData.GetWatchableObjectBlockPos(11);

        /// <summary>
        /// Может дышать под водой
        /// </summary>
        //public override bool CanBreatheUnderwater() => true;

        /// <summary>
        /// Инициализация навигации
        /// </summary>
        //protected override PathNavigate InitNavigate(WorldBase worldIn) => new PathNavigateSwimmer(this, worldIn);
        protected override PathNavigate InitNavigate(WorldBase worldIn)
        {
            PathNavigateGround navigate = new PathNavigateGround(this, worldIn);
            //navigate.CanSwim(true);
            //navigate.SetAvoidWater(false);
            return navigate;
        }


        /// <summary>
        /// Положение стоя
        /// </summary>
        protected override void Standing() => SetSize(.3f, .99f);
        //protected override void Standing() => SetSize(.6f, 1.49f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected override void Sitting() => SetSize(.3f, .7f);

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 5;

        /// <summary>
        /// Сущности наносит урон только на сервере
        /// </summary>
        /// <param name="amount">сила урона</param>
        /// <returns>true - урон был нанесён</returns>
        //public override bool AttackEntityFrom(EnumDamageSource source, float amount, vec3 motion, string name = "")
        //{
        //    entityAIWander.InstantExecution();
        //    return base.AttackEntityFrom(source, amount, motion, name);
        //}

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();
            PositionPrev = Position;
            if (!OnGround)
            {
                // При падении лапки двигаются в разы медленее
                LimbSwingAmount *= 0.25f;
            }
        }

        protected override void LivingUpdate()
        {
            base.LivingUpdate();
            if (!OnGround && Motion.y < 0)
            {
                // При падение, курица махая крыльями падает медленее
                Motion = new vec3(Motion.x, Motion.y * .6f, Motion.z);
            }

            if (!World.IsRemote)
            {
                if (!hatchingAnEgg)
                {
                    timeUntilNextEgg -= countEat;
                    if (timeUntilNextEgg <= 0)
                    {
                        hatchingAnEgg = true;
                        entityAIFindNest.run = true;
                    }
                }
                else if (hatchingAnEgg && !entityAIFindNest.ContinueExecuting())
                {
                    timeUntilNextEgg = World.Rnd.Next(6000) + 6000;
                    hatchingAnEgg = false;
                }
            }


        }

        public override void WriteEntityToNBT(TagCompound nbt)
        {
            base.WriteEntityToNBT(nbt);
            bool home = IsPosHome();
            if (home)
            {
                nbt.SetBool("Home", home);
                BlockPos blockPos = GetPosHome();
                nbt.SetInt("HomeX", blockPos.X);
                nbt.SetInt("HomeY", blockPos.Y);
                nbt.SetInt("HomeZ", blockPos.Z);
            }
            nbt.SetShort("Egg", (short)timeUntilNextEgg);
        }

        public override void ReadEntityFromNBT(TagCompound nbt)
        {
            base.ReadEntityFromNBT(nbt);
            bool home = nbt.GetBool("Home");
            if (home)
            {
                BlockPos blockPos = new BlockPos(nbt.GetInt("HomeX"), nbt.GetInt("HomeY"), nbt.GetInt("HomeZ"));
                SetPosHome(blockPos);
            }
            else
            {
                SetPosHomeNull();
            }
            timeUntilNextEgg = nbt.GetShort("Egg");
        }

        //public override bool WriteEntityToNBToptional(TagCompound nbt) => false;
    }
}
