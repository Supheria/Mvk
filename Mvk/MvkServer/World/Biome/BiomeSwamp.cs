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
            Decorator.grassPerChunk = 5;
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

        ///// <summary>
        ///// Возращаем сгенерированный столбец
        ///// </summary>
        ///// <param name="x">X 0..15</param>
        ///// <param name="z">Z 0..15</param>
        ///// <param name="height">Высота -1..0..1</param>
        ///// <param name="river">Определение центра реки 1..0..1</param>
        //public override void Column(int x, int z, float height, float river)
        //{
        //    int yh = 96 + (int)(height * 96f);
        //    int y;

        //    // пляшки чтоб понизить, больше в воде было
        //    float area = Provider.AreaNoise[x << 4 | z];
        //    if (area > 2f || area < -2f) yh--;

        //    if (yh < 96)
        //    {
        //        for (y = 3; y < yh; y++)
        //        {
        //            chunk.data[x << 12 | z << 8 | y] = 3;
        //        }
        //        chunk.data[x << 12 | z << 8 | yh] = 12;
        //        yh++;
        //        for (y = yh; y < 97; y++)
        //        {
        //            chunk.data[x << 12 | z << 8 | y] = 13;
        //        }
        //    }
        //    else
        //    {
        //        for (y = 3; y < 95; y++)
        //        {
        //            chunk.data[x << 12 | z << 8 | y] = 3;
        //        }
        //        for (y = 95; y < yh; y++)
        //        {
        //            chunk.data[x << 12 | z << 8 | y] = 8;
        //        }
        //        chunk.data[x << 12 | z << 8 | yh] = 9;
        //    }
        //}
    }
}
