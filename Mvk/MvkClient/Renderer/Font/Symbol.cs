using MvkClient.Util;
using MvkServer.Glm;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Renderer.Font
{
    /// <summary>
    /// Объект символа
    /// </summary>
    public class Symbol
    {
        #region static
        /// <summary>
        /// Массив символов
        /// </summary>
        public static char[] arrayKey;
        /// <summary>
        /// Строка символов
        /// </summary>
        private static string key = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзиклмнопрстуфхцчшщъыьэюяЁёЙйІіЎў";
        /// <summary>
        /// Массив для проверки символа
        /// </summary>
        private static bool[] checks;

        public static void Initialized()
        {
            arrayKey = key.ToCharArray();
            List<int> ar = new List<int>();
            int max = 0;
            char[] vc = key.ToCharArray();
            int i, id;
            for (i = 0; i < vc.Length; i++)
            {
                id = Convert.ToInt32(vc[i]);
                if (id > max) max = id;
                ar.Add(id);
            }
            checks = new bool[max + 1];
            for (i = 0; i < ar.Count; i++)
            {
                checks[ar[i]] = true;
            }
        }

        /// <summary>
        /// Проверить, присутствует ли такой символ
        /// </summary>
        /// <param name="key">id символа</param>
        public static bool IsPresent(int key) => key < checks.Length ? checks[key] : false;

        #endregion

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Символ
        /// </summary>
        public char Symb { get; private set; }
        /// <summary>
        /// Ширина символа
        /// </summary>
        public int Width { get; private set; } = 4;

        /// <summary>
        /// Индекс листа символа
        /// </summary>
        private uint dList;

        public Symbol(char c, int size)
        {
            Symb = c;
            Size = size;
        }

        public void Initialize(BufferedImage bi)
        {
            int index = key.IndexOf(Symb) + 32;
            if (index == -1) return;

            float u1 = (index & 15) * 0.0625f;
            float u2 = u1 + 0.0625f;
            float v1 = (index >> 4) * 0.0625f;
            float v2 = v1 + 0.0625f;

            GetWidth(bi, index);
            dList = GLRender.ListBegin();
            GLRender.Rectangle(0, 0, FontAdvance.HoriAdvance[Size], FontAdvance.VertAdvance[Size], u1, v1, u2, v2);
            GLRender.ListEnd();
        }

        /// <summary>
        /// Прорисовка символа
        /// </summary>
        public void Draw() => GLRender.ListCall(dList);

        /// <summary>
        /// Прорисовка наклонного символа
        /// </summary>
        public void DrawItalic()
        {
            int index = key.IndexOf(Symb) + 32;
            if (index == -1) return;

            float u1 = (index & 15) * 0.0625f;
            float u2 = u1 + 0.0625f;
            float v1 = (index >> 4) * 0.0625f;
            float v2 = v1 + 0.0625f;

            int x2 = FontAdvance.HoriAdvance[Size];
            int y2 = FontAdvance.VertAdvance[Size];

            GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
            GLRender.TexCoord(u1, v2);
            GLRender.Vertex(-1.6f, y2);
            GLRender.TexCoord(u2, v2);
            GLRender.Vertex(x2 - 1.6f, y2);
            GLRender.TexCoord(u1, v1);
            GLRender.Vertex(1.6f, 0);
            GLRender.TexCoord(u2, v1);
            GLRender.Vertex(x2 + 1.6f, 0);
            GLRender.End();
        }

        /// <summary>
        /// Получить ширину символа
        /// </summary>
        private void GetWidth(BufferedImage bi, int index)
        {
            int advance = FontAdvance.HoriAdvance[Size];

            int x0 = (index & 15) * advance;
            int y0 = (index >> 4) * advance;
            int x1 = x0 + advance - 1;
            int y1 = y0 + advance;

            for (int x = x1; x >= x0; x--)
            {
                for (int y = y0; y < y1; y++)
                {
                    vec4 col = bi.GetPixel(x, y);
                    if (col.w > 0)
                    {
                        Width = x - x0 + 1;
                        return;
                    }
                }
            }
        }

        
    }
}
