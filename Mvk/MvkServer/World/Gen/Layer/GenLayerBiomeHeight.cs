namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// По биому выставляем коэффициент высоты
    /// </summary>
    public class GenLayerBiomeHeight : GenLayer
    {
        /// <summary>
        /// Масссив высоты для каждого биома /2
        /// 48 - уровень моря
        /// </summary>
        private readonly int[] level = new int[] {
            24, // Sea
            40, // River
            52, // Plain
            52, // Desert
            48, // Beach
            52, // MixedForest
            52, // ConiferousForest
            52, // BirchForest
            52, // Tropics
            48, // Swamp
            96, // Mountains
            72  // MountainsDesert
        };

        public GenLayerBiomeHeight(GenLayer parent) => this.parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arParent = parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];

            for (int i = 0; i < count; i++)
            {
                ar[i] = level[arParent[i]];
            }
            return ar;
        }
    }
}
