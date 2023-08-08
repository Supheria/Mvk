using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет Кремень и сталь
    /// </summary>
    public class ItemFlintAndSteel : ItemBase
    {
        /// <summary>
        /// Блок огня
        /// </summary>
        protected BlockBase blockFire;

        public ItemFlintAndSteel() : base(EnumItem.FlintAndSteel, 100, 1)
        {
            blockFire = Blocks.GetBlockCache(EnumBlock.Fire);
            MaxDamage = 100;
        }

        /// <summary>
        ///  Вызывается, когда предмет щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool OnItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            if (!worldIn.IsRemote)
            {
                if (!worldIn.GetBlockState(blockPos).GetBlock().IsReplaceable)
                {
                    blockPos = blockPos.Offset(side);
                }

                if (worldIn.GetBlockState(blockPos).IsAir() && stack.Item != null
                    && CanPlaceBlockOnSide(stack, playerIn, worldIn, blockPos, blockFire, side, facing))
                {
                    BlockState blockState = blockFire.OnBlockPlaced(worldIn, blockPos, new BlockState(blockFire.EBlock), side, facing);
                    if (blockFire.CanBlockStay(worldIn, blockPos, blockState.met))
                    {
                        if (worldIn.Rnd.Next(10) != 0)
                        {
                            // Только урон, без зажигания
                            stack.DamageItem(worldIn, 1, playerIn, blockPos, true);
                            worldIn.PlaySound(AssetsSample.FireIgnite, blockPos.ToVec3() + .5f, 1f, worldIn.Rnd.NextFloat() * .2f + .4f);
                            return false;
                        }
                        // Вероятность зажигания всего 10%
                        BlockState blockStateOld = worldIn.GetBlockState(blockPos);
                        bool result = worldIn.SetBlockState(blockPos, blockState, 15);
                        if (result)
                        {
                            if (!playerIn.IsCreativeMode)
                            {
                                blockStateOld.GetBlock().DropBlockAsItem(worldIn, blockPos, blockStateOld);
                                stack.DamageItem(worldIn, 1, playerIn, blockPos, true);
                            }
                            worldIn.PlaySound(AssetsSample.FireIgnite, blockPos.ToVec3() + .5f, 1f, worldIn.Rnd.NextFloat() * .4f + .8f);
                        }
                        return result;
                    }
                }
                return false;
            }
            return true;
        }

    }
}
