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
        /// <summary>
        /// В каком блоке ставим, для замены
        /// </summary>
        private readonly ushort exchangeId;
        /// <summary>
        /// Количество
        /// </summary>
        private readonly int count;

        public WorldGenMinable(BlockState blockState, int count) : this(blockState, count, 3) { }

        public WorldGenMinable(BlockState blockState, int count, ushort exchangeId)
        {
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

            BlockPos bpos = new BlockPos();

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
                        for (y = y4; y <= y5; ++y)
                        {
                            yk = (y + .5f - y3) / (v / 2f);
                            if (xk * xk + yk * yk < 1f)
                            {
                                for (z = z4; z <= z5; ++z)
                                {
                                    zk = (z + .5f - z3) / (h / 2f);
                                    if (xk * xk + yk * yk + zk * zk < 1f)
                                    {
                                        bpos.X = x;
                                        bpos.Y = y;
                                        bpos.Z = z;
                                        if (world.GetBlockState(bpos).data == exchangeId)
                                        {
                                            SetBlock(world, bpos, posCh, blockState);
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
