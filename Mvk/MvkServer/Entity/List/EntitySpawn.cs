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
                        EntityBase entity = Entities.CreateEntityByEnum(World, EnumEntities.Chicken);
                        //pos.x += World.Rnd.NextFloat() - .5f;
                        //pos.z += World.Rnd.NextFloat() - .5f;
                        entity.SetPosition(pos);// moving.RayHit);
                        World.SpawnEntityInWorld(entity);
                    }
                }
            }
        }
    }
}
