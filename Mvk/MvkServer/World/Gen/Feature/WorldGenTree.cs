using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация дерева
    /// </summary>
    public abstract class WorldGenTree : WorldGenerator
    {
        /// <summary>
        /// ID бревна
        /// </summary>
        protected ushort idLog;
        /// <summary>
        /// ID листвы
        /// </summary>
        protected ushort idLeaves;
        /// <summary>
        /// Тип бревна
        /// </summary>
        protected EnumBlock log;
        /// <summary>
        /// Тип листвы
        /// </summary>
        protected EnumBlock leaves;

        // высота ствола дерева до кроны
        protected int trunkHeight;
        // ширина кроны
        protected int crownWidth;
        // высота кроны
        protected int crownHeight;
        // от ствола до макушки
        protected int crownHeightUp = 3;
        /// <summary>
        /// Вверх макушка неприкасаемая для отсутствие листвы, примел ёлка
        /// </summary>
        protected int crownHeightUntouchable = 0;
        /// <summary>
        /// Смещение кроны
        /// </summary>
        protected vec2i offsetCrown = new vec2i(0);
        /// <summary>
        /// Проверка блока на какой ставим
        /// </summary>
        //protected EnumBlock blockCheck = EnumBlock.Turf;
        /// <summary>
        /// Замена блока на какой ставим
        /// </summary>
        protected EnumBlock blockPut = EnumBlock.Dirt;

        /// <summary>
        /// Ветки и отсутствие листвы случайныим числом в битных данных
        /// </summary>
        protected ushort branchesAndLeaves;
        /// <summary>
        /// Наличие веток
        /// </summary>
        protected bool isBranches = false;

        protected virtual bool IsPut(EnumBlock enumBlock) 
            => enumBlock == EnumBlock.Air || enumBlock == EnumBlock.TallGrass
                || enumBlock == EnumBlock.FlowerClover || enumBlock == EnumBlock.FlowerDandelion;

        protected virtual int Radius(int y0)
        {
            if (y0 == 0) return crownWidth - 1;
            if (y0 == crownHeight - 1) return 0;
            if (y0 == crownHeight - 2) return 1;
            if (y0 == crownHeight - 3) return 2;
            return crownWidth;
        }

        protected virtual void RandSize(Rand rand)
        {
            // высота ствола дерева до кроны
            trunkHeight = rand.Next(3) + 2;
            // ширина кроны
            crownWidth = rand.Next(10) == 0 ? 3 : 2;
            // высота кроны
            crownHeight = rand.Next(3) + 9;
            crownHeightUp = 3;
        }

        public override bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos)
        {
            branchesAndLeaves = (ushort)(rand.Next(65536) & rand.Next(65536));
            RandSize(rand);

            EnumBlock enumBlock = world.GetBlockState(blockPos).GetEBlock();
            EnumBlock enumBlockDown = world.GetBlockState(blockPos.OffsetDown()).GetEBlock();

            if ((IsPut(enumBlock) && (enumBlockDown == EnumBlock.Dirt || enumBlockDown == EnumBlock.Turf
                || enumBlockDown == EnumBlock.Sand || enumBlockDown == EnumBlock.Clay || enumBlockDown == EnumBlock.Gravel)) 
                || (enumBlock == log && enumBlockDown != log))
            {
                BlockPos bPos = new BlockPos();
                vec2i posCh = chunk.Position;
                
                int index, x, y, z, x1, z1, bx2, bz2;
                int bx = blockPos.X + offsetCrown.x;
                int by = blockPos.Y + trunkHeight;
                int bz = blockPos.Z + offsetCrown.y;
                int size = crownWidth;
                int checkSize = size * size + 1;
                int y0;
                bool check = true;
                ChunkStorage chunkStorage;

                for (y0 = 0; y0 < crownHeight; ++y0)
                {
                    y = y0 + by;
                    size = Radius(y0);
                    checkSize = size * size + 1;
                    for (x = bx - size; x <= bx + size; ++x)
                    {
                        x1 = x - bx;
                        for (z = bz - size; z <= bz + size; ++z)
                        {
                            z1 = z - bz;
                            if (x1 * x1 + z1 * z1 <= checkSize)
                            {
                                bPos.X = x;
                                bPos.Y = y;
                                bPos.Z = z;
                                enumBlock = world.GetBlockState(bPos).GetEBlock();
                                if (!IsPut(enumBlock) 
                                    && enumBlock != EnumBlock.LeavesOak && enumBlock != EnumBlock.LogOak
                                    && enumBlock != EnumBlock.LeavesBirch && enumBlock != EnumBlock.LogBirch
                                    && enumBlock != EnumBlock.LeavesSpruce && enumBlock != EnumBlock.LogSpruce
                                    && enumBlock != EnumBlock.LeavesFruit && enumBlock != EnumBlock.LogFruit
                                    && enumBlock != EnumBlock.LeavesPalm && enumBlock != EnumBlock.LogPalm)
                                {
                                    check = false;
                                    break;
                                }
                            }
                        }
                        if (!check) break;
                    }
                    if (!check) break;
                }

                if (check)
                {
                    SetBlock(world, blockPos.OffsetDown(), posCh, (ushort)blockPut);
                    bool notLeaves = false;
                    int indexBranches = 0;
                    int ychu = crownHeight - crownHeightUntouchable;
                    for (y0 = 0; y0 < crownHeight; ++y0)
                    {
                        y = y0 + by;
                        size = Radius(y0);
                        checkSize = size * size + 1;
                        for (x = bx - size; x <= bx + size; ++x)
                        {
                            x1 = x - bx;
                            for (z = bz - size; z <= bz + size; ++z)
                            {
                                z1 = z - bz;
                                if (x1 * x1 + z1 * z1 <= checkSize)
                                {
                                    if (posCh.x == x >> 4 && posCh.y == z >> 4)
                                    {
                                        index = y >> 4;
                                        bx2 = x & 15;
                                        bz2 = z & 15;
                                        if (bx2 >> 4 == 0 && bz2 >> 4 == 0 && index >= 0 && index < ChunkBase.COUNT_HEIGHT)
                                        {
                                            chunkStorage = chunk.StorageArrays[index];
                                            index = (y & 15) << 8 | bz2 << 4 | bx2;
                                            // Генератор отсутствия листвы
                                            if (y0 < ychu && x1 * x1 + z1 * z1 >= checkSize - 1)
                                            {
                                                notLeaves = (branchesAndLeaves & (1 << indexBranches)) != 0;
                                                indexBranches++;
                                                if (indexBranches > 16) indexBranches = 0;
                                            }
                                            else
                                            {
                                                notLeaves = false;
                                            }

                                            if (!notLeaves)
                                            {
                                                if (chunkStorage.countBlock > 0)
                                                {
                                                    if (chunkStorage.data[index] == 0)
                                                    {
                                                        chunkStorage.SetData(index, idLeaves);
                                                    }
                                                }
                                                else
                                                {
                                                    chunkStorage.SetData(index, idLeaves);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (isBranches)
                    {
                        TrunkBranches(posCh, blockPos, chunk, trunkHeight + crownHeight - crownHeightUp);
                    }
                    else
                    {
                        Trunk(posCh, blockPos, chunk, trunkHeight + crownHeight - crownHeightUp);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ствол
        /// </summary>
        protected virtual void Trunk(vec2i posCh, BlockPos blockPos, ChunkBase chunk, int count)
        {
            int bx = blockPos.X;
            int by = blockPos.Y;
            int bz = blockPos.Z;
            int index, y, bx2, bz2;
            ChunkStorage chunkStorage;

            if (posCh.x == bx >> 4 && posCh.y == bz >> 4)
            {
                bx2 = bx & 15;
                bz2 = bz & 15;
                if (bx2 >> 4 == 0 && bz2 >> 4 == 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        y = by + i;
                        index = y >> 4;
                        if (index >= 0 && index < ChunkBase.COUNT_HEIGHT)
                        {
                            chunkStorage = chunk.StorageArrays[index];
                            index = (y & 15) << 8 | bz2 << 4 | bx2;
                            chunkStorage.SetData(index, idLog);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ствол с ветками
        /// </summary>
        protected virtual void TrunkBranches(vec2i posCh, BlockPos blockPos, ChunkBase chunk, int count)
        {
            int bx = blockPos.X;
            int by;
            int bz = blockPos.Z;
            int index, y, bx2, bz2, bx3, bz3, chy, bxv, bzv, i, j;
            vec2i vec;
            ChunkStorage chunkStorage;
            count--;
            int indexBranches = 0;
            bool check = posCh.x == bx >> 4 && posCh.y == bz >> 4;
            bool bit;
            by = blockPos.Y;
            bx2 = bx & 15;
            bz2 = bz & 15;
            int k = 0;
            for (i = count; i >= 0; i--)
            {
                y = by + i;
                chy = y >> 4;
                if (chy >= 0 && chy < ChunkBase.COUNT_HEIGHT)
                {
                    // Ствол
                    if (check && bx2 >> 4 == 0 && bz2 >> 4 == 0)
                    {
                        chunkStorage = chunk.StorageArrays[chy];
                        index = (y & 15) << 8 | bz2 << 4 | bx2;
                        chunkStorage.SetData(index, idLog);
                    }
                    // Ветки
                    if (i >= trunkHeight)
                    {
                        for (j = 0; j < 2; j++)
                        {
                            bit = (branchesAndLeaves & (1 << indexBranches)) != 0;
                            indexBranches++;
                            if (indexBranches > 16) indexBranches = 0;
                            vec = MvkStatic.AreaOne4[j + k];
                            if (bit)
                            {
                                bxv = bx + vec.x;
                                bzv = bz + vec.y;
                                if (posCh.x == bxv >> 4 && posCh.y == bzv >> 4)
                                {
                                    bx3 = bxv & 15;
                                    bz3 = bzv & 15;
                                    if (bx3 >> 4 == 0 && bz3 >> 4 == 0)
                                    {
                                        chunkStorage = chunk.StorageArrays[chy];
                                        index = (y & 15) << 8 | bz3 << 4 | bx3;
                                        chunkStorage.SetData(index, (ushort)(idLog & 0xFFF | (2 - j) << 12));
                                    }
                                }
                            }
                        }
                        k = (k == 0 ? 2 : 0);
                    }
                }
            }
        }

    }
}
