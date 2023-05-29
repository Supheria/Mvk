using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок длинной травы
    /// </summary>
    public class BlockTallGrass : BlockAbSapling
    {
        /***
         * Met
         * 0 - низ
         * 1 - вверх
         */

        /// <summary>
        /// Блок длинной травы
        /// </summary>
        public BlockTallGrass() : base(209, new vec3(.56f, .73f, .35f)) { }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) 
            => boxes[((xc + zc + xb + zb) & 4) + met * 5];

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .0625f, pos.Y, pos.Z + .0625f),
                new vec3(pos.X + .9375f, pos.Y + (met == 0 ? 1 : .75f), pos.Z + .9375f)) };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitBoxs()
        {
            vec3[] offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.1875f, 0, .1875f),
                new vec3(-.1875f, 0, .1875f),
                new vec3(.1875f, 0, -.1875f),
                new vec3(-.1875f, 0, -.1875f)
            };

            boxes = new Box[10][];
            int idx;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    idx = j + i * 5;
                    boxes[idx] = new Box[] {
                        new Box()
                        {
                            From = new vec3(-.2f, 0, .5f),
                            To = new vec3(1.2f, 1f, .5f),
                            RotateYaw = glm.pi45,
                            Faces = new Face[]
                            {
                                new Face(Pole.North, Particle + i, true, Color).SetBiomeColor(),
                                new Face(Pole.South, Particle + i, true, Color).SetBiomeColor(),
                            }
                        },
                        new Box()
                        {
                            From = new vec3(.5f, 0, -.2f),
                            To = new vec3(.5f, 1f, 1.2f),
                            RotateYaw = glm.pi45,
                            Faces = new Face[]
                            {
                                new Face(Pole.East, Particle + i, true, Color).SetBiomeColor(),
                                new Face(Pole.West, Particle + i, true, Color).SetBiomeColor()
                            }
                        }
                    };
                    boxes[idx][0].Translate = offsetMet[j];
                    boxes[idx][1].Translate = offsetMet[j];
                }
            }
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing) 
            => state.NewMet(1);

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos)
        {
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
            return enumBlock == EnumBlock.TallGrass || enumBlock == EnumBlock.Turf;
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
               // DropBlockAsItem(worldIn, blockPos, state, 0);
                worldIn.SetBlockToAir(blockPos, 15);
            }
            else if (worldIn.GetBlockState(blockPos.OffsetUp()).IsAir())
            {
                worldIn.SetBlockStateMet(blockPos, 1);
            }
        }
    }
}
