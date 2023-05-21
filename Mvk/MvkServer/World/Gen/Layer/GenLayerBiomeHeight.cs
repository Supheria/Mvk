namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// По биому выставляем коэффициент высоты
    /// </summary>
    public class GenLayerBiomeHeight : GenLayer
    {
        /// <summary>
        /// Уровень земель над водой
        /// </summary>
        public static int LevelEarth = 52;
        /// <summary>
        /// Масссив высоты для каждого биома /2
        /// 48 - уровень моря
        /// </summary>
        private readonly int[] level = new int[] {
            12, // Sea
            38, // River
            LevelEarth, // Plain
            LevelEarth, // Desert
            48, // Beach
            LevelEarth, // MixedForest
            LevelEarth, // ConiferousForest
            LevelEarth, // BirchForest
            LevelEarth, // Tropics
            48, // Swamp
            74, // Mountains 96
            64  // MountainsDesert 72

            //42, // Sea
            //44, // River
            //LevelEarth, // Plain
            //LevelEarth, // Desert
            //48, // Beach
            //LevelEarth, // MixedForest
            //LevelEarth, // ConiferousForest
            //LevelEarth, // BirchForest
            //LevelEarth, // Tropics
            //48, // Swamp
            //58, // Mountains
            //54  // MountainsDesert

            //-4, // Sea
            //-2, // River
            //1, // Plain
            //1, // Desert
            //0, // Beach
            //1, // MixedForest
            //1, // ConiferousForest
            //1, // BirchForest
            //1, // Tropics
            //0, // Swamp
            //5, // Mountains
            //3  // MountainsDesert

            //24, // Sea
            //40, // River
            //60, // Plain
            //60, // Desert
            //48, // Beach
            //60, // MixedForest
            //60, // ConiferousForest
            //60, // BirchForest
            //60, // Tropics
            //48, // Swamp
            //144, // Mountains
            //96  // MountainsDesert

            //24, // Sea
            //40, // River
            //64, // Plain
            //64, // Desert
            //48, // Beach
            //64, // MixedForest
            //64, // ConiferousForest
            //64, // BirchForest
            //64, // Tropics
            //48, // Swamp
            //240, // Mountains
            //144  // MountainsDesert

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
