using MvkServer.Entity.AI;
using MvkServer.Entity.AI.PathFinding;
using MvkServer.NBT;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность чемоглот
    /// </summary>
    public class EntityChemoglot : EntityLiving
    {
        public EntityChemoglot(WorldBase world) : base(world)
        {
            type = EnumEntities.Chemoglot;
            StepHeight = 1.01f;
            samplesStep = new AssetsSample[] { AssetsSample.MobChemoglotJump1, AssetsSample.MobChemoglotJump2, AssetsSample.MobChemoglotJump3, AssetsSample.MobChemoglotJump4 };
            samplesHurt = new AssetsSample[] { AssetsSample.MobChemoglotAttack1, AssetsSample.MobChemoglotAttack2 };
            Speed = .1f;
            if (!world.IsRemote)
            {
                tasks.AddTask(0, new EntityAISwimming(this));
                tasks.AddTask(1, new EntityAIAttackOnCollide(this, .5f));
                tasks.AddTask(1, new EntityAIHop(this));
                tasks.AddTask(2, new EntityAIFollowPlayer(this, 1f, false));
                tasks.AddTask(3, new EntityAIWander(this, .05f));
                tasks.AddTask(4, new EntityAILookIdle(this));
            }
        }

        /// <summary>
        /// Инициализация навигации
        /// </summary>
        protected override PathNavigate InitNavigate(WorldBase worldIn) => new PathNavigateGround(this, worldIn);

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected override void Standing() => SetSize(.95f, 1.4f);

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 10;

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
        protected override AssetsSample GetDeathSound() => AssetsSample.MobChemoglotDeath;

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
