using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Абстракный класс генерации особенностей, элементов
    /// </summary>
    public abstract class WorldGenerator
    {
        /// <summary>
        /// Генерация блока или столба не выходящего за чанк
        /// </summary>
        /// <param name="world"></param>
        /// <param name="rand"></param>
        /// <param name="blockPos"></param>
        /// <returns></returns>
        public virtual bool Generate(WorldBase world, Rand rand, BlockPos blockPos) => true;

        /// <summary>
        /// Генерация область, которая может выйти за чанк
        /// </summary>
        /// <param name="world"></param>
        /// <param name="chunk"></param>
        /// <param name="rand"></param>
        /// <param name="blockPos"></param>
        /// <returns></returns>
        public virtual bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos) => true;

        /// <summary>
        /// Сгенерировать область в кеш
        /// </summary>
        /// <param name="world"></param>
        /// <param name="rand"></param>
        /// <param name="blockPos"></param>
        protected virtual bool GenerateCache(WorldBase world, Rand rand, BlockPos blockPos) => false;

        /// <summary>
        /// Сменить блок 
        /// </summary>
        protected bool SetBlock(WorldBase world, BlockPos blockPos, vec2i posCh, ushort id)
            => SetBlock(world, blockPos, posCh, new BlockState(id));
        /// <summary>
        /// Сменить блок 
        /// </summary>
        protected bool SetBlock(WorldBase world, BlockPos blockPos, vec2i posCh, BlockState blockState)
        {
            if (posCh.x == blockPos.X >> 4 && posCh.y == blockPos.Z >> 4)
            {
                return world.SetBlockState(blockPos, blockState, 0);
            }
            return false;
        }

    }
}
