using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Песок
    /// </summary>
    public class BlockSand : BlockUniLoose
    {
        /// <summary>
        /// Блок Песок
        /// </summary>
        public BlockSand() : base(68, EnumItem.PieceSand)
        {
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigSand1, AssetsSample.DigSand2, AssetsSample.DigSand3, AssetsSample.DigSand4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepSand1, AssetsSample.StepSand2, AssetsSample.StepSand3, AssetsSample.StepSand4 };
        }

        /// <summary>
        /// Тон сэмпла сломанного блока
        /// </summary>
        public override float SampleBreakPitch(Rand random) => 1f;
    }
}
