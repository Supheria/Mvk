using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections.Generic;
using System.Diagnostics;

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
        /// ID саженца
        /// </summary>
        protected ushort idSapling;
        /// <summary>
        /// Тип бревна
        /// </summary>
        protected EnumBlock log;
        /// <summary>
        /// Тип листвы
        /// </summary>
        protected EnumBlock leaves;
        /// <summary>
        /// Тип саженца
        /// </summary>
        protected EnumBlock sapling;

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
        /// <summary>
        /// Список блоков которые будет построено дерево
        /// </summary>
        protected ListMvk<BlockCache> blockCaches = new ListMvk<BlockCache>(16384);

        protected virtual bool IsPut(EnumBlock enumBlock)
            => enumBlock == EnumBlock.Air || enumBlock == sapling || enumBlock == EnumBlock.TallGrass
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
            if (GenerateCache(world, rand, blockPos))
            {
                ChunkStorage chunkStorage;
                vec2i posCh = chunk.Position;
                int index, x, y, z, chy, bx, bz, i;
                int count = blockCaches.Count;
                BlockCache blockCache;
                for (i = 0; i < count; i++)
                {
                    blockCache = blockCaches[i];
                    x = blockCache.x;
                    y = blockCache.y;
                    z = blockCache.z;
                    if (y >= 0 && y <= ChunkBase.COUNT_HEIGHT_BLOCK && posCh.x == x >> 4 && posCh.y == z >> 4)
                    {
                        bx = x & 15;
                        bz = z & 15;
                        if (bx >> 4 == 0 && bz >> 4 == 0)
                        {
                            chy = y >> 4;
                            chunkStorage = chunk.StorageArrays[chy];
                            index = (y & 15) << 8 | bz << 4 | bx;
                            chunkStorage.SetData(index, blockCache.id, blockCache.met);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Генерация дерева, от саженца
        /// </summary>
        public bool GenefateTree(WorldBase world, Rand rand, BlockPos blockPos)
        {
            if (GenerateCache(world, rand, blockPos))
            {
                BlockState blockState = world.GetBlockState(blockPos);
                world.SetBlockToAir(blockPos);
                int i;
                int count = blockCaches.Count;
                BlockPos pos;
                BlockCache blockCache;
                for (i = 0; i < count; i++)
                {
                    blockCache = blockCaches[i];
                    pos.X = blockCache.x;
                    pos.Y = blockCache.y;
                    pos.Z = blockCache.z;
                    if (pos.IsValid())
                    {
                        world.SetBlockState(pos, new BlockState(blockCache.id, blockCache.met), 14);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Сгенерировать дерево в кеш
        /// </summary>
        /// <param name="world"></param>
        /// <param name="rand"></param>
        /// <param name="blockPos"></param>
        protected override bool GenerateCache(WorldBase world, Rand rand, BlockPos blockPos)
        {
            blockCaches.Clear();
            branchesAndLeaves = (ushort)(rand.Next(65536) & rand.Next(65536));
            RandSize(rand);

            EnumBlock enumBlock = world.GetBlockState(blockPos).GetEBlock();
            EnumBlock enumBlockDown = world.GetBlockState(blockPos.OffsetDown()).GetEBlock();

            if ((IsPut(enumBlock) && (enumBlockDown == EnumBlock.Dirt || enumBlockDown == EnumBlock.Turf
                || enumBlockDown == EnumBlock.Sand || enumBlockDown == EnumBlock.Clay || enumBlockDown == EnumBlock.Gravel))
                || (enumBlock == log && enumBlockDown != log))
            {
                BlockPos bPos = new BlockPos();

                int x, y, z, x1, z1;
                int bx = blockPos.X + offsetCrown.x;
                int by = blockPos.Y + trunkHeight;
                int bz = blockPos.Z + offsetCrown.y;
                int size = crownWidth;
                int checkSize = size * size + 1;
                int y0;
                bool check = true;

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
                    blockCaches.Add(new BlockCache(blockPos.OffsetDown(), blockPut));
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
                                        blockCaches.Add(new BlockCache(x, y, z, idLeaves));
                                    }
                                }
                            }
                        }
                    }

                    if (isBranches)
                    {
                        TrunkBranches(blockPos, trunkHeight + crownHeight - crownHeightUp);
                    }
                    else
                    {
                        Trunk(blockPos, trunkHeight + crownHeight - crownHeightUp);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ствол
        /// </summary>
        protected virtual void Trunk(BlockPos blockPos, int count)
        {
            int bx = blockPos.X;
            int by = blockPos.Y;
            int bz = blockPos.Z;
            int i, y;

            for (i = 0; i < count; i++)
            {
                y = by + i;
                blockCaches.Add(new BlockCache(bx, y, bz, idLog, (ushort)(i == 0 ? 6 : 0)));
            }
        }

        /// <summary>
        /// Ствол с ветками
        /// </summary>
        protected virtual void TrunkBranches(BlockPos blockPos, int count)
        {
            int bx = blockPos.X;
            int by = blockPos.Y;
            int bz = blockPos.Z;
            int y, i, j;
            vec2i vec;
            count--;
            int indexBranches = 0;
            bool bit;
            int k = 0;
            for (i = count; i >= 0; i--)
            {
                y = by + i;
                // Ствол
                blockCaches.Add(new BlockCache(bx, y, bz, idLog, (ushort)(i == 0 ? 6 : 0)));

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
                            blockCaches.Add(new BlockCache(bx + vec.x, y, bz + vec.y, idLog, (ushort)(2 - j)));
                        }
                    }
                    k = (k == 0 ? 2 : 0);
                }
            }
        }
    }
}
