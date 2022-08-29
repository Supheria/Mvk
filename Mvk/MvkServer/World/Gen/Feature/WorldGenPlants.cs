using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация растений
    /// </summary>
    public class WorldGenPlants : WorldGenerator
    {
        /// <summary>
        /// id растения
        /// </summary>
        private ushort id;

        public void SetId(ushort id) => this.id = id;

        public override bool Generate(WorldBase world, Rand rand, BlockPos blockPos)
        {
            EnumBlock enumBlock = world.GetBlockState(blockPos).GetEBlock();
            EnumBlock enumBlockDown = world.GetBlockState(blockPos.OffsetDown()).GetEBlock();

            if (enumBlock == EnumBlock.Air && (enumBlockDown == EnumBlock.Turf || enumBlockDown == EnumBlock.Dirt))
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
