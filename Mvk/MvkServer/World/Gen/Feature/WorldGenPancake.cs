using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация блинчика различных блоков в воде не ниже 88 уровня
    /// </summary>
    public class WorldGenPancake : WorldGenerator
    {
        /// <summary>
        /// Какой блок ставим
        /// </summary>
        private readonly ushort blockId;
        /// <summary>
        /// Радиус блинчика
        /// </summary>
        private readonly int radius;
        /// <summary>
        /// Высота блинчика
        /// </summary>
        private readonly int height;

        public WorldGenPancake(EnumBlock enumBlock, int radius, int height)
        {
            blockId = (ushort)enumBlock;
            this.radius = radius;
            this.height = height;
        }

        public override bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos)
        {
            // Генератором уменьшаем радиус блинчика
            int size = rand.Next(radius - 4) + 4;
            // Высота блинчика 2 - 7
            int h = rand.Next(height - 3) + 2;
            // Центр блинчика от точки старта
            int center = rand.Next(h);

            if (world.GetBlockState(blockPos).GetBlock().Material.EMaterial != EnumMaterial.Water)
            {
                return false;
            }
            while(true)
            {
                blockPos = blockPos.OffsetDown();
                if (world.GetBlockState(blockPos).GetBlock().Material.EMaterial != EnumMaterial.Water)
                {
                    break;
                }
            }
            int bx = blockPos.X;
            int by = blockPos.Y;
            int bz = blockPos.Z;
            // Если меньше 88 значит глубоко, блинчики не нужны
            if (by < BiomeBase.HEIGHT_PANCAKE_MIN) return false;

            vec2i posCh = chunk.Position;
            ChunkStorage chunkStorage;
            int index, x, y, z, x1 , z1, bx2, bz2;

            int ymin = by - center;
            int ymax = ymin + h;
            ushort id;
            int check = size * size + 1;

            for (x = bx - size; x <= bx + size; ++x)
            {
                x1 = x - bx;
                for (z = bz - size; z <= bz + size; ++z)
                {
                    z1 = z - bz;
                    if (x1 * x1 + z1 * z1 <= check)
                    {
                        for (y = ymin; y <= ymax; ++y)
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
                                    if (chunkStorage.countBlock > 0)
                                    {
                                        id = chunkStorage.data[index];
                                        if (id >= 3 && id <= 12)
                                            chunkStorage.data[index] = blockId;
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
