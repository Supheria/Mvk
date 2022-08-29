using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом болотный
    /// </summary>
    public class BiomeSwamp : BiomeBase
    {
        public BiomeSwamp(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.oakPerChunk = 1;
            Decorator.fruitPerChunk = 1;
            Decorator.grassPerChunk = 32;
            Decorator.sandPancakePerChunk = 1;
            Decorator.clayPancakePerChunk = 4;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river)
        {
            int yh = 96 + (int)(height * 96f);
            // пляшки чтоб понизить, больше в воде было
            float area = Provider.AreaNoise[x << 4 | z];
            if (area > 2f || area < -2f) yh--;
            return yh;
        }
    }
}
