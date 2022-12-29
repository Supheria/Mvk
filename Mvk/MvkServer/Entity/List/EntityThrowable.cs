using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность метательная
    /// </summary>
    public class EntityThrowable : EntityBase
    {
        /// <summary>
        /// Кто метнул предмет
        /// </summary>
        public EntityLiving EntityThrower { get; protected set; }
        /// <summary>
        /// Сколько тиков жизни
        /// </summary>
        private int age = 0;

        public EntityThrowable(WorldBase world) : base(world) => SetSize(.25f, .5f);

        public EntityThrowable(WorldBase world, EntityLiving entityThrower) : this(world)
        {
            EntityThrower = entityThrower;
            vec3 pos = entityThrower.Position;
            pos.x += glm.cos(entityThrower.RotationYawHead) * .32f;
            pos.z += glm.sin(entityThrower.RotationYawHead) * .32f;
            pos.y += entityThrower.GetEyeHeight() - .2f;
            SetPosition(pos);
            float pitchxz = glm.cos(entityThrower.RotationPitch);
            vec3 motion = new vec3(
                glm.sin(entityThrower.RotationYawHead) * pitchxz * 1.4f,
                glm.sin(entityThrower.RotationPitch) * 1.4f,
                -glm.cos(entityThrower.RotationYawHead) * pitchxz * 1.4f
            );
            float f1 = rand.NextFloat() * .02f;
            float f2 = rand.NextFloat() * glm.pi360;
            motion.x += glm.cos(f2) * f1;
            motion.z += glm.sin(f2) * f1;
            Motion = motion;
        }

        /// <summary>
        /// Задать предмет в сущность
        /// </summary>
        public void SetItem(EnumItem item)
        {
            MetaData.UpdateObject(10, (int)item);
            //MetaData.SetObjectWatched(10);
        }

        protected override void AddMetaData() => MetaData.AddByDataType(10, 2);

        /// <summary>
        /// Возвращает ItemStack, соответствующий Entity 
        /// (Примечание: если предмет не существует, регистрируется ошибка, 
        /// но все равно возвращается ItemStack, содержащий Block.stone)
        /// </summary>
        public ItemStack GetEntityItemStack()
        {
            try
            {
                EnumItem item = (EnumItem)MetaData.GetWatchableObjectInt(10);
                ItemStack itemStack = new ItemStack(Items.GetItemCache(item));
                return itemStack;
            }
            catch(Exception ex)
            {
                World.Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Вызывается, когда этот EntityThrowable сталкивается с блоком или сущностью.
        /// </summary>
        protected virtual void OnImpact(MovingObjectPosition moving, bool isLiquid)
        {
            //if (!World.IsRemote && moving.IsBlock())
            //{
                //if (World.GetBlockState(moving.BlockPosition.OffsetUp()).IsAir())
                //{
                //    World.SetBlockState(moving.BlockPosition.OffsetUp(), new BlockState(EnumBlock.SmallStone), 14);
                //}
                //World.CreateExplosion(moving.BlockPosition.ToVec3() + .5f, 7f, 5);
                //World.SetBlockToAir(moving.BlockPosition);
            //}
           
        }

        /// <summary>
        /// Получает величину гравитации за каждый тик
        /// </summary>
        protected virtual float GetGravityVelocity() => .06f;

        public override void Update()
        {
            if (GetEntityItemStack() == null || Position.y < -16 || age >= 1000)
            {
                SetDead();
                return;
            }

            base.Update();
            age++;

            MovingObjectPosition moving = World.RayCastBlock(Position, Motion.normalize(), glm.distance(Motion), false);

            if (!World.IsRemote)
            {
                // Рамка траектории одного тика метательного снаряда, т.е. от позиции до момента перемещения, плюс рамка 0.2
                AxisAlignedBB axis = BoundingBox.AddCoordBias(Motion).Expand(new vec3(.2f));
                // Собираем все сущности которые могут соприкосатся с рамкой снаряда
                List<EntityBase> list = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axis, -1);
                MovingObjectPosition moving2 = null;
                EntityBase entityHit = null;
                EntityBase entity;
                float distance = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    entity = list[i];
                    // Проверка может ли сущность быть в проверке
                    if (entity.CanBeCollidedWith() && (entity.Id != EntityThrower.Id || age > 5))
                    {
                        // Дополнительная обработка рамки, с целью проверить точное косание сущности
                        moving2 = entity.BoundingBox.Expand(new vec3(.3f)).CalculateIntercept(Position, Position + Motion);
                        if (moving2 != null)
                        {
                            // Если косание сущности было фиксируем, и фиксируем на иближайшее к позиции прошлого такта
                            float f = glm.distance(Position, moving2.RayHit);
                            if (f < distance || distance == 0)
                            {
                                entityHit = entity;
                                distance = f;
                            }
                        }
                    }
                }
                if (entityHit != null)
                {
                    // Помечаем что было поподание по сущности для OnImpact
                    moving = new MovingObjectPosition(entityHit);
                }
            }

            bool isWater = IsInWater();
            bool isLiquid = isWater || IsInLava() || IsInOil();

            if (moving.IsCollision())
            {
                // столкновение с блоком или сущностью
                OnImpact(moving, isLiquid);
                SetDead();
            }
            else
            {
                if (isWater)
                {
                    World.SpawnParticle(EnumParticle.Bubble, 4, Position + Motion * .25f, new vec3(.5f), 0);
                }

                PositionPrev = Position;
                SetPosition(Position + Motion);

                vec3 motion = Motion * (isLiquid ? .8f : .99f);
                motion.y -= GetGravityVelocity();
                Motion = motion;

                // Если мелочь убираем
                Motion = new vec3(
                    Mth.Abs(Motion.x) < 0.005f ? 0 : Motion.x,
                    Mth.Abs(Motion.y) < 0.005f ? 0 : Motion.y,
                    Mth.Abs(Motion.z) < 0.005f ? 0 : Motion.z
                );
                HandleLiquidMovement();
            }
        }

        public override void WriteEntityToNBT(TagCompound nbt)
        {
            base.WriteEntityToNBT(nbt);
            nbt.SetTag("Motion", new TagList(new float[] { Motion.x, Motion.y, Motion.z }));
            nbt.SetShort("Age", (short)age);
            nbt.SetShort("ItemId", (short)MetaData.GetWatchableObjectInt(10));
        }

        public override void ReadEntityFromNBT(TagCompound nbt)
        {
            base.ReadEntityFromNBT(nbt);
            TagList motion = nbt.GetTagList("Motion", 5);
            Motion = new vec3(motion.GetFloat(0), motion.GetFloat(1), motion.GetFloat(2));
            age = nbt.GetShort("Age");
            SetItem((EnumItem)nbt.GetShort("ItemId"));
        }
    }
}
