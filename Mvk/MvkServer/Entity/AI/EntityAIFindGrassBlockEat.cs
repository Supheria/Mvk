using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Задача найти блок дёрна, подойти к нему и съесть
    /// </summary>
    public class EntityAIFindGrassBlockEat : EntityAIFindSaplingEat
    {
        /// <summary>
        /// Задача найти блок дёрна, подойти к нему и съесть
        /// </summary>
        public EntityAIFindGrassBlockEat(EntityLiving entity, float speed = 1f, float probability = .004f)
            : base(entity, speed, probability) { }

        /// <summary>
        /// Проверка нахождении нужного блока
        /// </summary>
        protected override bool Check(int x, int y, int z)
            => entity.World.GetBlockState(new BlockPos(x, y - 1, z)).GetEBlock() == EnumBlock.Turf;

        /// <summary>
        /// Действие когда дошли до блока
        /// </summary>
        protected override void Action(BlockPos blockPos)
        {
            if (entity.World.GetBlockState(blockPos).GetBlock().Material == EnumMaterial.Sapling)
            {
                entity.World.SetBlockToAir(blockPos, 31);
            }
            else
            {
                blockPos = blockPos.OffsetDown();
                if (entity.World.GetBlockState(blockPos).GetEBlock() == EnumBlock.Turf)
                {
                    entity.World.SetBlockState(blockPos, new BlockState(EnumBlock.Dirt), 31);
                }
            }
        }
    }
}
