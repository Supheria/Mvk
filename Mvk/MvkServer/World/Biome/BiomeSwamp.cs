using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом болотный
    /// </summary>
    public class BiomeSwamp : BiomeAbGrass
    {
        public BiomeSwamp(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBiomDebug = (ushort)EnumBlock.Clay;
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.oakPerChunk = 1;
            Decorator.fruitPerChunk = 1;
            Decorator.grassPerChunk = 16;
            Decorator.sandPancakePerChunk = 1;
            Decorator.clayPancakePerChunk = 4;
            Decorator.tinaPerChunk = 96;
            Decorator.thicketsGrassPerChunk = 4;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river)
        {
            int yh = HEIGHT_WATER + (int)(height * HEIGHT_HILL);
            // пляшки чтоб понизить, больше в воде было
            float area = Provider.AreaNoise[x << 4 | z];
            if (area > 2f || area < -2f) yh--;
            return yh;
        }
    }
}
