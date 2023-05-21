using MvkServer.World.Biome;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Создание берега
    /// </summary>
    public class GenLayerShore : GenLayer
    {
        private readonly int idPlain = (int)EnumBiome.Plain;
        private readonly int idDesert = (int)EnumBiome.Desert;
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

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = arParent[x + 1 + (z + 1) * pw];
                    if (idx == idPlain || idx == idDesert)
                    {
                        ar[x + z * width] = arParent[x + 0 + (z + 1) * pw] == idSea
                            || arParent[x + 2 + (z + 1) * pw] == idSea
                            || arParent[x + 1 + (z + 0) * pw] == idSea
                            || arParent[x + 1 + (z + 2) * pw] == idSea ? idBeach : idx;
                    }
                    else if (idx == idSea)
                    {
                        ar[x + z * width] = Check(arParent[x + 0 + (z + 1) * pw])
                            || Check(arParent[x + 2 + (z + 1) * pw])
                            || Check(arParent[x + 1 + (z + 0) * pw])
                            || Check(arParent[x + 1 + (z + 2) * pw]) ? idBeach : idx;
                    }
                    else
                    {
                        ar[x + z * width] = idx;
                    }
                }
            }
            return ar;
        }

        private bool Check(int idx) => idx == idPlain || idx == idDesert;
    }
}
