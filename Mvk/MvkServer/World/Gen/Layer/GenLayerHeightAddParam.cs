using MvkServer.Util;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Абстрактный класс по добавлении высот
    /// </summary>
    public abstract class GenLayerHeightAddParam : GenLayer
    {
        public GenLayerHeightAddParam(long baseSeed, GenLayer parent) : base(baseSeed) => this.parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arParent = parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];
            int x, z, idx;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = x + z * width;
                    InitChunkSeed(x + areaX, z + areaZ);
                    ar[idx] = GetParam(arParent[idx]);
                }
            }
            return ar;
        }

        protected virtual int GetParam(int param) => param;
    }
}
