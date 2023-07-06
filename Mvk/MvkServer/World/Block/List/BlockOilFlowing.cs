using MvkServer.Sound;

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
            sideLiquids = new SideLiquid[]
            {
                new SideLiquid(0, 57, 0, 32, 8),
                new SideLiquid(1, 57, 0, 32, 8),
                new SideLiquid(2, 56, 0, 64, 2),
                new SideLiquid(3, 56, 0, 64, 2),
                new SideLiquid(4, 56, 0, 64, 2),
                new SideLiquid(5, 56, 0, 64, 2)
            };
        }

        /// <summary>
        /// Статус для растекания на огонь
        /// </summary>
        protected override bool IsFire(EnumMaterial eMaterial) => false;
    }
}
