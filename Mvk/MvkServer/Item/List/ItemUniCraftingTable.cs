using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.TileEntity;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Абстрактный предмет крафтого стола
    /// </summary>
    public class ItemUniCraftingTable : ItemBase
    {
        /// <summary>
        /// Объект блока стола
        /// </summary>
        private readonly BlockBase blockTable;

        public ItemUniCraftingTable(EnumItem enumItem, int numberTexture, EnumBlock enumBlock) : base(enumItem, numberTexture, 1)
        {
            blockTable = Blocks.GetBlockCache(enumBlock);
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
            // Определяем положение 2*2, смещением blockPos
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

            if (CanPlaceBlocksOnSide(stack, playerIn, worldIn, blockPos, side, facing))
            {
                // Можно ставить
                BlockState blockState;
                BlockState blockState0 = new BlockState();
                BlockPos blockPosCheck;
                bool result = true;
                int x, y, z;
                for (x = 0; x < 2; x++)
                {
                    for (z = 0; z < 2; z++)
                    {
                        for (y = 0; y < 2; y++)
                        {
                            if (result)
                            {
                                blockPosCheck = blockPos.Offset(x, y, z);
                                BlockState blockStateOld = worldIn.GetBlockState(blockPosCheck);
                                ushort met = (ushort)(z << 2 | x << 1 | y);
                                blockState = new BlockState(blockTable.EBlock, met);
                                if (x == 0 && z == 0 && y == 0) 
                                {
                                    blockState0 = blockState;
                                }
                                if (!worldIn.SetBlockState(blockPosCheck, blockState, 15))
                                {
                                    result = false;
                                }
                                else
                                {
                                    if (!playerIn.IsCreativeMode)
                                    {
                                        blockStateOld.GetBlock().DropBlockAsItem(worldIn, blockPos, blockStateOld);
                                    }
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
                    worldIn.PlaySound(playerIn, blockTable.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                    
                    if (!worldIn.IsRemote)
                    {
                        // создаём тайл
                        worldIn.CreateTileEntity(EnumTileEntities.Crafting, blockPos, blockState0);
                    }

                    return true;
                }
                // Откатать на воздух!
                for (x = 0; x < 2; x++)
                {
                    for (z = 0; z < 2; z++)
                    {
                        for (y = 0; y < 2; y++)
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
            int x, y, z;
            // Проверяем объём двери
            for (x = 0; x < 2; x++)
            {
                for (z = 0; z < 2; z++)
                {
                    for (y = 0; y < 2; y++)
                    {
                        if (!CanPlaceBlockOnSide(stack, playerIn, worldIn, blockPos.Offset(x, y, z), blockTable, side, facing))
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
