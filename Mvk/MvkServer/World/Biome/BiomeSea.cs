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
        /// Заполняем тело
        /// </summary>
        protected override void BodyRobinson(int yb, int yh, int index)
        {
            for (int y = yb; y <= yh; y++)
            {
                chunk.id[index | y] = (ushort)(yh < HEIGHT_WATER ? blockIdBody : 3);
            }
        }
    }
}
