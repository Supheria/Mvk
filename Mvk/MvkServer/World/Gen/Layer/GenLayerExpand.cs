namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Объект который расширяет
    /// </summary>
    public class GenLayerExpand : GenLayer
    {
        private readonly int id;

        public GenLayerExpand(GenLayer parent, int id)
        {
            this.parent = parent;
            this.id = id;
        }

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

                    if (idx != id && (arParent[x + 0 + (z + 1) * pw] == id
                        || arParent[x + 1 + (z + 0) * pw] == id
                        || arParent[x + 2 + (z + 1) * pw] == id
                        || arParent[x + 1 + (z + 2) * pw] == id))
                    {
                        ar[x + z * width] = id;
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
