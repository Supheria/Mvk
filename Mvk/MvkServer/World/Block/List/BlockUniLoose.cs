using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект сыпучих блоков
    /// </summary>
    public class BlockUniLoose : BlockUniSolid
    {
        public BlockUniLoose(int numberTexture, vec3 color, EnumItem dropItem, int hardness = 5, float resistance = .5f)
            : base(numberTexture, color, dropItem, hardness, resistance)
        {
            Material = Materials.GetMaterialCache(EnumMaterial.Loose);
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGravel1, AssetsSample.DigGravel2, AssetsSample.DigGravel3, AssetsSample.DigGravel4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepGravel1, AssetsSample.StepGravel2, AssetsSample.StepGravel3, AssetsSample.StepGravel4 };
        }

        /// <summary>
        /// Тон сэмпла сломанного блока
        /// </summary>
        public override float SampleBreakPitch(Rand random) => .8f;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        //public override ItemBase GetItemDropped(BlockState state, Rand rand, int fortune)
        //    => Items.GetItemCache(EnumItem.PieceDirt);
    }
}
