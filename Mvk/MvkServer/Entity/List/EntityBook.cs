using MvkServer.Entity.AI;
using MvkServer.Entity.AI.PathFinding;
using MvkServer.NBT;
using MvkServer.Sound;
using MvkServer.World;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность книга
    /// </summary>
    public class EntityBook : EntityLiving
    {
        public EntityBook(WorldBase world) : base(world)
        {
            IsFlying = true;
            type = EnumEntities.Book;
            StepHeight = 1.01f;
            samplesHurt = new AssetsSample[] { AssetsSample.MobBookAttack };
            samplesSay = new AssetsSample[] { AssetsSample.MobBookSay1, AssetsSample.MobBookSay2, AssetsSample.MobBookSay3 };
            Speed = .04f;
            if (!world.IsRemote)
            {
                tasks.AddTask(1, new EntityAIAttackOnCollide(this, .5f));
                // нужно, чтоб перепрыгивать когда приследует игрока
                tasks.AddTask(2, new EntityAIHop(this)); 
                tasks.AddTask(3, new EntityAIFollowPlayer(this, 1f, false));
                tasks.AddTask(4, new EntityAIWanderFly(this, .05f));
                tasks.AddTask(5, new EntityAILookIdle(this));
                tasks.AddTask(5, new EntityAISay(this));
            }
        }

        /// <summary>
        /// Инициализация навигации
        /// </summary>
        protected override PathNavigate InitNavigate(WorldBase worldIn) => new PathNavigateFly(this, worldIn);

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected override void Standing() => SetSize(.48f, 1f);

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 5;

        /// <summary>
        /// Вызывается сущностью игрока при столкновении с сущностью 
        /// </summary>
        public override void OnCollideWithPlayer(EntityPlayer entityIn)
        {
            //if (entityIn.AttackEntityFrom(EnumDamageSource.CauseMobDamage, .25f))
            //{
            //    PlaySound(SampleHurt(), 1, (rand.NextFloat() - rand.NextFloat()) * .2f + 1f);
            //}
        }

        /// <summary>
        /// Возвращает звук, издаваемый этим мобом при смерти
        /// </summary>
        protected override AssetsSample GetDeathSound() => AssetsSample.MobBookDeath;

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
        }

        //public override void Col
        //protected override void LivingUpdate()
        //{
        //    base.LivingUpdate();
        //    if (!OnGround && Motion.y < 0)
        //    {
        //        // При падение, курица махая крыльями падает медленее
        //        Motion = new vec3(Motion.x, Motion.y * .6f, Motion.z);
        //    }
        //}

        public override bool WriteEntityToNBToptional(TagCompound nbt) => false;
    }
}
