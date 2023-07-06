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
            SetAir();
            IsCollidable = collidable;
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;
    }
}
