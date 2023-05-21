using MvkServer.World.Biome;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// По добавляем дополнительные высоты в определённых биомах
    /// </summary>
    public class GenLayerHeightAddBiome : GenLayer
    {
        private readonly int idBirchForest = (int)EnumBiome.BirchForest;
        private readonly int idMountains = (int)EnumBiome.Mountains;
        private readonly int idSwamp = (int)EnumBiome.Swamp;
        private readonly bool isForest = false;

        private readonly GenLayer layerBiome;

        public GenLayerHeightAddBiome(long baseSeed, GenLayer parent, GenLayer layerBiome, bool isForest) : base(baseSeed)
        {
            this.parent = parent;
            this.layerBiome = layerBiome;
            this.isForest = isForest;
        }

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arBiome = layerBiome.GetInts(areaX, areaZ, width, height);
            int[] arParent = parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];
            int x, z, idx, c, b;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = x + z * width;
                    c = arParent[idx];
                    b = arBiome[idx];

                    if (b == idSwamp)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        c += NextInt(3) - 1;
                    }
                    else if (b == idMountains)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        c += NextInt(9) - 4;
                    }
                    else if (isForest && b == idBirchForest)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        c += NextInt(5) - 2;
                    }
                    ar[idx] = c;
                }
            }
            return ar;
        }
    }
}
