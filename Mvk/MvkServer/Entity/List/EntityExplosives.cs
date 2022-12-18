using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность взрывчатки
    /// </summary>
    public class EntityExplosives : EntityThrowable
    {
        public EntityExplosives(WorldBase world) : base(world)
            => Type = EnumEntities.Piece;
        public EntityExplosives(WorldBase world, EntityPlayer entityPlayer) : base(world, entityPlayer)
            => Type = EnumEntities.Piece;

        protected override void OnImpact(MovingObjectPosition moving, bool isLiquid)
        {
            if (!World.IsRemote)
            {
                if (moving.IsCollision())
                {
                    EnumItem eItem = (EnumItem)MetaData.GetWatchableObjectInt(10);
                    ItemBase item = Items.GetItemCache(eItem);
                    if (item is ItemUniExplosives itemExplosives)
                    {
                        vec3 pos = new vec3();
                        bool b = false;
                        if (moving.IsBlock())
                        {
                            pos = moving.BlockPosition.ToVec3() + .5f;
                            b = true;
                        }
                        else if (moving.IsEntity())
                        {
                            pos = moving.Entity.Position;
                            b = true;
                        }
                        if (b) 
                        {
                            World.CreateExplosion(pos, itemExplosives.GetStrength(), itemExplosives.GetDistance());
                        }
                    }
                }
            }
        }
    }
}
