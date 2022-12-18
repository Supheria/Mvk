using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей нефти
    /// </summary>
    public class BlockOil : BlockAbLiquid
    {
        /// <summary>
        /// Блок стоячей нефти
        /// </summary>
        public BlockOil() : base()
        {
            material = EnumMaterial.Oil;
            eBlock = EnumBlock.Oil;
            eBlockFlowing = EnumBlock.OilFlowing;
            tickRate = 15;
            stepWave = 3;

            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            //Translucent = true;
            Combustibility = true;
            IgniteOddsSunbathing = 100;
            BurnOdds = 100;
            LightOpacity = 1;
            Material = EnumMaterial.Oil;
            samplesStep = new AssetsSample[0];
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyLava1, AssetsSample.BucketEmptyLava2, AssetsSample.BucketEmptyLava3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillLava1, AssetsSample.BucketFillLava2, AssetsSample.BucketFillLava3 };
            faces = new Face[]
            {
                new Face(57).SetAnimation(32, 8),
                new Face(56).SetAnimation(64, 2)
            };
            InitBoxs();
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
           // vec3 color = new vec3(0.24f, 0.45f, 0.88f);

            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 57).SetAnimation(32, 8),
                        new Face(Pole.Down, 57).SetAnimation(32, 8),
                        new Face(Pole.East, 56).SetAnimation(32, 2),
                        new Face(Pole.North, 56).SetAnimation(32, 2),
                        new Face(Pole.South, 56).SetAnimation(32, 2),
                        new Face(Pole.West, 56).SetAnimation(32, 2)
                    }
                }
            }};
        }

        /// <summary>
        /// Статус для растекания на огонь
        /// </summary>
        protected override bool IsFire(EnumMaterial eMaterial) => false;

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            BlockPos blockPosUp = blockPos.OffsetUp();
            if (world.GetBlockState(blockPosUp).GetEBlock() == EnumBlock.Water)
            {
                // блок воды сверху, значит меняем их местами
                world.SetBlockState(blockPosUp, new BlockState(EnumBlock.Oil), 14);
                world.SetBlockState(blockPos, new BlockState(EnumBlock.Water), 14);
            }
            else
            {
                base.UpdateTick(world, blockPos, blockState, random);
            }
        }
    }
}
