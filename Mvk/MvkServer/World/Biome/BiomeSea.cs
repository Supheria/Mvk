using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом моря
    /// </summary>
    public class BiomeSea : BiomeBase
    {
        public BiomeSea(ChunkProviderGenerate chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            // Увеличиваем глубину в 4 раза
            height *= 4;
            int yh = 95 + (int)(height * 96f);
            int y;

            for (y = 3; y < yh; y++)
            {
                chunk.data[x << 12 | z << 8 | y] = 3;
            }
            chunk.data[x << 12 | z << 8 | yh] = 11;
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
