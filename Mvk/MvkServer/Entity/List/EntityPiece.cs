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
            => type = EnumEntities.Piece;
        public EntityPiece(WorldBase world, EntityLiving entityThrower) : base(world, entityThrower)
            => type = EnumEntities.Piece;

        protected override void OnImpact(MovingObjectPosition moving, bool isLiquid)
        {
            if (!World.IsRemote)
            {
                if (moving.IsCollision())
                {
                    // TODO::2023-05-29 EntityPiece-OnImpact-IsCollision Всё тут мне ненравиться, надо как-то разделить на корректность методов, чтоб вызывались при определённой связи к примеру на что ударили, и кем ударили
                    EnumItem eItem = (EnumItem)MetaData.GetWatchableObjectInt(10);
                    ItemBase item = Items.GetItemCache(eItem);
                    float power = item.GetImpactStrength();
                    bool isBreak = true;

                    if (moving.IsBlock())
                    {
                        BlockBase blockBase = moving.Block.GetBlock();
                        float resistance = blockBase.Resistance;
                        power += power * (World.Rnd.NextFloat() - World.Rnd.NextFloat()) * .3f;
                        resistance += resistance * (World.Rnd.NextFloat() - World.Rnd.NextFloat()) * .3f;

                        isBreak = power > resistance;

                        if (!isLiquid)
                        {
                            if (isBreak)
                            {
                                // блок разрушаем
                                BlockState blockState = World.GetBlockState(moving.BlockPosition);
                                blockState.GetBlock().DropBlockAsItem(World, moving.BlockPosition, blockState);
                                World.SetBlockToAir(moving.BlockPosition, 15);
                            }
                            if (eItem == EnumItem.Coconut)
                            {
                                ItemStack itemStack;
                                vec3 pos = moving.RayHit;
                                if (!isBreak && blockBase.Material.EMaterial == EnumMaterial.Solid)
                                {
                                    // Разбивается кокос на две половинки
                                    itemStack = new ItemStack(Items.GetItemCache(EnumItem.HalfCoconut), World.Rnd.Next(5) == 0 ? 2 : 1);
                                    World.PlaySoundPop(pos);
                                    World.SetBlockState(moving.BlockPosition.Offset(moving.Norm), new BlockState(EnumBlock.WaterFlowing, 7), 14);
                                }
                                else
                                {
                                    // Спавним этот же предмет, но не как метательную сущность
                                    isBreak = false;
                                    itemStack = new ItemStack(Items.GetItemCache(eItem));
                                }
                                // Траекторию полёта в зависимости в какую сторону блока ударили
                                vec3 motion = new vec3(Motion.x * .2f, Motion.y * .4f, Motion.z * .2f);
                                Pole side = moving.Side;
                                if (side == Pole.Up) motion.y = -motion.y;
                                else if (side == Pole.Down) pos.y -= Height;
                                else if (side == Pole.East || side == Pole.West)
                                {
                                    motion.x = -motion.x;
                                    pos.x += side == Pole.West ? -Width : Width;
                                }
                                else if (side == Pole.South || side == Pole.North)
                                {
                                    motion.z = -motion.z;
                                    pos.z += side == Pole.North ? -Width : Width;
                                }
                                // Спавним сущность предмета
                                EntityItem entityItem = new EntityItem(World, pos, itemStack);
                                entityItem.SetMotion(motion);
                                entityItem.SetDefaultPickupDelay();
                                World.SpawnEntityInWorld(entityItem);
                            }
                            else if (eItem == EnumItem.PieceStone)
                            {
                                if (World.Rnd.NextFloat() < .25f)
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
                            else if (!isBreak) isBreak = true;
                        }
                        else if (!isBreak) isBreak = true;
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
