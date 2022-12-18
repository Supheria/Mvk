using MvkServer.Glm;
using MvkServer.World;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Пламя
    /// </summary>
    public class EntityFlameFX : EntityFX
    {
        /// <summary>
        /// Начальный масштаб частицы
        /// </summary>
        private readonly float flameScale;

        public EntityFlameFX(WorldBase world, vec3 pos, vec3 motion) : base(world, pos, motion)
        {
            textureUV = new vec2i(0, 3);
            color = new vec3(1);
            motion.x = motion.x * .009f + motion.x;
            motion.y = motion.y * .009f + motion.y;
            motion.z = motion.z * .009f + motion.z;
            Motion = motion;
            particleMaxAge += 40;
            flameScale = particleScale;
            NoClip = true;
        }

        public override void Update()
        {
            particleScale = flameScale * (1 + (glm.cos(particleAge * .2f) + 2f) * .2f);
            LastTickPos = PositionPrev = Position;

            if (particleAge++ >= particleMaxAge) SetDead();

            vec3 motion = Motion * .9599f;
            if (OnGround)
            {
                motion.x *= .6999f;
                motion.z *= .6999f;
            }
            MoveEntity(motion);
        }
    }
}
