using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация ели или сосны дерева
    /// </summary>
    public class WorldGenTreeSpruce2 : WorldGenTree2
    {
        /// <summary>
        /// сосна
        /// </summary>
        private bool pine = true;

        public WorldGenTreeSpruce2()
        {
            log = EnumBlock.LogSpruce;
            leaves = EnumBlock.LeavesSpruce;
            sapling = EnumBlock.SaplingSpruce;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            idSapling = (ushort)sapling;
        }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void RandSize()
        {
            // определение сосны
            pine = NextInt(4) != 0;
            if (pine)
            {
                // Высота ствола дерева до кроны
                trunkHeight = NextInt(9) + 20;
                // Ствол снизу без веток 
                trunkWithoutBranches = NextInt(3) + 4;
                // Количество секций веток для сужения
                //                _    / \
                //          _    / \  |   |
                //    _    / \  |   | |   |
                //   / \  |   | |   | |   |
                //   \ /   \ /   \ /   \ /
                //  2 |   3 |   4 |   5 |
                sectionCountBranches = 3;
                // Минимальная обязательная длинна ветки
                branchLengthMin = 5;
                // Случайная дополнительная длинна ветки к обязательной
                branchLengthRand = 3;
            }
            else
            {
                // Высота ствола дерева до кроны
                trunkHeight = NextInt(10) + 10;
                // Ствол снизу без веток 
                trunkWithoutBranches = NextInt(3) + 1;
                // Минимальная обязательная длинна ветки
                branchLengthMin = trunkHeight / 4;
                // Случайная дополнительная длинна ветки к обязательной
                branchLengthRand = trunkHeight / 6;
            }
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
                vec2i vec;
                int bx = blockPos.X;
                int by = blockPos.Y;
                int bz = blockPos.Z;
                // Значения где формируется ствол, чтоб ствол не уходил далеко
                int bx0 = bx;
                int bz0 = bz;
                int y, iUp, iSide, iBranche;
                bool pineDown = false;
                // Длина ветки
                int lightBranche;
                // высота ствола
                int count = trunkHeight;
                // ствол с ветками
                int trunkWithBranches = trunkHeight - trunkWithoutBranches;
                // i для счётчика сужения веток
                int itwb;
                // stick - палка
                int sx, sy, sz;
                // через сколько может быть смещение ветки по y, один раз
                int stickBiasY;
                // через сколько может быть смещение ветки по x или z, много раз
                int stickBiasXZ;

                // Cмещения веток по всем сторонам, чтоб не слипались друг с другом
                int row = 0;
                // Коэффициент длинны веток 0 - 1
                float factorLightBranches;
                // Количество блоков в секции
                int countBlockSection;
                // снизу вверх
                for (iUp = 0; iUp < count; iUp++)
                {
                    y = by + iUp;

                    // Ствол
                    if (CheckBlock(world, bx, y, bz)) return false;
                    // Ствол, если нижний то параметр для пенька
                    blockCaches.Add(new BlockCache(bx, y, bz, idLog, (ushort)(iUp == 0 ? 6 : 0), true));

                    // Ветки
                    if (iUp >= trunkWithoutBranches)
                    {
                        if (row == 0)
                        {
                            // Есть ветка

                            // параметр смещения ветки в 2 блока между ветками
                            row = 2;

                            // Находи коэффициент длинны веток
                            itwb = iUp - trunkWithoutBranches;
                            if (pine)
                            {
                                countBlockSection = trunkWithBranches / sectionCountBranches;
                                factorLightBranches = itwb < countBlockSection ? itwb / (float)countBlockSection : itwb > trunkWithBranches - countBlockSection
                                    ? (countBlockSection - (itwb - (trunkWithBranches - countBlockSection))) / (float)countBlockSection : 1;

                                // Больше сухих веток или меньше снизу
                                pineDown = itwb < trunkWithBranches / (NextInt(2) + 2);

                                if (pineDown)
                                {
                                    // Сокрощяем длинные сухие ветки
                                    if (factorLightBranches > .25f) factorLightBranches /= 2f;
                                    row = 1;
                                }
                            }
                            else
                            {
                                factorLightBranches = 1f - itwb / (float)trunkWithBranches;

                                if (factorLightBranches < .55f) row = 1;

                                // Листва вокруг ствола
                                FoliageAroundTrunk(bx, y, bz);
                            }

                            // цикл направлении веток
                            for (iSide = 0; iSide < 4; iSide++)
                            {
                                // определяем длинну ветки
                                lightBranche = (int)((NextInt(branchLengthRand) + branchLengthMin) * factorLightBranches);

                                if (pine && pineDown && NextInt(3) == 0)
                                {
                                    lightBranche = 0;
                                }

                                if (lightBranche > 0)
                                {
                                    
                                    // Точно имеется ветка работаем с ней
                                    vec = MvkStatic.AreaOne4[iSide];
                                    sx = bx;
                                    sy = y;
                                    sz = bz;

                                    stickBiasY = 3;
                                    stickBiasXZ = 2;

                                    for (iBranche = 1; iBranche <= lightBranche; iBranche++)
                                    {
                                        // цикл длинны ветки
                                        if (vec.x != 0) sx += vec.x;
                                        if (vec.y != 0) sz += vec.y;

                                        // Проверка смещение ветки по вертикале
                                        if (stickBiasY >= 0)
                                        {
                                            if (stickBiasY == 0) sy++;
                                            stickBiasY--;
                                        }

                                        // Проверка смещение ветки по горизонтали
                                        if (stickBiasXZ == 0)
                                        {
                                            stickBiasXZ = 2;
                                            if (vec.y == 0) sz += NextInt(3) - 1;
                                            if (vec.x == 0) sx += NextInt(3) - 1;
                                        }
                                        else
                                        {
                                            stickBiasXZ--;
                                        }

                                        if (CheckBlock(world, sx, sy, sz)) return false;

                                        // фиксируем ветку
                                        blockCaches.Add(new BlockCache(sx, sy, sz, idLog,
                                            (ushort)((iSide == 0 || iSide == 2) ? 2 : 1), true));

                                        if ((!pine || (pine && !pineDown)))
                                        {
                                            // Листва на ветке
                                            FoliageBranch(sx, sy, sz, iSide);
                                        }
                                    }
                                }
                                else if (!pine)
                                {
                                    // Ветка
                                    vec = MvkStatic.AreaOne4[iSide];
                                    sx = bx + vec.x;
                                    sy = y;
                                    sz = bz + vec.y;
                                    blockCaches.Add(new BlockCache(sx, sy, sz, idLeaves, (byte)(iSide + NextInt(2) * 6 + 2)));
                                }
                            }
                        }
                        else
                        {
                            row--;
                        }
                    }
                }
                y = by + count;
                // Ствол
                blockCaches.Add(new BlockCache(bx, y, bz, idLog, 0, true));
                // Листва на мокушке
                FoliageTop(bx, y, bz);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Листва на ветке, side = 0-3 горизонтальный вектор направление вектора MvkStatic.AreaOne4
        /// 0 - South, 1 - East, 2 - North, 3 - West
        /// </summary>
        protected override void FoliageBranch(int x, int y, int z, int side)
        {
            if (side == 0) // South
            {
                // Все кроме North
                // East
                blockCaches.Add(new BlockCache(x + 1, y, z, idLeaves, (byte)(2 + NextInt(2) * 6)));
                // West
                blockCaches.Add(new BlockCache(x - 1, y, z, idLeaves, (byte)(3 + NextInt(2) * 6)));
                // South
                blockCaches.Add(new BlockCache(x, y, z + 1, idLeaves, (byte)(5 + NextInt(2) * 6)));
            }
            else if (side == 1) // East
            {
                // Все кроме West
                // East
                blockCaches.Add(new BlockCache(x + 1, y, z, idLeaves, (byte)(2 + NextInt(2) * 6)));
                // North
                blockCaches.Add(new BlockCache(x, y, z - 1, idLeaves, (byte)(4 + NextInt(2) * 6)));
                // South
                blockCaches.Add(new BlockCache(x, y, z + 1, idLeaves, (byte)(5 + NextInt(2) * 6)));
            }
            else if (side == 2) // North
            {
                // Все кроме South
                // East
                blockCaches.Add(new BlockCache(x + 1, y, z, idLeaves, (byte)(2 + NextInt(2) * 6)));
                // West
                blockCaches.Add(new BlockCache(x - 1, y, z, idLeaves, (byte)(3 + NextInt(2) * 6)));
                // North
                blockCaches.Add(new BlockCache(x, y, z - 1, idLeaves, (byte)(4 + NextInt(2) * 6)));
            }
            else // West
            {
                // Все кроме East
                // West
                blockCaches.Add(new BlockCache(x - 1, y, z, idLeaves, (byte)(3 + NextInt(2) * 6)));
                // North
                blockCaches.Add(new BlockCache(x, y, z - 1, idLeaves, (byte)(4 + NextInt(2) * 6)));
                // South
                blockCaches.Add(new BlockCache(x, y, z + 1, idLeaves, (byte)(5 + NextInt(2) * 6)));
            }
        }
    }
}
