using MvkAssets;
using MvkServer.Glm;
using MvkServer.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект изображения
    /// </summary>
    public class BufferedImage
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высотп
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Массив байт
        /// </summary>
        public byte[] Buffer { get; private set; }
        /// <summary>
        /// Ключ текстуры
        /// </summary>
        public AssetsTexture Key { get; private set; }

        public BufImage[] Images { get; private set; } = new BufImage[0];

        public BufferedImage(AssetsTexture key, Bitmap bitmap, bool mipmap = false)
        {
            Key = key;
            Width = bitmap.Width;
            Height = bitmap.Height;
            Buffer = BufImage.BitmapToByteArray(bitmap);
            if (mipmap) MipMap(Buffer, new List<BufImage>());
        }

        /// <summary>
        /// Получить цвет пикселя
        /// </summary>
        public vec4 GetPixel(int x, int y)
        {
            int index = y * Height * 4 + x * 4;
            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];
            byte a = Buffer[index + 3];
            return new vec4(Bf(r), Bf(g), Bf(b), Bf(a));
        }

        private float Bf(byte c) => (float)c / 255f;

        /// <summary>
        /// Создание уровней MipMap
        /// </summary>
        private void MipMap(Bitmap bitmap, List<BufImage> list)
        {
            if (bitmap.Width > 64)
            {
                int w = bitmap.Width;
                int h = bitmap.Height;
                Bitmap bm = new Bitmap(w / 2, h / 2);

                int r, g, b, a, c, x, y, x1, y1;
                Color color;

                for (x = 0; x < w; x+=2)
                {
                    for (y = 0; y < h; y+=2)
                    {
                        r = g = b = a = c = 0;

                        for (x1 = 0; x1 < 2; x1++)
                        {
                            for (y1 = 0; y1 < 2; y1++)
                            {
                                color = bitmap.GetPixel(x + x1, y + y1);
                                if (color.A > 0)
                                {
                                    r += color.R;
                                    g += color.G;
                                    b += color.B;
                                    a += color.A;
                                    c++;
                                }
                            }
                        }
                        if (c > 0)
                        {
                            bm.SetPixel(x / 2, y / 2, Color.FromArgb(a / c, r / c, g / c, b / c));
                        }
                    }
                }
                list.Add(new BufImage(bm));
                //bm.Save("Atlas" + list.Count + ".png", ImageFormat.Png);
                MipMap(bm, list);
            }
            else
            {
                Images = list.ToArray();
            }
        }

        /// <summary>
        /// Создание уровней MipMap
        /// </summary>
        private void MipMap(byte[] buffer, List<BufImage> list)
        {
            if (buffer.Length > 16384) // 64 * 64
            {
                int w = (int)Mth.Sqrt(buffer.Length / 4);
                int w2 = w / 2;
                byte[] buf = new byte[w * w];
                int r, g, b, a, a1, c, x, y, x1, y1, index;

                for (x = 0; x < w; x += 2)
                {
                    for (y = 0; y < w; y += 2)
                    {
                        r = g = b = a = c = 0;
                        for (x1 = 0; x1 < 2; x1++)
                        {
                            for (y1 = 0; y1 < 2; y1++)
                            {
                                index = ((y + y1) * w + x + x1) * 4;
                                a1 = buffer[index + 3];
                                if (a1 > 0)
                                {
                                    r += buffer[index + 0];
                                    g += buffer[index + 1];
                                    b += buffer[index + 2];
                                    a += a1;
                                    c++;
                                }
                            }
                        }
                        index = ((y / 2) * w2 + x / 2) * 4;
                        if (c > 0)
                        {
                            buf[index + 0] = (byte)(r / c);
                            buf[index + 1] = (byte)(g / c);
                            buf[index + 2] = (byte)(b / c);
                            buf[index + 3] = (byte)(a / c);
                        }
                    }
                }
                list.Add(new BufImage(buf, w2));
                MipMap(buf, list);
            }
            else
            {
                Images = list.ToArray();
            }
        }
    }
}
