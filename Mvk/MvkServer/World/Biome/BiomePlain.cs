using MvkServer.World.Chunk;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом ровнины
    /// </summary>
    public class BiomePlain : BiomeBase
    {
        public BiomePlain(ChunkProviderGenerate chunkProvider) => Provider = chunkProvider;

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

            //float area = Provider.AreaNoise[x << 4 | z];
            //if (area > 4f || area < -4f) yh--;
            //if (Provider.AreaNoise[x << 4 | z] > 3f) chunk.data[x << 12 | z << 8 | 200] = 3;
            //if (Provider.AreaNoise[x << 4 | z] < -3f) chunk.data[x << 12 | z << 8 | 200] = 6;

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
                chunk.data[x << 12 | z << 8 | yh] = 9;//9
                //chunk.arrayLightBlocks.Add(new Glm.vec3i(x, yh, z));
            }

            float gn = Provider.GrassNoise[x << 4 | z];
            if (gn > .56f) chunk.data[x << 12 | z << 8 | (yh + 1)] = 47;
            else if (gn > .5f) chunk.data[x << 12 | z << 8 | (yh + 1)] = 46;
            else if (gn > .1f) chunk.data[x << 12 | z << 8 | (yh + 1)] = 45;
        }
    }
}
