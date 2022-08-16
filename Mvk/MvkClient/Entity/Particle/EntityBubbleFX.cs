using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Эффект пузыря
    /// </summary>
    public class EntityBubbleFX : EntityFX
    {
        public EntityBubbleFX(WorldBase world, vec3 pos, vec3 motion) : base(world, pos, motion)
        {
            textureUV = new vec2i(0, 2);
            color = new vec3(1);
            SetSize(.04f, .04f);
            particleScale *= rand.NextFloat() + .4f;
            motion.x = motion.x * .2f + (rand.NextFloat() * 2f - 1f) * .01999f;
            motion.y = motion.y * .2f + (rand.NextFloat() * 2f - 1f) * .01999f;
            motion.z = motion.z * .2f + (rand.NextFloat() * 2f - 1f) * .01999f;
            Motion = motion;
            particleMaxAge = (int)(8f / rand.NextFloat() * .8f + .2f);
        }

        public override void Update()
        {
            base.Update();

            LastTickPos = PositionPrev = Position;
            vec3 motion = Motion;
            motion.y += .002f;

            // Проверка столкновения
            MoveEntity(motion);
            motion = Motion * .85f;

            if (World.GetBlockState(new BlockPos(GetBlockPos())).GetBlock().Material != EnumMaterial.Water
                || particleAge-- <= 0)
            {
                SetDead();
            }
        }
    }
}
