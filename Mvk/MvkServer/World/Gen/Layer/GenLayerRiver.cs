using MvkServer.World.Biome;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Создание реки
    /// </summary>
    public class GenLayerRiver : GenLayer
    {
        private readonly int idRiver = (int)EnumBiome.River;
        private readonly int idSwamp = (int)EnumBiome.Swamp;
        private readonly int idConiferousForest = (int)EnumBiome.ConiferousForest;

        public GenLayerRiver(GenLayer parent) => this.parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, idx;
            bool c11;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = arParent[x + 1 + (z + 1) * pw];
                    c11 = Check(idx);

                    ar[x + z * width] = (c11 == Check(arParent[x + 0 + (z + 1) * pw]) 
                        && c11 == Check(arParent[x + 1 + (z + 0) * pw]) 
                        && c11 == Check(arParent[x + 2 + (z + 1) * pw]) 
                        && c11 == Check(arParent[x + 1 + (z + 2) * pw])) ? idx : idRiver;
                }
            }

            return ar;
        }

        private bool Check(int idx) => idx == idConiferousForest || idx == idSwamp;
    }
}
