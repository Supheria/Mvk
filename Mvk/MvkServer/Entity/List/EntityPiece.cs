using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность спавним
    /// </summary>
    public class EntityPiece : EntityThrowable
    {
        public EntityPiece(WorldBase world) : base(world)
            => Type = EnumEntities.Piece;
        public EntityPiece(WorldBase world, EntityLiving entityThrower) : base(world, entityThrower)
            => Type = EnumEntities.Piece;

        protected override void OnImpact(MovingObjectPosition moving, bool isLiquid)
        {
            if (!World.IsRemote)
            {
                if (moving.IsCollision())
                {
                    EnumItem eItem = (EnumItem)MetaData.GetWatchableObjectInt(10);
                    ItemBase item = Items.GetItemCache(eItem);
                    float power = item.GetImpactStrength();
                    bool isBreak = true;

                    if (moving.IsBlock())
                    {
                        float resistance = moving.Block.GetBlock().Resistance;
                        power += power * (World.Rnd.NextFloat() - World.Rnd.NextFloat()) * .3f;
                        resistance += resistance * (World.Rnd.NextFloat() - World.Rnd.NextFloat()) * .3f;

                        isBreak = power > resistance;

                        if (!isLiquid)
                        {
                            if (isBreak)
                            {
                                // блок разрушаем
                                World.SetBlockToAir(moving.BlockPosition, 15);
                            }

                            if (eItem == EnumItem.PieceStone && World.Rnd.NextFloat() < .25f)
                            {
                                // Камень с 25% вероятности может остатся
                                //BlockPos blockPosDown = isBreak ? moving.BlockPosition.OffsetDown() : moving.BlockPosition;
                                //BlockPos blockPosUp = isBreak ? moving.BlockPosition : moving.BlockPosition.OffsetUp();
                                BlockPos blockPos = isBreak ? moving.BlockPosition : moving.BlockPosition.OffsetUp();
                                BlockBase blockStone = Blocks.GetBlockCache(EnumBlock.SmallStone);
                                if (blockStone.CanBlockStay(World, blockPos))
                                {
                                    World.SetBlockState(blockPos, new BlockState(EnumBlock.SmallStone), 14);
                                    isBreak = true;
                                }
                            }
                        }
                        if (!isBreak || isLiquid) isBreak = true;
                    }
                    else if (moving.IsEntity())
                    {
                        moving.Entity.AttackEntityFrom(EnumDamageSource.Piece, power, Motion * .2f, EntityThrower);
                    }
                    if (isBreak)
                    {
                        // частички разрушения предмета
                        World.SpawnParticle(EnumParticle.ItemPart, 32, Position + Motion * .5f, new vec3(1), 0, (int)eItem);
                    }
                }
            }
        }
    }
}
