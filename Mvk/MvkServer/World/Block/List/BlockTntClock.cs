using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок TNT с часами
    /// </summary>
    public class BlockTntClock : BlockTnt
    {
        /// <summary>
        /// Блок TNT с часами
        /// </summary>
        public BlockTntClock() : base()
        {
            strength = 6f;
            distance = 1f;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitBoxs()
        {
            vec3 color = new vec3(.95f, .4f, .4f);
            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 515, color),
                        new Face(Pole.Down, 512, color),
                        new Face(Pole.East, 513, color),
                        new Face(Pole.North, 513, color),
                        new Face(Pole.South, 513, color),
                        new Face(Pole.West, 513, color)
                    }
                }
            }};
        }

        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            base.OnBlockAdded(worldIn, blockPos, state);

            if (worldIn.GetBlockState(blockPos).GetEBlock() == EnumBlock.TntClock)
            {
                // запустить мгновенный тик 1-2 сек до взрыва
                worldIn.SetBlockTick(blockPos, (uint)(worldIn.Rnd.Next(20) + 20));
            }
        }

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            Explosion(world, blockPos);
        }
    }
}
