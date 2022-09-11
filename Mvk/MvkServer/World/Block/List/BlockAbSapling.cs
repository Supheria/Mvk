using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект саженцев и прочих растений
    /// </summary>
    public abstract class BlockAbSapling : BlockBase
    {
        public BlockAbSapling(int numberTexture) : this(numberTexture, new vec3(1)) { }
        protected BlockAbSapling(int numberTexture, vec3 color)
        {
            Color = color;
            Material = EnumMaterial.Sapling;
            Particle = numberTexture;
            FullBlock = false;
            АmbientOcclusion = false;
            Shadow = false;
            AllSideForcibly = true;
            NoSideDimming = true;
            LightOpacity = 0;
            IsCollidable = false;
            UseNeighborBrightness = true;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitBoxs();
        }

        /// <summary>
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        //public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        //{
        //    if (Check(worldIn, blockPos))
        //    {
        //        return base.Put(worldIn, blockPos, state, side, facing);
        //    }
        //    return false;
        //}

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                DropBlockAsItem(worldIn, blockPos, state, 0);
                worldIn.SetBlockState(blockPos, new BlockState(EnumBlock.Air), 14);
            }
        }

        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos)
        {
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
            return enumBlock == EnumBlock.Dirt || enumBlock == EnumBlock.Turf;
        }

        /// <summary>
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public override bool BlocksNotSame(int met) => true;

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
        /// Инициализация коробок
        /// </summary>
        protected virtual void InitBoxs()
        {
            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    From = new vec3(0, 0, .5f),
                    To = new vec3(1f, 1f, .5f),
                    RotateYaw = glm.pi45,
                    Faces = new Face[]
                    {
                        new Face(Pole.North, Particle),
                        new Face(Pole.South, Particle),
                    }
                },
                new Box()
                {
                    From = new vec3(.5f, 0, 0),
                    To = new vec3(.5f, 1f, 1f),
                    RotateYaw = glm.pi45,
                    Faces = new Face[]
                    {
                        new Face(Pole.East, Particle),
                        new Face(Pole.West, Particle)
                    }
                }
            }};
        }
    }
}
