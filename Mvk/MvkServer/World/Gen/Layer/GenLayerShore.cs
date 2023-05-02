using MvkServer.World.Biome;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Создание берега
    /// </summary>
    public class GenLayerShore : GenLayer
    {
        private readonly int idPlain = (int)EnumBiome.Plain;
        private readonly int idSea = (int)EnumBiome.Sea;
        private readonly int idBeach = (int)EnumBiome.Beach;

        public GenLayerShore(GenLayer parent) => this.parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, idx;
            bool c01, c21, c10, c12;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = arParent[x + 1 + (z + 1) * pw];
                    if (idx == idPlain)
                    {
                        c01 = arParent[x + 0 + (z + 1) * pw] == idSea;
                        c21 = arParent[x + 2 + (z + 1) * pw] == idSea;
                        c10 = arParent[x + 1 + (z + 0) * pw] == idSea;
                        c12 = arParent[x + 1 + (z + 2) * pw] == idSea;

                        ar[x + z * width] = (c01 || c10 || c21 || c12) ? idBeach : idx;
                    }
                    else if (idx == idSea)
                    {
                        c01 = arParent[x + 0 + (z + 1) * pw] == idPlain;
                        c21 = arParent[x + 2 + (z + 1) * pw] == idPlain;
                        c10 = arParent[x + 1 + (z + 0) * pw] == idPlain;
                        c12 = arParent[x + 1 + (z + 2) * pw] == idPlain;

                        ar[x + z * width] = (c01 || c10 || c21 || c12) ? idBeach : idx;
                    }
                    else
                    {
                        ar[x + z * width] = idx;
                    }
                }
            }

            return ar;
        }
    }
}
