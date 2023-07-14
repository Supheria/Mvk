using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок длинной травы
    /// </summary>
    public class BlockTallGrass : BlockAbPlants
    {
        /***
         * Met
         * 0 - низ
         * 1 - вверх
         */

        /// <summary>
        /// Блок длинной травы
        /// </summary>
        public BlockTallGrass() : base(209) { }

        /// <summary>
        /// Блок который замедляет сущность в перемещении на ~30%
        /// </summary>
        public override bool IsSlow(BlockState state) => state.met == 0;

        /// <summary>
        /// Стороны целого блока для рендера 0 - 5 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) 
            => quads[((xc + zc + xb + zb) & 4) + met * 5];


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
        protected override void InitQuads()
        {
            vec3[] offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.1875f, 0, .1875f),
                new vec3(-.1875f, 0, .1875f),
                new vec3(.1875f, 0, -.1875f),
                new vec3(-.1875f, 0, -.1875f)
            };

            quads = new QuadSide[10][];
            for (int i = 0; i < 5; i++)
            {
                quads[i] = new QuadSide[] {
                    new QuadSide(1).SetTexture(Particle).SetSide(Pole.North, true, -2, 0, 8, 18, 16, 8).SetRotate(glm.pi45).SetTranslate(offsetMet[i]),
                    new QuadSide(1).SetTexture(Particle).SetSide(Pole.West, true, 8, 0, -2, 8, 16, 18).SetRotate(glm.pi45).SetTranslate(offsetMet[i])
                };
                quads[i + 5] = new QuadSide[] {
                    new QuadSide(1).SetTexture(Particle + 1).SetSide(Pole.North, true, -2, 0, 8, 18, 16, 8).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind(),
                    new QuadSide(1).SetTexture(Particle + 1).SetSide(Pole.West, true, 8, 0, -2, 8, 16, 18).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind()
                };
            }
        }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing) 
            => state.NewMet(1);

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
            return enumBlock == EnumBlock.TallGrass || enumBlock == EnumBlock.Turf;
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
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

        #region Drop

        /// <summary>
        /// Получите количество выпавших на основе данного уровня удачи
        /// </summary>
        protected override int QuantityDroppedWithBonus(ItemAbTool itemTool, Rand random) 
            => (itemTool != null && itemTool.EItem == EnumItem.AxeSteel) || random.Next(10) == 0 ? 1 : 0;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool) 
            => Items.GetItemCache(EnumItem.DryGrass);

        #endregion
    }
}
