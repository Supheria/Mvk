using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Горы
    /// </summary>
    public class BiomeMountains : BiomeBase
    {
        public BiomeMountains(ChunkProviderGenerate chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            float h = height - .125f;
            h *= 4f;
            height = h + .125f;

            int yh = 96 + (int)(height * 96f);
            int y;

            for (y = 3; y <= yh; y++)
            {
                chunk.data[x << 12 | z << 8 | y] = 3;
            }
        }
    }
}
