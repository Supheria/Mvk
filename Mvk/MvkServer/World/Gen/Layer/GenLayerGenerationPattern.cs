using MvkServer.Util;
using System;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Генерация из шаблона мира
    /// </summary>
    public class GenLayerGenerationPattern : GenLayer
    {
        /// <summary>
        /// массив стартового шаблона мира
        /// </summary>
        private readonly byte[] arPattern = new byte[1024];

        public GenLayerGenerationPattern(long worldSeed)
        {
            // Генерация шаблоновб где 15, 15 это ноль
            GenPatternWorld genPattern = new GenPatternWorld(worldSeed);
            arPattern = genPattern.GetArray();
        }

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] ar = new int[height * width];
            int x, z, x2, z2;

            try
            {
                for (z = 0; z < height; z++)
                {
                    for (x = 0; x < width; x++)
                    {
                        x2 = areaX + x;
                        z2 = areaZ + z;
                        if (x2 > -15 && x2 < 16 && z2 > -15 && z2 < 16)
                        {
                            ar[z * width + x] = arPattern[(z2 + 15) << 5 | (x2 + 15)];
                        }
                    }
                }

                return ar;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                return new int[0];
            }
        }
    }
}
