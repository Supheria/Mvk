using MvkServer.Util;

namespace MvkServer.Gen
{
    /// <summary>
    /// Объект обработки шума Перлина одной актавы
    /// </summary>
    public class NoiseGeneratorSimplex
    {
        private int[] permutations;
        public float xCoord;
        public float yCoord;
        public float zCoord;
        private static readonly float[] arStat1 = new float[] { 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f };
        private static readonly float[] arStat2 = new float[] { 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f };
        private static readonly float[] arStat3 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, -1.0f };
        private static readonly float[] arStat4 = new float[] { 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f };
        private static readonly float[] arStat5 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, -1.0f };

        public NoiseGeneratorSimplex() => Generator(new Rand());
        public NoiseGeneratorSimplex(Rand random) => Generator(random);

        protected void Generator(Rand random)
        {
            permutations = new int[512];
            xCoord = random.NextFloat() * 256f;
            yCoord = random.NextFloat() * 256f;
            zCoord = random.NextFloat() * 256f;
            int i;

            for (i = 0; i < 256; permutations[i] = i++) ;

            for (i = 0; i < 256; i++)
            {
                int r = random.Next(256 - i) + i;
                int old = permutations[i];
                permutations[i] = permutations[r];
                permutations[r] = old;
                permutations[i + 256] = permutations[i];
            }
        }

        public float Lerp(float f1, float f2, float f3) => f2 + f1 * (f3 - f2);

        /// <summary>
        /// Умнажаем первые значения с массива и суммируем их
        /// </summary>
        public float Multiply(int index, float x, float y)
        {
            int i = index & 15;
            return arStat4[i] * x + arStat5[i] * y;
        }

        public float Grad(int index, float x, float y, float z)
        {
            int i = index & 15;
            return arStat1[i] * x + arStat2[i] * y + arStat3[i] * z;
        }

        /// <summary>
        /// Генерация шума объёма в массив
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="yOffset">координата Y</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="ySize">ширина по Y</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="yScale">масштаб по Y</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <param name="noiseScale">масштаб шума</param>
        public void PopulateNoiseArray3d(float[] noiseArray, float xOffset, float yOffset, float zOffset,
            int xSize, int ySize, int zSize, float xScale, float yScale, float zScale, float noiseScale)
        {
            float noise = 1.0f / noiseScale;
            int count = 0;

            int x, y, z, xi, xa, zi, za, yi, ya, p1, p2, p3, p4, p5, p6;
            float xd, yd, zd, xr, yr, zr, ler5, ler6, ler7, f1, f2, f3;

            float ler1 = 0.0f;
            float ler2 = 0.0f;
            float ler3 = 0.0f;
            float ler4 = 0.0f;

            for (x = 0; x < xSize; x++)
            {
                xd = xOffset + (float)x * xScale + xCoord;
                xi = (int)xd;
                if (xd < (float)xi) xi--;
                xa = xi & 255;
                xd -= (float)xi;
                xr = xd * xd * xd * (xd * (xd * 6.0f - 15.0f) + 10.0f);

                for (z = 0; z < zSize; z++)
                {
                    zd = zOffset + (float)z * zScale + zCoord;
                    zi = (int)zd;
                    if (zd < (float)zi) zi--;
                    za = zi & 255;
                    zd -= (float)zi;
                    zr = zd * zd * zd * (zd * (zd * 6.0f - 15.0f) + 10.0f);

                    for (y = 0; y < ySize; ++y)
                    {
                        yd = yOffset + (float)y * yScale + yCoord;
                        yi = (int)yd;
                        if (yd < (float)yi) yi--;
                        ya = yi & 255;
                        yd -= (float)yi;
                        yr = yd * yd * yd * (yd * (yd * 6.0f - 15.0f) + 10.0f);

                        if (y == 0 || ya >= 0)
                        {
                            p1 = permutations[xa] + ya;
                            p2 = permutations[p1] + za;
                            p3 = permutations[p1 + 1] + za;
                            p4 = permutations[xa + 1] + ya;
                            p5 = permutations[p4] + za;
                            p6 = permutations[p4 + 1] + za;
                            f1 = xr;
                            f2 = Grad(permutations[p2], xd, yd, zd);
                            f3 = Grad(permutations[p5], xd - 1, yd, zd);
                            ler1 = f2 + f1 * (f3 - f2);
                            f2 = Grad(permutations[p3], xd, yd - 1, zd);
                            f3 = Grad(permutations[p6], xd - 1, yd - 1, zd);
                            ler2 = f2 + f1 * (f3 - f2);
                            f2 = Grad(permutations[p2 + 1], xd, yd, zd - 1);
                            f3 = Grad(permutations[p5 + 1], xd - 1, yd, zd - 1);
                            ler3 = f2 + f1 * (f3 - f2);
                            f2 = Grad(permutations[p3 + 1], xd, yd - 1, zd - 1);
                            f3 = Grad(permutations[p6 + 1], xd - 1, yd - 1, zd - 1);
                            ler4 = f2 + f1 * (f3 - f2);
                        }
                        ler5 = ler1 + yr * (ler2 - ler1);
                        ler6 = ler3 + yr * (ler4 - ler3);
                        ler7 = ler5 + zr * (ler6 - ler5);

                        noiseArray[count] += ler7 * noise;
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// Генерация шума плоскости в массиве
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <param name="noiseScale">масштаб шума</param>
        public void PopulateNoiseArray2d(float[] noiseArray, float xOffset, float zOffset,
            int xSize, int zSize, float xScale, float zScale, float noiseScale)
        {
            float noise = 1.0f / noiseScale;
            int count = 0;

            int x, z, xi, xa, zi, za, p1, p2, p3, p4;
            float xd, zd, xr, zr, ler1, ler2, ler3, f1, f2, f3;

            for (x = 0; x < xSize; x++)
            {
                xd = xOffset + (float)x * xScale + xCoord;
                xi = (int)xd;
                if (xd < xi) xi--;
                xa = xi & 255;
                xd -= xi;
                xr = xd * xd * xd * (xd * (xd * 6.0f - 15.0f) + 10.0f);

                for (z = 0; z < zSize; z++)
                {
                    zd = zOffset + (float)z * zScale + zCoord;
                    zi = (int)zd;
                    if (zd < (float)zi) zi--;
                    za = zi & 255;
                    zd -= (float)zi;
                    zr = zd * zd * zd * (zd * (zd * 6.0f - 15.0f) + 10.0f);
                    p1 = permutations[xa] + 0;
                    p2 = permutations[p1] + za;
                    p3 = permutations[xa + 1] + 0;
                    p4 = permutations[p3] + za;
                    f1 = xr;
                    f2 = Multiply(permutations[p2], xd, zd);
                    f3 = Grad(permutations[p4], xd - 1.0f, 0.0f, zd);
                    ler1 = f2 + f1 * (f3 - f2);
                    f2 = Grad(permutations[p2 + 1], xd, 0.0f, zd - 1.0f);
                    f3 = Grad(permutations[p4 + 1], xd - 1.0f, 0.0f, zd - 1.0f);
                    ler2 = f2 + f1 * (f3 - f2);
                    ler3 = ler1 + zr * (ler2 - ler1);
                    noiseArray[count] += ler3 * noise;
                    count++;
                }
            }
        }
    }
}
