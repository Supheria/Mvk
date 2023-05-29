using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок травы
    /// </summary>
    public class BlockGrass : BlockAbPlants
    {
        /// <summary>
        /// Блок травы
        /// </summary>
        public BlockGrass() : base(195, new vec3(.56f, .73f, .35f))
        {
            IsReplaceable = true;
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .125f, pos.Y, pos.Z + .125f),
                new vec3(pos.X + .875f, pos.Y + .875f, pos.Z + .875f)) };
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }
    }
}
