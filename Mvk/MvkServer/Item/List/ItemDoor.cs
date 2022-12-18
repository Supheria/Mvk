using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Block.List;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет дверь
    /// </summary>
    public class ItemDoor : ItemBase
    {
        /// <summary>
        /// Объект блока двери
        /// </summary>
        private readonly BlockUniDoor blockDoor;

        public ItemDoor(EnumItem enumItem, int numberTexture, EnumBlock enumBlock)
        {
            EItem = enumItem;
            NumberTexture = numberTexture;
            blockDoor = (BlockUniDoor)Blocks.GetBlockCache(enumBlock);
            UpId();
        }

        /// <summary>
        /// Вызывается, когда предмет щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool OnItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            // Только на что-то ставим, значит ориентир на пол
            if (side != Pole.Up) return false;

            if (!worldIn.GetBlockState(blockPos).GetBlock().IsReplaceable)
            {
                blockPos = blockPos.Offset(side);
            }

            // Определяем на какую сторону смотрит игрок
            Pole pole = EnumFacing.FromAngle(playerIn.RotationYawHead);
            // Определение с какой стороны петля
            bool left = EnumFacing.IsFromAngleLeft(playerIn.RotationYawHead, pole);
            // Определяем положение двери 2*2, смещением blockPos
            blockPos = blockPos.Offset(pole);

            if (pole == Pole.East)
            {
                blockPos.X--;
                if (!left) blockPos.Z--;
            }
            else if (pole == Pole.South)
            {
                blockPos.Z--;
                if (left) blockPos.X--;
            }
            else if (pole == Pole.North && !left) blockPos.X--;
            else if (pole == Pole.West && left) blockPos.Z--;

            Pole poleOpposite = EnumFacing.GetOpposite(pole);

            if (CanPlaceBlocksOnSide(stack, playerIn, worldIn, blockPos, side, facing) 
                && blockDoor.IsCanDoorStay(worldIn, blockPos, false, left, poleOpposite))
            {
                // Можно ставить

                // Определяем поворот двери
                int indexPole = (int)poleOpposite - 2;

                BlockPos blockPosCheck;
                bool result = true;
                for (int x = 0; x < 2; x++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            blockPosCheck = blockPos.Offset(x, y, z);
                            BlockState blockStateOld = worldIn.GetBlockState(blockPosCheck);
                            ushort met = BlockUniDoor.ConvMetData(false, left, indexPole, y, x, z);
                            if (!worldIn.SetBlockState(blockPosCheck, new BlockState(blockDoor.EBlock, met), 15))
                            {
                                result = false;
                            }
                            else
                            {
                                if (!playerIn.IsCreativeMode)
                                {
                                    blockStateOld.GetBlock().DropBlockAsItem(worldIn, blockPos, blockStateOld, 0);
                                }
                            }
                        }
                    }
                }

                if (result)
                {
                    // инвентарь, и звук
                    if (!playerIn.IsCreativeMode)
                    {
                        if (stack.Item != null)
                        {
                            playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
                        }
                    }
                    worldIn.PlaySound(playerIn, blockDoor.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                    return true;
                }
                // Откатать на воздух!
                for (int x = 0; x < 2; x++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            worldIn.SetBlockToAir(blockPos.Offset(x, y, z));
                        }
                    }
                }
            }
            return false;
        }

        private bool CanPlaceBlocksOnSide(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            // Проверяем объём двери
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        if (!CanPlaceBlockOnSide(stack, playerIn, worldIn, blockPos.Offset(x, y, z), blockDoor, side, facing))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


    }
}
