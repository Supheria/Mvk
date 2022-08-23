using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    public class WorldGenPlants : WorldGenerator
    {
        /// <summary>
        /// id Блока
        /// </summary>
        private ushort id;

        public void SetId(ushort id) => this.id = id;

        public override bool Generate(WorldBase world, Rand rand, BlockPos blockPos)
        {
            if (Blocks.GetBlockCache(id).CanBlockStay(world, blockPos))
            {
                return world.SetBlockState(blockPos, new BlockState(id), 0);
            }
            return false;
        }
    }
}
