using MvkServer.World.Chunk;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Хвойный лес
    /// </summary>
    public class BiomeConiferousForest : BiomeBase
    {
        public BiomeConiferousForest(ChunkProviderGenerate chunkProvider) => Provider = chunkProvider;

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            int yh = 96 + (int)(height * 96f);
            int y;

            for (y = 3; y <= yh; y++)
            {
                chunk.data[x << 12 | z << 8 | y] = 3;
            }
            if (yh < 96)
            {
                yh++;
                for (y = yh; y < 97; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 13;
                }
            }
            else
            {
                chunk.data[x << 12 | z << 8 | yh] = 9;
            }

            if (Provider.GrassNoise[x << 4 | z] > .55f)
            {
                for (y = yh + 1; y <= yh + 5; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 22;
                }
            }
            
        }
    }
}
