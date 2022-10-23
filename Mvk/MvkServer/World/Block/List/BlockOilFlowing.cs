using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок тикучей нефти
    /// </summary>
    public class BlockOilFlowing : BlockAbLiquidFlowing
    {
        /// <summary>
        /// Блок тикучей нефти
        /// </summary>
        public BlockOilFlowing() : base()
        {
            material = EnumMaterial.Oil;
            eBlock = EnumBlock.Oil;
            eBlockFlowing = EnumBlock.OilFlowing;
            tickRate = 15;
            stepWave = 3;
            LightOpacity = 0;

            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            //Translucent = true;
            Combustibility = true;
            IgniteOddsSunbathing = 100;
            BurnOdds = 100;
            LightOpacity = 0;
            Material = EnumMaterial.Oil;
            samplesStep = new AssetsSample[0];
            faces = new Face[]
            {
                new Face(57).SetAnimation(32, 8),
                new Face(56).SetAnimation(32, 16)
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
                            new Face(Pole.Up, 59).SetAnimation(32, 8),
                            new Face(Pole.Down, 59).SetAnimation(32, 8)
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
                            new Face(Pole.East, 58).SetAnimation(64, 4),
                            new Face(Pole.North, 58).SetAnimation(64, 4),
                            new Face(Pole.South, 58).SetAnimation(64, 4),
                            new Face(Pole.West, 58).SetAnimation(64, 4)
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Статус для растекания на огонь
        /// </summary>
        protected override bool IsFire(EnumMaterial eMaterial) => false;
    }
}
