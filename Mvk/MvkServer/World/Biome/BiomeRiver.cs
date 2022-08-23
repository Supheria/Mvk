using MvkServer.Glm;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом река
    /// </summary>
    public class BiomeRiver : BiomeBase
    {
        public BiomeRiver(ChunkProviderGenerate chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            float riv = 1f - river;
            int yh = 96 + (int)(height * 96f);
            // Уменьшаем рельеф где вода
            if (riv > .5f) riv = glm.cos(river * glm.pi) * .5f + .5f;
            yh = yh - (int)(((yh - 94) + height * 96f) * riv);

            //ColumnUpSeed(x, z);
            //float r = rand.NextFloat();
            //if (r > .8f) yh--;

            float area = Provider.AreaNoise[x << 4 | z];
            if (area > 3f || area < -3f) yh--;// else if (area < -3f && height <= 0) yh++;

            int y;

            if (yh < 96)
            {
                for (y = 3; y < yh; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 3;
                }
                chunk.data[x << 12 | z << 8 | yh] = 12;
                yh++;
                for (y = yh; y < 97; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 13;
                }
            }
            else
            {
                for (y = 3; y < 95; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 3;
                }
                for (y = 95; y < yh; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 8;
                }
                chunk.data[x << 12 | z << 8 | yh] = 9;
            }
        }
    }
}
