using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Sound;

namespace MvkServer.World
{
    /// <summary>
    /// Продолжение мира, но тут дополнительные звуковые эффекты
    /// </summary>
    abstract partial class WorldBase
    {
        private readonly AssetsSample[] samplesFallingIntoWater = new AssetsSample[] { AssetsSample.LiquidSplash1, AssetsSample.LiquidSplash2 };
        private readonly AssetsSample[] samplesSoundInTheWater = new AssetsSample[] { AssetsSample.LiquidSwim1, AssetsSample.LiquidSwim2, AssetsSample.LiquidSwim3, AssetsSample.LiquidSwim4 };
        private readonly AssetsSample[] samplesSoundEat = new AssetsSample[] { AssetsSample.Eat1, AssetsSample.Eat2, AssetsSample.Eat3 };
        private readonly AssetsSample[] samplesDamageDrown = new AssetsSample[] { AssetsSample.DamageDrown1, AssetsSample.DamageDrown2, AssetsSample.DamageDrown3, AssetsSample.DamageDrown4 };
        private readonly AssetsSample[] samplesDamageFireHurt = new AssetsSample[] { AssetsSample.DamageFireHurt1, AssetsSample.DamageFireHurt2, AssetsSample.DamageFireHurt3 };

        /// <summary>
        /// Семпл попадания в воду
        /// </summary>
        public AssetsSample SampleFallingIntoWater() => samplesFallingIntoWater[Rnd.Next(samplesFallingIntoWater.Length)];
        /// <summary>
        /// Семпл звук в воде
        /// </summary>
        public AssetsSample SampleSoundInTheWater() => samplesSoundInTheWater[Rnd.Next(samplesSoundInTheWater.Length)];
        /// <summary>
        /// Семпл звук еды
        /// </summary>
        public AssetsSample SampleSoundEat() => samplesSoundEat[Rnd.Next(samplesSoundEat.Length)];
        /// <summary>
        /// Семпл звук питья
        /// </summary>
        public AssetsSample SampleSoundDrink() => AssetsSample.Drink;
        /// <summary>
        /// Семпл звук урон от утопления
        /// </summary>
        public AssetsSample SampleSoundDamageDrown() => samplesDamageDrown[Rnd.Next(samplesDamageDrown.Length)];
        /// <summary>
        /// Семпл звук урон от огня
        /// </summary>
        public AssetsSample SampleSoundDamageFireHurt() => samplesDamageFireHurt[Rnd.Next(samplesDamageFireHurt.Length)];

        /// <summary>
        /// Проиграть звуковой эффект
        /// </summary>
        public virtual void PlaySound(EntityLiving entity, AssetsSample key, vec3 pos, float volume, float pitch) { }

        /// <summary>
        /// Проиграть звуковой эффект только для клиента
        /// </summary>
        public virtual void PlaySound(AssetsSample key, vec3 pos, float volume, float pitch) { }
    }
}
