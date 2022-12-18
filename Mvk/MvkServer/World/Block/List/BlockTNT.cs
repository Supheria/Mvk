using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок TNT
    /// </summary>
    public class BlockTnt : BlockAbElectricity
    {
        protected float power = .5f;
        protected float distance = 3f;

        /// <summary>
        /// Блок TNT
        /// </summary>
        public BlockTnt() : base(513)
        {
            CanDropFromExplosion = false;
            InitBoxs();
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected virtual void InitBoxs()
        {
            vec3 color = new vec3(.95f, .4f, .4f);
            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 514, color),
                        new Face(Pole.Down, 512, color),
                        new Face(Pole.East, 513, color),
                        new Face(Pole.North, 513, color),
                        new Face(Pole.South, 513, color),
                        new Face(Pole.West, 513, color)
                    }
                }
            }};
        }

        /// <summary>
        /// Действие блока после его установки
        /// </summary>
        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            if (CheckNeighborBlock(worldIn, blockPos.OffsetSouth()))
            {
                Explosion(worldIn, blockPos);
                return;
            }
            if (CheckNeighborBlock(worldIn, blockPos.OffsetEast()))
            {
                Explosion(worldIn, blockPos);
                return;
            }
            if (CheckNeighborBlock(worldIn, blockPos.OffsetNorth()))
            {
                Explosion(worldIn, blockPos);
                return;
            }
            if (CheckNeighborBlock(worldIn, blockPos.OffsetWest()))
            {
                Explosion(worldIn, blockPos);
                return;
            }
            if (CheckNeighborBlock(worldIn, blockPos.OffsetUp()))
            {
                Explosion(worldIn, blockPos);
                return;
            }
            if (CheckNeighborBlock(worldIn, blockPos.OffsetDown()))
            {
                Explosion(worldIn, blockPos);
            }
        }

        /// <summary>
        /// Проверка соседнего блока на взрыв
        /// </summary>
        private bool CheckNeighborBlock(WorldBase worldIn, BlockPos blockPos)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            EnumBlock enumBlock = blockState.GetEBlock();
            return enumBlock == EnumBlock.Lava || enumBlock == EnumBlock.LavaFlowing || enumBlock == EnumBlock.Fire;
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos pos, BlockState state, BlockBase neighborBlock)
        {
            if (neighborBlock.EBlock == EnumBlock.Fire || neighborBlock.Material == EnumMaterial.Lava)
            {
                Explosion(worldIn, pos);
            }
        }

        /// <summary>
        /// Блок подключен к электроэнергии
        /// </summary>
        public override void UnitConnectedToElectricity(WorldBase world, BlockPos blockPos)
        {
            Explosion(world, blockPos);
        }

        /// <summary>
        /// Имеется ли у блока подключение к электроэнергии
        /// </summary>
        public override bool IsUnitConnectedToElectricity() => true;
        /// <summary>
        /// Имеется ли у блока разрядка к электроэнергии
        /// </summary>
        public override bool IsUnitDischargeToElectricity() => true;

        /// <summary>
        /// Анализ соседей взрывчаток и взрыв
        /// </summary>
        protected void Explosion(WorldBase world, BlockPos blockPos)
        {
            float power = this.power;
            float distance = this.distance;
            world.SetBlockToAir(blockPos);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) continue;

                        if (world.GetBlockState(blockPos.Offset(x, y, z)).GetEBlock() == EnumBlock.Tnt)
                        {
                            power = (power + 1) * .97f;
                            distance = (distance + 1) * .95f;
                        }
                    }
                }
            }
            world.CreateExplosion(blockPos.ToVec3() + .5f, power, distance);
        }
    }
}
