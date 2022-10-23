using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект изображения
    /// </summary>
    public struct BufImage
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public readonly int width;
        /// <summary>
        /// Высотп
        /// </summary>
        public readonly int height;
        /// <summary>
        /// Массив байт
        /// </summary>
        public readonly byte[] buffer;

        public BufImage(Bitmap bitmap)
        {
            width = bitmap.Width;
            height = bitmap.Height;
            buffer = BitmapToByteArray(bitmap);
        }

        public BufImage(byte[] buffer, int size)
        {
            width = height = size;
            this.buffer = buffer;
        }

        /// <summary>
        /// Конвертация из Bitmap в объект BufferedImage
        /// </summary>
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }
    }
}
