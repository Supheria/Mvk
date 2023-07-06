using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна пальмы
    /// </summary>
    public class BlockLogPalm : BlockUniLog
    {
        protected EnumBlock leaves;
        protected int idLeaves;
        // ширина кроны
        protected readonly int crownWidth = 4;
        // высота дерева
        protected readonly int heightTree = 15;

        /// <summary>
        /// Блок бревна пальмы
        /// </summary>
        public BlockLogPalm() : base(158, 157)
        {
            leaves = EnumBlock.LeavesPalm;
            idLeaves = (int)leaves;
        }

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        public override void OnBreakBlock(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            int met = state.met;
            if (met == 0 || met == 6)
            {
                EnumBlock enumBlock;
                BlockPos bPos = new BlockPos();
                BlockState blockState;
                BlockBase block;
                int x, y, z, x1, z1;
                int size = crownWidth;
                int checkSize = size * size + 1;
                int y0, i;
                vec2i vec;

                for (y0 = 0; y0 <= heightTree; ++y0)
                {
                    y = y0 + blockPos.Y;
                    bPos.X = blockPos.X;
                    bPos.Y = y;
                    bPos.Z = blockPos.Z;
                    blockState = worldIn.GetBlockState(bPos);
                    enumBlock = blockState.GetEBlock();
                    if ((enumBlock == EBlock && blockState.met == 0))
                    {
                        block = blockState.GetBlock();
                        block.DropBlockAsItem(worldIn, bPos, blockState, 0);
                        worldIn.SetBlockToAir(bPos);
                    }
                    else
                    {
                        // проверяем смещение
                        for (i = 0; i < 4; i++)
                        {
                            vec = MvkStatic.AreaOne4[i];
                            bPos = blockPos.Offset(vec.x, 0, vec.y);
                            bPos.Y = y;
                            blockState = worldIn.GetBlockState(bPos);
                            enumBlock = blockState.GetEBlock();
                            if ((enumBlock == EBlock && blockState.met == 0))
                            {
                                blockPos.X = bPos.X;
                                blockPos.Z = bPos.Z;
                                break;
                            }
                        }
                        if ((enumBlock == EBlock && blockState.met == 0) || enumBlock == leaves)
                        {
                            block = blockState.GetBlock();
                            block.DropBlockAsItem(worldIn, bPos, blockState, 0);
                            worldIn.SetBlockToAir(bPos);
                        }
                    }
                    int bx = blockPos.X;
                    int bz = blockPos.Z;
                    for (x = bx - size; x <= bx + size; ++x)
                    {
                        x1 = x - bx;
                        for (z = bz - size; z <= bz + size; ++z)
                        {
                            z1 = z - bz;
                            if (x1 * x1 + z1 * z1 <= checkSize)
                            {
                                if (x != blockPos.X || z != blockPos.Z)
                                {
                                    bPos.X = x;
                                    bPos.Y = y;
                                    bPos.Z = z;
                                    blockState = worldIn.GetBlockState(bPos);
                                    enumBlock = blockState.GetEBlock();
                                    if ((enumBlock == EBlock && blockState.met == 0) || enumBlock == leaves)
                                    {
                                        block = blockState.GetBlock();
                                        block.DropBlockAsItem(worldIn, bPos, blockState, 0);
                                        worldIn.SetBlockToAir(bPos);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
