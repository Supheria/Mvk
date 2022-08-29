using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация минералов и всяких блоков ввиде скопления
    /// </summary>
    public class WorldGenMinable : WorldGenerator
    {
        /// <summary>
        /// Какой блок ставим
        /// </summary>
        private BlockState blockState;
        private readonly ushort blockId;
        /// <summary>
        /// В каком блоке ставим, для замены
        /// </summary>
        private readonly ushort exchangeId;
        /// <summary>
        /// Количество, меньше 3 нельзя, не будет совсем
        /// </summary>
        private readonly int count;

        public WorldGenMinable(BlockState blockState, int count) : this(blockState, count, 3) { }

        public WorldGenMinable(BlockState blockState, int count, ushort exchangeId)
        {
            blockId = blockState.data;
            this.blockState = blockState;
            this.count = count;
            this.exchangeId = exchangeId;
        }

        public override bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos)
        {
            float k, x3, y3, z3, c, h, v, xk, yk, zk;
            int x4, y4, z4, x5, y5, z5, i, x, y, z;
            float f = rand.NextFloat() * glm.pi;
            vec2i posCh = chunk.Position;

            float x1 = blockPos.X + 8f + glm.sin(f) * count / 8f;
            float x2 = blockPos.X + 8f - glm.sin(f) * count / 8f;
            float z1 = blockPos.Z + 8f + glm.cos(f) * count / 8f;
            float z2 = blockPos.Z + 8f - glm.cos(f) * count / 8f;
            float y1 = blockPos.Y + rand.Next(3) - 2;
            float y2 = blockPos.Y + rand.Next(3) - 2;

            int index, bx, by, bz, chy;
            ChunkStorage chunkStorage;

            // TODO::2022-08-29 Оптимизировать минералы, занимает примерно ~2 мс с чанка
            for (i = 0; i < count; ++i)
            {
                k = (float)i / (float)count;
                x3 = x1 + (x2 - x1) * k;
                y3 = y1 + (y2 - y1) * k;
                z3 = z1 + (z2 - z1) * k;
                c = rand.NextFloat() * count / 16f;
                h = (glm.sin(glm.pi * k) + 1f) * c + 1f;
                v = (glm.sin(glm.pi * k) + 1f) * c + 1f;
                x4 = Mth.Floor(x3 - h / 2f);
                y4 = Mth.Floor(y3 - v / 2f);
                z4 = Mth.Floor(z3 - h / 2f);
                x5 = Mth.Floor(x3 + h / 2f);
                y5 = Mth.Floor(y3 + v / 2f);
                z5 = Mth.Floor(z3 + h / 2f);

                for (x = x4; x <= x5; ++x)
                {
                    xk = (x + .5f - x3) / (h / 2f);
                    if (xk * xk < 1f)
                    {
                        bx = x & 15;
                        for (y = y4; y <= y5; ++y)
                        {
                            yk = (y + .5f - y3) / (v / 2f);
                            if (xk * xk + yk * yk < 1f)
                            {
                                chy = y >> 4;
                                by = y & 15;
                                for (z = z4; z <= z5; ++z)
                                {
                                    zk = (z + .5f - z3) / (h / 2f);
                                    if (xk * xk + yk * yk + zk * zk < 1f)
                                    {
                                        if (posCh.x == x >> 4 && posCh.y == z >> 4)
                                        {
                                            bz = z & 15;
                                            if (bx >> 4 == 0 && bz >> 4 == 0 && chy >= 0 && chy < ChunkBase.COUNT_HEIGHT)
                                            {
                                                chunkStorage = chunk.StorageArrays[chy];
                                                index = by << 8 | bz << 4 | bx;
                                                if (chunkStorage.countBlock > 0 && chunkStorage.data[index] == exchangeId)
                                                {
                                                    chunkStorage.data[index] = blockId;
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
            return true;
        }

    }
}
