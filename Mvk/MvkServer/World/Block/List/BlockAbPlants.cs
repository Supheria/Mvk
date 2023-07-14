using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект растений с случайным смещением, для травы и цветов
    /// </summary>
    public abstract class BlockAbPlants : BlockBase
    {
        protected bool biomeColor;

        public BlockAbPlants(int numberTexture, bool biomeColor = false)
        {
            Material = Materials.GetMaterialCache(EnumMaterial.Sapling);
            Particle = numberTexture;
            SetUnique(true);
            IsCollidable = false;
            Combustibility = true;
            this.biomeColor = biomeColor;
            IgniteOddsSunbathing = 60;
            BurnOdds = 100;
            Resistance = 0f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitQuads();
        }

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public override bool IsLiquidDestruction() => true;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                DropBlockAsItem(worldIn, blockPos, neighborState);
                worldIn.SetBlockToAir(blockPos);
            }
        }

        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
            return enumBlock == EnumBlock.Turf;
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .25f, pos.Y, pos.Z + .25f),
                new vec3(pos.X + .75f, pos.Y + .875f, pos.Z + .75f)) };
        }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        /// <summary>
        /// Стороны целого блока для рендера 0 - 5 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[(xc + zc + xb + zb) & 4];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected virtual void InitQuads()
        {
            vec3[] offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.1875f, 0, .1875f),
                new vec3(-.1875f, 0, .1875f),
                new vec3(.1875f, 0, -.1875f),
                new vec3(-.1875f, 0, -.1875f)
            };

            quads = new QuadSide[5][];
            for (int i = 0; i < 5; i++)
            {
                quads[i] = new QuadSide[] {
                    new QuadSide((byte)(biomeColor ? 1 : 0)).SetTexture(Particle).SetSide(Pole.South, true, 0, 0, 8, 16, 16, 8).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind(),
                    new QuadSide((byte)(biomeColor ? 1 : 0)).SetTexture(Particle).SetSide(Pole.East, true, 8, 0, 0, 8, 16, 16).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind()
                };
            }
        }
    }
}
