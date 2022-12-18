using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет блок
    /// </summary>
    public class ItemBlock : ItemBase
    {
        public BlockBase Block { get; private set; }

        public ItemBlock(BlockBase block)
        {
            Block = block;
            EItem = EnumItem.Block;
            UpId();
        }

        public override string GetName() => "block." + Block.EBlock;

        /// <summary>
        ///  Вызывается, когда блок щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool OnItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            if (!worldIn.GetBlockState(blockPos).GetBlock().IsReplaceable)
            {
                blockPos = blockPos.Offset(side);
            }
            if (CanPlaceBlockOnSide(stack, playerIn, worldIn, blockPos, Block, side, facing))
            {
                if (Block.CanBlockStay(worldIn, blockPos))
                {
                    BlockState blockStateOld = worldIn.GetBlockState(blockPos);
                    BlockState blockState = Block.OnBlockPlaced(worldIn, blockPos, new BlockState(Block.EBlock), side, facing);
                    bool result = worldIn.SetBlockState(blockPos, blockState, 15);
                    if (result)
                    {
                        if (!playerIn.IsCreativeMode)
                        {
                            blockStateOld.GetBlock().DropBlockAsItem(worldIn, blockPos, blockStateOld, 0);
                            if (stack.Item != null)
                            {
                                playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
                            }
                        }
                        worldIn.PlaySound(playerIn, Block.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                    }
                    return result;
                }
            }
            return false;
        }
    }
}
