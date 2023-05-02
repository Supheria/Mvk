using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Сешанный лес
    /// </summary>
    public class BiomeMixedForest : BiomeAbGrass
    {
        public BiomeMixedForest(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.oakPerChunk = 1;
            Decorator.birchPerChunk = 1;
            Decorator.sprucePerChunk = 1;
            Decorator.fruitPerChunk = 1;
            Decorator.randomTree = 2;

            Decorator.grassPerChunk = 16;
            Decorator.flowersPerChunk = 1;
        }
    }
}
