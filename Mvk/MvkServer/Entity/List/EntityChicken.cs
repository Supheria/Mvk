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
        public EntityChicken(WorldBase world) : base(world)
        {
            Type = EnumEntities.Chicken;
            StepHeight = 1.01f;
            samplesStep = new AssetsSample[] { AssetsSample.MobChickenStep1, AssetsSample.MobChickenStep2 };
            Speed = .05f;
            //IsFlying = true;
            persistenceRequired = true;
            if (!world.IsRemote)
            {
                tasks.AddTask(0, new EntityAISwimming(this));
                tasks.AddTask(1, new EntityAIPanic(this, 1.6f));
                //tasks.AddTask(2, new EntityAIFollowPlayer(this, 1.2f));
                tasks.AddTask(3, new EntityAIFindGrassBlockEat(this));
                tasks.AddTask(3, new EntityAIFindSaplingEat(this));
                tasks.AddTask(4, new EntityAIFollowYour(this, 1.4f));
                tasks.AddTask(5, new EntityAIWander(this, 1, .002f));
                tasks.AddTask(6, new EntityAIWatchClosest(this, 10f));
                tasks.AddTask(7, new EntityAILookIdle(this));
            }
        }

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

        int iii = 0;
        int iii2 = 0;
        bool f = true;
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
            /*
            if (World is WorldServer)
            {
                iii--;
                if (iii <= 0)
                {
                    SetRotationHead(RotationYawHead + glm.radians(rand.Next(180) - 90), RotationPitch);
                    iii = rand.Next(200) + 50;
                }
                iii2--;
                if (iii2 <= 0)
                {
                    Movement.SetAIMoveState(f, false, !f, false);

                    //Input = f ? Util.EnumInput.Forward : Util.EnumInput.Down;
                    f = !f;
                    iii2 = rand.Next(200) + 100;
                    if (f) iii2 += 200;
                }
                //InputAdd(Util.EnumInput.Down);
                //ChunkBase chunk = World.GetChunk(GetChunkPos());
                //if (chunk != null && chunk.CountEntity() > 1)
                //{
                //    Input = Util.EnumInput.Forward;
                //}
                //else
                //{
                //    Input = Util.EnumInput.None;
                //}
            }*/
        }

        protected override void LivingUpdate()
        {
            base.LivingUpdate();
            if (!OnGround && Motion.y < 0)
            {
                // При падение, курица махая крыльями падает медленее
                Motion = new vec3(Motion.x, Motion.y * .6f, Motion.z);
            }
        }

        //public override bool WriteEntityToNBToptional(TagCompound nbt) => false;
    }
}
