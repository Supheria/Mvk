using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом моря
    /// </summary>
    public class BiomeSea : BiomeBase
    {
        public BiomeSea(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBody = blockIdUp = (ushort)EnumBlock.Gravel;
            blockIdBiomDebug = (ushort)EnumBlock.Water;
            isBlockBody = true;
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.sandPancakePerChunk = 2;
            Decorator.dirtPancakePerChunk = 1;
            Decorator.oilPerChunk = 20;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) 
            => HEIGHT_WATER_MINUS + (int)(height * HEIGHT_HILL_SEA);

        /// <summary>
        /// Получить уровень множителя высоты
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        /// <param name="heightNoise">Высота -1..0..1</param>
        /// <param name="addNoise">Диапазон -1..0..1</param>
        protected override int GetLevelHeightRobinson(int x, int z, int height, float heightNoise, float addNoise)
        {
            if (height < 12) addNoise *= addNoise;
            return height + (int)(heightNoise * (heightNoise < 0 ? 18f : 6f)) + (int)(addNoise * 4f);
        }
    }
}
