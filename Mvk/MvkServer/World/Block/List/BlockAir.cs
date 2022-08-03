using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок воздуха, пустота
    /// </summary>
    public class BlockAir : BlockBase
    {
        /// <summary>
        /// Блок воздуха, пустотаб может сталкиваться
        /// </summary>
        /// <param name="collidable">Выбрать может ли блок сталкиваться</param>
        public BlockAir(bool collidable = false)
        {
            IsAir = true;
            IsAction = false;
            IsCollidable = collidable;
            IsParticle = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            LightOpacity = 0;
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }
    }
}
