using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация пальмы
    /// </summary>
    public class WorldGenTreePalm2 : WorldGenTree2
    {
        /// <summary>
        /// Вектор наклона
        /// </summary>
        private vec2i vecIncline;

        public WorldGenTreePalm2()
        {
            log = EnumBlock.LogPalm;
            leaves = EnumBlock.LeavesPalm;
            sapling = EnumBlock.SaplingPalm;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            idSapling = (ushort)sapling;
            blockPut = EnumBlock.Sand;
        }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void RandSize()
        {
            // Высота ствола дерева до кроны
            trunkHeight = NextInt(7) + 13;
            // Максимальное смещение ствола от пенька
            maxTrunkBias = NextInt(7);
            // смещение, через какое ствол может смещаться
            trunkBias = NextInt(5) + 1;
            // Вектор наклона
            vecIncline = MvkStatic.AreaOne8[NextInt(8)];
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
            SetRand(rand);
            EnumBlock enumBlock = world.GetBlockState(blockPos).GetEBlock();
            // Проверка, может ли рости дереве тут
            if (CheckBlockDown(enumBlock, world.GetBlockState(blockPos.OffsetDown()).GetEBlock()))
            {
                blockCaches.Add(new BlockCache(blockPos.OffsetDown(), blockPut, 0, true));
                int bx = blockPos.X;
                int by = blockPos.Y;
                int bz = blockPos.Z;
                // Значения где формируется ствол, чтоб ствол не уходил далеко
                int bx0 = bx;
                int bz0 = bz;
                int y, iUp;
                // высота ствола
                int count = trunkHeight;

                // снизу вверх
                for (iUp = 0; iUp < count; iUp++)
                {
                    y = by + iUp;
                    // готовим смещение ствола
                    if (trunkBias <= 0)
                    {
                        if (NextInt(2) == 0)
                        {
                            if (vecIncline.x != 0 && bx0 >= bx + vecIncline.x - maxTrunkBias && bx0 <= bx + vecIncline.x + maxTrunkBias)
                            {
                                bx += vecIncline.x;
                                trunkBias = NextInt(6) + 1;
                            }
                        }
                        else
                        {
                            if (vecIncline.y != 0 && bz0 >= bz + vecIncline.y - maxTrunkBias && bz0 <= bz + vecIncline.y + maxTrunkBias)
                            {
                                bz += vecIncline.y;
                                trunkBias = NextInt(6) + 1;
                            }
                        }
                    }
                    else
                    {
                        trunkBias--;
                    }

                    // Ствол
                    if (CheckBlock(world, bx, y, bz)) return false;
                    // Ствол, если нижний то параметр для пенька
                    blockCaches.Add(new BlockCache(bx, y, bz, idLog, (ushort)(iUp == 0 ? 6 : 0), true));
                }
                y = by + count;
                // Ствол
                blockCaches.Add(new BlockCache(bx, y, bz, idLog, 0, true));

                // Листва по AreaOne8
                blockCaches.Add(new BlockCache(bx, y, bz + 1, idLeaves, 0)); // Pole=5
                blockCaches.Add(new BlockCache(bx + 1, y, bz + 1, idLeaves, 1));
                blockCaches.Add(new BlockCache(bx + 1, y, bz, idLeaves, 2)); // Pole=2
                blockCaches.Add(new BlockCache(bx + 1, y, bz - 1, idLeaves, 3));
                blockCaches.Add(new BlockCache(bx, y, bz - 1, idLeaves, 4)); // Pole=4
                blockCaches.Add(new BlockCache(bx - 1, y, bz - 1, idLeaves, 5));
                blockCaches.Add(new BlockCache(bx - 1, y, bz, idLeaves, 6)); // Pole=3
                blockCaches.Add(new BlockCache(bx - 1, y, bz + 1, idLeaves, 7));

                if (NextBool(8)) blockCaches.Add(new BlockCache(bx, y, bz + 2, idLeaves, 8));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx + 2, y, bz + 2, idLeaves, 9));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx + 2, y, bz, idLeaves, 10));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx + 2, y, bz - 2, idLeaves, 11));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx, y, bz - 2, idLeaves, 12));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx - 2, y, bz - 2, idLeaves, 13));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx - 2, y, bz, idLeaves, 14));
                if (NextBool(8)) blockCaches.Add(new BlockCache(bx - 2, y, bz + 2, idLeaves, 15));
                return true;
            }
            return false;
        }
    }
}
