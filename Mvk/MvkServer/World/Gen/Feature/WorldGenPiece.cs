using MvkServer.Util;
using MvkServer.World.Chunk;
namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация маленьких блоков
    /// </summary>
    public class WorldGenPiece : WorldGenerator
    {
        /// <summary>
        /// id растения
        /// </summary>
        private ushort id;

        public void SetId(ushort id) => this.id = id;

        public override bool Generate(WorldBase world, Rand rand, BlockPos blockPos)
        {
            if (world.GetBlockState(blockPos).IsAir() 
                && world.GetBlockState(blockPos.OffsetDown()).GetBlock().FullBlock)
            {
                ChunkBase chunk = world.GetChunk(blockPos);
                chunk.StorageArrays[blockPos.Y >> 4].SetData(
                    (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15), id
                );
                return true;
            }

            return false;
        }
    }
}
