using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Берёзовый лес
    /// </summary>
    public class BiomeBirchForest : BiomeAbGrass
    {
        public BiomeBirchForest(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.birchPerChunk = 4;
            Decorator.grassPerChunk = 8;
            Decorator.flowersPerChunk = 5;
        }
    }
}
