using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект сыпучих блоков
    /// </summary>
    public abstract class BlockAbLoose : BlockBase
    {
        public BlockAbLoose(int numberTexture, vec3 color)
        {
            Material = EnumMaterial.Loose;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGravel1, AssetsSample.DigGravel2, AssetsSample.DigGravel3, AssetsSample.DigGravel4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepGravel1, AssetsSample.StepGravel2, AssetsSample.StepGravel3, AssetsSample.StepGravel4 };
            Particle = numberTexture;
            Resistance = .5f;
            InitBoxs(numberTexture, false, color);
        }

        /// <summary>
        /// Тон сэмпла сломанного блока,
        /// </summary>
        public override float SampleBreakPitch(Rand random) => .8f;
    }
}
