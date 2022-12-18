using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей лавы
    /// </summary>
    public class BlockLava : BlockAbLiquid
    {
        /// <summary>
        /// Блок стоячей лавы
        /// </summary>
        public BlockLava() : base()
        {
            material = EnumMaterial.Lava;
            eBlock = EnumBlock.Lava;
            eBlockFlowing = EnumBlock.LavaFlowing;
            tickRate = 30;
            stepWave = 4;
            Particle = 60;

            NeedsRandomTick = true;
            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            // Translucent = true; 
            // LightOpacity = 4;
            LightValue = 15;
            Material = EnumMaterial.Lava;
            samplesStep = new AssetsSample[0];
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyLava1, AssetsSample.BucketEmptyLava2, AssetsSample.BucketEmptyLava3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillLava1, AssetsSample.BucketFillLava2, AssetsSample.BucketFillLava3 };
            faces = new Face[]
            {
                new Face(60).SetAnimation(32, 4),
                new Face(59).SetAnimation(64, 1)
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
                        new Face(Pole.Up, 60).SetAnimation(32, 4),
                        new Face(Pole.Down, 60).SetAnimation(32, 4),
                        new Face(Pole.East, 59).SetAnimation(32, 1),
                        new Face(Pole.North, 59).SetAnimation(32, 1),
                        new Face(Pole.South, 59).SetAnimation(32, 1),
                        new Face(Pole.West, 59).SetAnimation(32, 1)
                    }
                }
            }};
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random) 
            => SetFireTo(world, blockPos, random);
    }
}
