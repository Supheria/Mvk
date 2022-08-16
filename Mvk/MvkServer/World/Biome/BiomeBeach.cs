using MvkServer.Glm;
using MvkServer.World.Chunk;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляжа
    /// </summary>
    public class BiomeBeach : BiomeBase
    {
        public BiomeBeach(ChunkProviderGenerate chunkProvider) => Provider = chunkProvider;

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            height *= 3f;
            //float r = 1f - river;
            int yh = 95 + (int)(height * 96f);
            //// Уменьшаем рельеф где вода
            //if (r > .5f) r = glm.cos(river * glm.pi) * .5f + .5f;
            //yh = yh - (int)(((yh - 94) + height * 96f) * r);

            //float area = Provider.AreaNoise[x << 4 | z];
            //if (area > 3f || area < -3f) yh--;// else if (area < -3f && height <= 0) yh++;

            int y;

            for (y = 3; y < yh; y++)
            {
                chunk.data[x << 12 | z << 8 | y] = 3;
            }
            chunk.data[x << 12 | z << 8 | yh] = 10;
            if (yh < 96)
            {
                yh++;
                for (y = yh; y < 97; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 13;
                }
            }
        }
    }
}
