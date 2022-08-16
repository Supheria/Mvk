using MvkServer.World.Chunk;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляж
    /// </summary>
    public class BiomeDesert : BiomeBase
    {
        public BiomeDesert(ChunkProviderGenerate chunkProvider) => Provider = chunkProvider;

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

            float area = Provider.AreaNoise[x << 4 | z];
            if (area > 7f) yh++;
            if (area > 6f) yh++;
            if (area > 5f) yh++;

            for (y = 3; y < yh; y++)
            {
                chunk.data[x << 12 | z << 8 | y] = 3;
            }
            chunk.data[x << 12 | z << 8 | yh] = 10;

            if (Provider.GrassNoise[x << 4 | z] > .7f)
            {
                for (y = yh + 1; y <= yh + 5; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 40;
                }
            }
        }
    }
}
