using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Diagnostics;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация дерева
    /// </summary>
    public abstract class WorldGenTree2 : WorldGenerator
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

        /// <summary>
        /// Высота ствола дерева
        /// </summary>
        protected int trunkHeight;
        /// <summary>
        /// Ствол снизу без веток 
        /// </summary>
        protected int trunkWithoutBranches;
        /// <summary>
        /// Cмещение ствола, через какое количество блоков может смещаться
        /// </summary>
        protected int trunkBias;
        /// <summary>
        /// Максимальное смещение ствола от пенька
        /// </summary>
        protected int maxTrunkBias;
        /// <summary>
        /// Количество секций веток для сужения
        ///                _    / \
        ///          _    / \  |   |
        ///    _    / \  |   | |   |
        ///   / \  |   | |   | |   |
        ///   \ /   \ /   \ /   \ /
        ///  2 |   3 |   4 |   5 |
        /// </summary>
        protected int sectionCountBranches;
        /// <summary>
        /// Минимальная обязательная длинна ветки
        /// </summary>
        protected int branchLengthMin;
        /// <summary>
        /// Случайная дополнительная длинна ветки к обязательной
        /// </summary>
        protected int branchLengthRand;
        /// <summary>
        /// Насыщенность листвы на ветке, значение в NextInt() меньше 1 не допустимо, чем больше тем веток меньше
        /// При значении 1 максимально
        /// </summary>
        protected int foliageBranch;

        /// <summary>
        /// Смещение кроны
        /// </summary>
        protected vec2i offsetCrown = new vec2i(0);
        /// <summary>
        /// Замена блока на какой ставим
        /// </summary>
        protected EnumBlock blockPut = EnumBlock.Dirt;

        /// <summary>
        /// Список блоков которые будет построено дерево
        /// </summary>
        protected ListMvk<BlockCache> blockCaches = new ListMvk<BlockCache>(16384);

        /// <summary>
        /// Заключительная часть LCG, которая использует координаты фрагмента X, Z
        /// вместе с двумя другими начальными значениями для генерации псевдослучайных чисел
        /// </summary>
        private long seed;
        /// <summary>
        /// Для проверок позиция блока
        /// </summary>
        private BlockPos checkPos = new BlockPos();
        /// <summary>
        /// Для проверки тип блока
        /// </summary>
        private EnumBlock checkEnumBlock;
        /// <summary>
        /// Для проверки материал блока
        /// </summary>
        private EnumMaterial checkEnumMaterial;
        /// <summary>
        /// Генерация с рельефа, без проверки соседних блоков
        /// </summary>
        protected bool isGen = true;

        /// <summary>
        /// Возвращает псевдослучайное число LCG из [0, x). Аргументы: целое х
        /// </summary>
        protected int NextInt(int x)
        {
            int random = (int)((seed >> 24) % x);
            if (random < 0) random += x;
            seed *= seed * 1284865837 + 4150755663;
            return random;
        }
        /// <summary>
        /// Вероятность, где 1 это 100%, 2 50/50. NextInt(x) != 0
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected bool NextBool(int x) => NextInt(x) != 0;

        protected virtual bool IsPut(EnumBlock enumBlock)
            => enumBlock == EnumBlock.Air || enumBlock == sapling || enumBlock == EnumBlock.Grass || enumBlock == EnumBlock.TallGrass
                || enumBlock == EnumBlock.FlowerClover || enumBlock == EnumBlock.FlowerDandelion;

        /// <summary>
        /// Задать случайное зерно
        /// </summary>
        protected void SetRand(Rand rand)
        {
            // сид для доп генерации дерева, не отвлекаясь от генерации мира
            seed = rand.Next();
            RandSize();
        }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected virtual void RandSize()
        {
            // Высота ствола дерева до кроны
            trunkHeight = NextInt(11) + 12;
            // Ствол снизу без веток 
            trunkWithoutBranches = NextInt(3) + 2;
            // смещение, через какое ствол может смещаться
            trunkBias = 100;// NextInt(3) + 2;
            // Максимальное смещение ствола от пенька
            maxTrunkBias = 6;
            // Количество секций веток для сужения
            //                _    / \
            //          _    / \  |   |
            //    _    / \  |   | |   |
            //   / \  |   | |   | |   |
            //   \ /   \ /   \ /   \ /
            //  2 |   3 |   4 |   5 |
            sectionCountBranches = 4;
            // Минимальная обязательная длинна ветки
            branchLengthMin = 4;
            // Случайная дополнительная длинна ветки к обязательной
            branchLengthRand = 4;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            foliageBranch = 32;
        }
        
        public override bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos)
        {
            isGen = true;
            GenerateCache(world, rand, blockPos);
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
                        if (blockCache.body || (!chunkStorage.IsEmptyData() && (chunkStorage.data[index] & 0xFFF) == 0)
                            || chunkStorage.IsEmptyData())
                        {
                            chunkStorage.SetData(index, blockCache.id, blockCache.met);
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Генерация дерева, от саженца
        /// </summary>
        public bool GenefateTree(WorldBase world, Rand rand, BlockPos blockPos)
        {
            isGen = false;
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (GenerateCache(world, rand, blockPos))
            {
                //world.Log.Log("GenTree2G[{1}]: {0:0.000} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, blockPos);
                //stopwatch.Restart();
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
                        if (blockCache.body || world.GetBlockState(pos).IsAir())
                        world.SetBlockState(pos, new BlockState(blockCache.id, blockCache.met), 14);
                    }
                }
                //world.Log.Log("GenTree2B[{1}]: {0:0.000} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, blockPos);
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
                // временное значение смещение ствола
                int txz;
                int y, iUp, iSide, iBranche;
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

                // Массив смещения веток по конкретной стороне, чтоб не слипались друг с другом
                int[] row = new int[] { NextInt(3), NextInt(3), NextInt(3), NextInt(3) };
                // Коэффициент длинны веток 0 - 1
                float factorLightBranches;
                // Количество блоков в секции
                int countBlockSection;
                // снизу вверх
                for (iUp = 0; iUp < count; iUp++)
                {
                    y = by + iUp;
                    // готовим смещение ствола
                    if (trunkBias <= 0)
                    {
                        trunkBias = NextInt(3) + 1;
                        txz = NextInt(3) - 1;

                        if (txz != 0 && bx0 >= bx + txz - maxTrunkBias && bx0 <= bx + txz + maxTrunkBias)
                        {
                            bx += txz;
                        }
                        else 
                        {
                            txz = NextInt(3) - 1;
                            if (txz != 0 && bz0 >= bz + txz - maxTrunkBias && bz0 <= bz + txz + maxTrunkBias)
                            {
                                bz += txz;
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

                    // Ветки
                    if (iUp >= trunkWithoutBranches)
                    {
                        // Находи коэффициент длинны веток
                        itwb = iUp - trunkWithoutBranches;
                        countBlockSection = trunkWithBranches / sectionCountBranches;
                        factorLightBranches = itwb < countBlockSection ? itwb / (float)countBlockSection : itwb > trunkWithBranches - countBlockSection
                            ? (countBlockSection - (itwb - (trunkWithBranches - countBlockSection))) / (float)countBlockSection : 1;

                        // цикл направлении веток
                        for (iSide = 0; iSide < 4; iSide++)
                        {
                            if (row[iSide] == 0)
                            {
                                // Есть ветка

                                if (NextInt(16) == 0)
                                {
                                    // Разреживание веток, отрицаем ветку и добавляем смещение к ветке
                                    row[iSide] = NextInt(3);
                                }
                                else
                                {
                                    // определяем длинну ветки
                                    lightBranche = (int)((NextInt(branchLengthRand) + branchLengthMin) * factorLightBranches);
                                    if (lightBranche > 0)
                                    {
                                        // Точно имеется ветка работаем с ней

                                        // параметр смещения ветки, 2 - 3 можно откорректировать для разных деревьев
                                        row[iSide] = NextInt(2) + 2;

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

                                            if (iBranche == lightBranche || NextInt(foliageBranch) == 0)
                                            {
                                                // Листва на ветке
                                                FoliageBranch(sx, sy, sz, iSide);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                row[iSide]--;
                            }
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
        /// Проверить блок если вернёт true останавливаем генерацию
        /// </summary>
        protected bool CheckBlock(WorldBase world, int x, int y, int z)
        {
            if (!isGen)
            {
                checkPos.X = x;
                checkPos.Y = y;
                checkPos.Z = z;
                checkEnumBlock = world.GetBlockState(checkPos).GetEBlock();
                checkEnumMaterial = Blocks.GetBlockCache(checkEnumBlock).Material;
                if (!(checkEnumBlock == EnumBlock.Air || checkEnumMaterial == EnumMaterial.Leaves || checkEnumMaterial == EnumMaterial.Sapling)
                    && checkEnumBlock != log)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверка блока на котором стоит дерево
        /// </summary>
        protected bool CheckBlockDown(EnumBlock enumBlock, EnumBlock enumBlockDown)
        {
            return (IsPut(enumBlock) && (enumBlockDown == EnumBlock.Dirt || enumBlockDown == EnumBlock.Turf
                || enumBlockDown == EnumBlock.Sand || enumBlockDown == EnumBlock.Clay || enumBlockDown == EnumBlock.Gravel))
                || (enumBlock == log && enumBlockDown != log);
        }

        /// <summary>
        ///Листва на мокушке
        /// </summary>
        protected void FoliageTop(int x, int y, int z)
        {
            // Up
            blockCaches.Add(new BlockCache(x, y + 1, z, idLeaves, (byte)(NextInt(2) * 6)));
            // East
            blockCaches.Add(new BlockCache(x + 1, y, z, idLeaves, (byte)(2 + NextInt(2) * 6)));
            // West
            blockCaches.Add(new BlockCache(x - 1, y, z, idLeaves, (byte)(3 + NextInt(2) * 6)));
            // North
            blockCaches.Add(new BlockCache(x, y, z - 1, idLeaves, (byte)(4 + NextInt(2) * 6)));
            // South
            blockCaches.Add(new BlockCache(x, y, z + 1, idLeaves, (byte)(5 + NextInt(2) * 6)));
        }

        /// <summary>
        /// Листва вокруг ствола
        /// </summary>
        protected void FoliageAroundTrunk(int x, int y, int z)
        {
            // East
            blockCaches.Add(new BlockCache(x + 1, y, z, idLeaves, (byte)(2 + NextInt(2) * 6)));
            // West
            blockCaches.Add(new BlockCache(x - 1, y, z, idLeaves, (byte)(3 + NextInt(2) * 6)));
            // North
            blockCaches.Add(new BlockCache(x, y, z - 1, idLeaves, (byte)(4 + NextInt(2) * 6)));
            // South
            blockCaches.Add(new BlockCache(x, y, z + 1, idLeaves, (byte)(5 + NextInt(2) * 6)));
        }

        /// <summary>
        /// Листва на ветке, side = 0-3 горизонтальный вектор направление вектора MvkStatic.AreaOne4
        /// 0 - South, 1 - East, 2 - North, 3 - West
        /// </summary>
        protected virtual void FoliageBranch(int x, int y, int z, int side)
        {
            // Up
            blockCaches.Add(new BlockCache(x, y + 1, z, idLeaves, (byte)(NextInt(2) * 6)));
            // Down
            blockCaches.Add(new BlockCache(x, y - 1, z, idLeaves, (byte)(1 + NextInt(2) * 6)));

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
