using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Хвойный лес
    /// </summary>
    public class BiomeConiferousForest : BiomeAbGrass
    {
        public BiomeConiferousForest(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider) { }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.sprucePerChunk = 4;
            Decorator.grassPerChunk = 12;
            Decorator.randomThicketsGrass = 8;
        }
    }
}
