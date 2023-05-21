namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Эфект гладкости, смешать соседние в средний, для высот рельефа
    /// </summary>
    public class GenLayerSmoothMix : GenLayer
    {
        public GenLayerSmoothMix(GenLayer parent) => this.parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, c;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    c = arParent[x + 1 + (z + 1) * pw];
                    c += arParent[x + 0 + (z + 1) * pw];
                    c += arParent[x + 2 + (z + 1) * pw];
                    c += arParent[x + 1 + (z + 0) * pw];
                    c += arParent[x + 1 + (z + 2) * pw];

                    //c += arParent[x + 0 + (z + 0) * pw];
                    //c += arParent[x + 0 + (z + 2) * pw];
                    //c += arParent[x + 2 + (z + 0) * pw];
                    //c += arParent[x + 2 + (z + 2) * pw];

                    ar[x + z * width] = c / 5;
                }
            }

            return ar;
        }
    }
}
