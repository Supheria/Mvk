using MvkAssets;
using MvkServer.Glm;
using System.Drawing;

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

        public BufferedImage(AssetsTexture key, Bitmap bitmap)
        {
            Key = key;
            Width = bitmap.Width;
            Height = bitmap.Height;
            Buffer = BufImage.BitmapToByteArray(bitmap);
        }

        public void SetImages(BufImage[] images) => Images = images;

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
    }
}
