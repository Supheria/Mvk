using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.List
{

    /// <summary>
    /// Сущность кусок
    /// </summary>
    public class EntitySpawn : EntityThrowable
    {
        public EntitySpawn(WorldBase world) : base(world)
            => Type = EnumEntities.Piece;
        public EntitySpawn(WorldBase world, EntityPlayer entityPlayer) : base(world, entityPlayer)
            => Type = EnumEntities.Piece;

        protected override void OnImpact(MovingObjectPosition moving, bool isLiquid)
        {
            if (!World.IsRemote && moving.IsBlock())
            {
                EnumItem eItem = (EnumItem)MetaData.GetWatchableObjectInt(10);
                ItemBase item = Items.GetItemCache(eItem);


                // частички разрушения предмета
                vec3 pos = moving.RayHit;
                //vec3 pos = moving.BlockPosition.Offset(moving.Side).ToVec3();
                World.SpawnParticle(EnumParticle.ItemPart, 32, pos, new vec3(1), 0, (int)eItem);

                //if (!isLiquid)
                {
                    // Спавн
                    //for (int i = 0; i < 10; i++)
                    {
                        EnumEntities enumEntities = GetEnumEntities(eItem);
                        if (enumEntities != EnumEntities.None)
                        {
                            EntityBase entity = Entities.CreateEntityByEnum(World, enumEntities);
                            vec3 normal = new vec3(moving.Norm);
                            pos += normal * entity.Width;
                            //pos.x += World.Rnd.NextFloat() - .5f;
                            //pos.z += World.Rnd.NextFloat() - .5f;
                            entity.SetPosition(pos);// moving.RayHit);
                            //entity.SetPosition(moving.RayHit);
                            World.SpawnEntityInWorld(entity);
                        }
                    }
                }
            }
        }

        private EnumEntities GetEnumEntities(EnumItem item)
        {
            switch (item)
            {
                case EnumItem.SpawnChicken: return EnumEntities.Chicken;
                case EnumItem.SpawnChemoglot: return EnumEntities.Chemoglot;
                case EnumItem.SpawnPakan: return EnumEntities.Pakan;
            }
            return EnumEntities.None;
        }
    }
}
