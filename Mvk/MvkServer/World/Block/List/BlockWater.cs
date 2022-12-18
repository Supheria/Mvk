using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей воды
    /// </summary>
    public class BlockWater : BlockAbLiquid
    {
        /// <summary>
        /// Блок стоячей воды
        /// </summary>
        public BlockWater() : base()
        {
            Translucent = true;
            BiomeColor = true;
            LightOpacity = 1;
            Material = EnumMaterial.Water;
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyWater1, AssetsSample.BucketEmptyWater2, AssetsSample.BucketEmptyWater3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillWater1, AssetsSample.BucketFillWater2, AssetsSample.BucketFillWater3 };
            faces = new Face[]
            {
                new Face(63, true).SetAnimation(32, 2),
                new Face(62, true).SetAnimation(64, 1)
            };

            InitBoxs();
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 color = new vec3(0.24f, 0.45f, 0.88f);

            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 63, true, color).SetAnimation(32, 2),
                        new Face(Pole.Down, 63, true, color).SetAnimation(32, 2),
                        new Face(Pole.East, 62, true, color).SetAnimation(64, 1),
                        new Face(Pole.North, 62, true, color).SetAnimation(64, 1),
                        new Face(Pole.South, 62, true, color).SetAnimation(64, 1),
                        new Face(Pole.West, 62, true, color).SetAnimation(64, 1)
                    }
                }
            }};
        }

        

        
    }
}
