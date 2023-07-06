using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом ровнины
    /// </summary>
    public class BiomePlain : BiomeAbGrass
    {
        public BiomePlain(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.grassPerChunk = 64;
            Decorator.flowersPerChunk = 3;
            Decorator.fruitPerChunk = 1;
            Decorator.oakPerChunk = 1;
            Decorator.randomTree = 32;
            Decorator.randomPiece = 24;
            Decorator.randomThicketsGrass = 12;
        }

        /// <summary>
        /// Получить уровень множителя высоты
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        /// <param name="heightNoise">Высота -1..0..1</param>
        /// <param name="addNoise">Диапазон -1..0..1</param>
        //protected override int GetLevelHeightRobinson(int x, int z, int height, float heightNoise, float addNoise)
        //    => height + (int)(heightNoise * 6f);
    }
}
