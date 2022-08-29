using MvkServer.Glm;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом река
    /// </summary>
    public class BiomeRiver : BiomeBase
    {
        public BiomeRiver(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.sandPancakePerChunk = 3;
            Decorator.gravelPancakePerChunk = 2;
            Decorator.clayPancakePerChunk = 1;
            Decorator.grassPerChunk = 8;
            Decorator.flowersPerChunk = 2;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) => 97 + (int)(height * 96f);

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

            float area = Provider.AreaNoise[x << 4 | z];
            if (area > 3f || area < -3f) yh--;

            chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y;

            if (yh < 96)
            {
                for (y = 3; y < yh; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 3; // Дёрн
                }
                chunk.data[x << 12 | z << 8 | yh] = 8; // Земля
                yh++;
                for (y = yh; y < 97; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 13; // Вода
                }
            }
            else
            {
                for (y = 3; y < 95; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 3; // Дёрн
                }
                for (y = 95; y < yh; y++)
                {
                    chunk.data[x << 12 | z << 8 | y] = 8; // Земля
                }
                chunk.data[x << 12 | z << 8 | yh] = 9; // Дёрн
            }
        }
    }
}
