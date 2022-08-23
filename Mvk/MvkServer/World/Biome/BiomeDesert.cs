using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляж
    /// </summary>
    public class BiomeDesert : BiomeBase
    {
        public BiomeDesert(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.genCactus = new Gen.Feature.WorldGenCactus();
            Decorator.cactiPerChunk = 10;
            Decorator.grassPerChunk = 0;
            blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) => 96 + (int)(height * 96f);

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

        //    float area = Provider.AreaNoise[x << 4 | z];
        //    if (area > 7f) yh++;
        //    if (area > 6f) yh++;
        //    if (area > 5f) yh++;

        //    for (y = 3; y < yh; y++)
        //    {
        //        chunk.data[x << 12 | z << 8 | y] = 3;
        //    }
        //    chunk.data[x << 12 | z << 8 | yh] = 10;

        //    ColumnUpSeed(x, z);
        //    float r = rand.NextFloat();
        //    if (r > .999f)
        //    {
        //        int h = rand.Next(4);
        //        yh++;
        //        for (y = yh; y <= yh + h; y++)
        //        {
        //            chunk.data[x << 12 | z << 8 | y] = 40;
        //        }
        //    }
        //}
    }
}
