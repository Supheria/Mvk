using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный класс бревна
    /// </summary>
    public abstract class BlockAbLog : BlockAbWood
    {
        /***
         * Met
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3 - вверх, игрок
         * 4/5 - бок, игрок
         * 6 - вверх, генерация, нижний блок для пня
         */

        protected EnumBlock leaves;
        protected int idLeaves;
        // ширина кроны
        protected int crownWidth = 3;
        // высота дерева
        protected int heightTree = 15;

        public BlockAbLog(int numberTextureButt, int numberTextureSide, vec3 colorButt, vec3 colorSide, EnumBlock leaves) 
            : base(numberTextureButt, numberTextureSide, colorButt, colorSide)
        {
            Combustibility = true;
            IgniteOddsSunbathing = 5;
            BurnOdds = 10;
            this.leaves = leaves;
            idLeaves = (int)leaves;
            metUp = 3;
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => state.met == 6 ? 100 : 30;

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb)
        {
            // gen 0 up; 1 e||w; 2 s||n; 
            // play 3 up; 4 e||w; 5 s||n;
            // gen down 6 up;
            if (met > 2) met -= 3;
            if (met > 2) met -= 3;
            return boxes[met];
        }

        /// <summary>
        /// Разрушить блок
        /// </summary>
        public override void Destroy(WorldBase worldIn, BlockPos blockPos, BlockState state, bool sound, bool particles)
        {
            base.Destroy(worldIn, blockPos, state, sound, particles);
            int met = state.met;
            if (met == 0 || met == 6)
            {
                EnumBlock enumBlock;
                BlockPos bPos = new BlockPos();
                BlockState blockState;
                BlockBase block;
                int x, y, z, x1, z1;
                int bx = blockPos.X;
                int by = blockPos.Y;
                int bz = blockPos.Z;
                int size = crownWidth;
                int checkSize = size * size + 1;
                int y0;

                for (y0 = 0; y0 <= heightTree; ++y0)
                {
                    y = y0 + blockPos.Y;
                    bPos.X = blockPos.X;
                    bPos.Y = y;
                    bPos.Z = blockPos.Z;
                    blockState = worldIn.GetBlockState(bPos);
                    met = blockState.met;
                    enumBlock = blockState.GetEBlock();
                    if ((enumBlock == EBlock && met == 0) || enumBlock == leaves)
                    {
                        block = blockState.GetBlock();
                        block.DropBlockAsItem(worldIn, bPos, blockState, 0);
                        block.Destroy(worldIn, bPos, blockState);
                    }
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
                                    met = blockState.met;
                                    if ((enumBlock == EBlock && (met == 1 || met == 2)) || enumBlock == leaves)
                                    {
                                        block = blockState.GetBlock();
                                        block.DropBlockAsItem(worldIn, bPos, blockState, 0);
                                        block.Destroy(worldIn, bPos, blockState);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        //public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        //{
        //    EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
        //    if (enumBlock == EnumBlock.Air)
        //    {
        //        BlockPos bPos = new BlockPos();
        //        BlockState blockState;
        //        int x, y, z, x1, z1;
        //        int bx = blockPos.X;
        //        int by = blockPos.Y;
        //        int bz = blockPos.Z;
        //        int size = 5;
        //        int checkSize = size * size + 1;
        //        int y0;


        //        for (y0 = 0; y0 < 16; ++y0)
        //        {
        //            y = y0 + blockPos.Y;
        //            bPos.X = blockPos.X;
        //            bPos.Y = y;
        //            bPos.Z = blockPos.Z;
        //            blockState = worldIn.GetBlockState(bPos);
        //            enumBlock = blockState.GetEBlock();
        //            if (enumBlock != EBlock && enumBlock != leaves)
        //            {
        //                //break;
        //            }
        //            else
        //            {
        //                blockState.GetBlock().DropBlockAsItem(worldIn, bPos, blockState, 0);
        //                worldIn.SetBlockState(bPos, new BlockState(EnumBlock.Air), 12);
        //            }
        //            for (x = bx - size; x <= bx + size; ++x)
        //            {
        //                x1 = x - bx;
        //                for (z = bz - size; z <= bz + size; ++z)
        //                {
        //                    z1 = z - bz;
        //                    if (x1 * x1 + z1 * z1 <= checkSize)
        //                    {
        //                        if (x != blockPos.X || z != blockPos.Z)
        //                        {
        //                            bPos.X = x;
        //                            bPos.Y = y;
        //                            bPos.Z = z;
        //                            blockState = worldIn.GetBlockState(bPos);
        //                            enumBlock = blockState.GetEBlock();
        //                            if ((enumBlock == EBlock && blockState.Met() != 0) || enumBlock == leaves)
        //                            {
        //                                blockState.GetBlock().DropBlockAsItem(worldIn, bPos, blockState, 0);
        //                                worldIn.SetBlockState(bPos, new BlockState(EnumBlock.Air), 12);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //if (!CanBlockStay(worldIn, blockPos))
        //    //{
        //    //    DropBlockAsItem(worldIn, blockPos, state, 0);
        //    //    worldIn.SetBlockState(blockPos, new BlockState(EnumBlock.Air), 14);
        //    //}
        //}
    }
}
