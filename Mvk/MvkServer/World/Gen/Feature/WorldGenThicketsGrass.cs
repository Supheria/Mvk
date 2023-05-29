using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация зарослей травы
    /// </summary>
    public class WorldGenThicketsGrass : WorldGenerator
    {
        /// <summary>
        /// Какой блок ставим
        /// </summary>
        private readonly ushort blockId = (ushort)EnumBlock.TallGrass;
        /// <summary>
        /// Радиус блинчика
        /// </summary>
        private int radius = 5;
        /// <summary>
        /// Максимальная высота
        /// </summary>
        private int maxHeight = 4;

        public override bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos)
        {
            if (world.GetBiome(blockPos) == Biome.EnumBiome.Swamp)
            {
                maxHeight = 4;
                radius = 5;
            }
            else
            {
                maxHeight = 2;
                radius = 8;
            }
            
            // Генератором уменьшаем радиус блинчика
            int size = rand.Next(radius - 2) + 2;
            
            // Стартовая высота
            int height = rand.Next(maxHeight) + 1;

            int bx = blockPos.X;
            int by = blockPos.Y;
            int bz = blockPos.Z;

            BlockPos blockPos2;

            vec2i posCh = chunk.Position;
            ChunkStorage chunkStorage;
            int index, x, y, z, x1, z1, bx2, bz2, yd, iy, yh;
            int check = size * size + 1;

            for (x = bx - size; x <= bx + size; ++x)
            {
                x1 = x - bx;
                for (z = bz - size; z <= bz + size; ++z)
                {
                    z1 = z - bz;
                    if (x1 * x1 + z1 * z1 <= check)
                    {
                        if (posCh.x == x >> 4 && posCh.y == z >> 4)
                        {
                            bx2 = x & 15;
                            bz2 = z & 15;
                            blockPos2 = new BlockPos(x, by, z);
                            y = blockPos2.Y = world.GetChunk(blockPos2).GetHeightGen(bx2, bz2);
                                    
                            if (bx2 >> 4 == 0 && bz2 >> 4 == 0 && y > 0 && y < ChunkBase.COUNT_HEIGHT_BLOCK)
                            {
                                if (height >= maxHeight) height = 0; else height++;

                                // Check Down
                                yd = y - 1;
                                chunkStorage = chunk.StorageArrays[yd >> 4];
                                index = (yd & 15) << 8 | bz2 << 4 | bx2;
                                if (chunkStorage.countBlock > 0)
                                {
                                    if (chunkStorage.data[index] == 9) // Дёрн
                                    {
                                        yh = y + height;
                                        for (iy = y; iy <= yh; iy++)
                                        {
                                            chunkStorage = chunk.StorageArrays[iy >> 4];
                                            index = (iy & 15) << 8 | bz2 << 4 | bx2;
                                            if (chunkStorage.IsEmptyData() || chunkStorage.data[index] == 0)
                                            {
                                                chunkStorage.SetData(index, blockId, (ushort)(iy == yh ? 1 : 0));
                                            }
                                            else
                                            {
                                                iy--;
                                                chunkStorage = chunk.StorageArrays[iy >> 4];
                                                index = (iy & 15) << 8 | bz2 << 4 | bx2;
                                                if (chunkStorage.data[index] == blockId)
                                                {
                                                    chunkStorage.NewMetBlock(index, 0);
                                                }
                                                break;
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
