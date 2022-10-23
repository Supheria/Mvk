using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок тикучей лавы
    /// </summary>
    public class BlockLavaFlowing : BlockAbLiquidFlowing
    {
        /// <summary>
        /// Блок тикучей лавы
        /// </summary>
        public BlockLavaFlowing() : base()
        {
            material = EnumMaterial.Lava;
            eBlock = EnumBlock.Lava;
            eBlockFlowing = EnumBlock.LavaFlowing;
            tickRate = 30;
            stepWave = 4;
            LightOpacity = 0;

            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            // Translucent = true; 
            NeedsRandomTick = true;
            LightValue = 15;
            Material = EnumMaterial.Lava;
            samplesStep = new AssetsSample[0];
            //samplesBreak = new AssetsSample[] { AssetsSample.LiquidSplash1, AssetsSample.LiquidSplash2 };
            //samplesStep = new AssetsSample[] { AssetsSample.LiquidSwim1, AssetsSample.LiquidSwim2, AssetsSample.LiquidSwim3, AssetsSample.LiquidSwim4 };
            faces = new Face[]
            {
                new Face(60).SetAnimation(32, 4),
                new Face(59).SetAnimation(32, 1)
            };
            InitBoxs();
        }

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[16][];

            for (int i = 0; i < 16; i++)
            {
                boxes[i] = new Box[] {
                    new Box()
                    {
                        From = new vec3(0),
                        To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[i + 1], MvkStatic.Xy[16]),
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, 61).SetAnimation(32, 4),
                            new Face(Pole.Down, 61).SetAnimation(32, 4)
                        }
                    },
                    new Box()
                    {
                        From = new vec3(0),
                        To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[i + 1], MvkStatic.Xy[16]),
                        UVFrom = new vec2(0, MvkStatic.Uv[16 - i]),
                        UVTo = new vec2(MvkStatic.Uv[16], MvkStatic.Uv[16]),
                        Faces = new Face[]
                        {
                            new Face(Pole.East, 60).SetAnimation(64, 1),
                            new Face(Pole.North, 60).SetAnimation(64, 1),
                            new Face(Pole.South, 60).SetAnimation(64, 1),
                            new Face(Pole.West, 60).SetAnimation(64, 1)
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
            => SetFireTo(world, blockPos, random);
    }
}
