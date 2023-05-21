using MvkClient.Util;
using System.Collections.Generic;

namespace MvkClient.Renderer.Font
{
    /// <summary>
    /// Объект хранит все глифы
    /// </summary>
    public class FontAdvance
    {
        /// <summary>
        /// Горизонтальное смещение начала следующего глифа
        /// </summary>
        public static int[] HoriAdvance { get; private set; } = new int[] { 8, 12, 16 };
        /// <summary>
        /// Вертикальное смещение начала следующего глифа 
        /// </summary>
        public static int[] VertAdvance { get; private set; } = new int[] { 8, 12, 16 };

        /// <summary>
        /// Массив символов
        /// </summary>
        private static readonly Dictionary<char, Symbol>[] items = new Dictionary<char, Symbol>[3];

        /// <summary>
        /// Инициализировать шрифты
        /// </summary>
        public static void Initialize(BufferedImage textureFont8, BufferedImage textureFont12, BufferedImage textureFont16)
        {
            InitializeFontX(textureFont8, 0);
            InitializeFontX(textureFont12, 1);
            InitializeFontX(textureFont16, 2);
        }

        private static void InitializeFontX(BufferedImage textureFont, int size)
        {
            HoriAdvance[size] = textureFont.Width >> 4;
            VertAdvance[size] = textureFont.Height >> 4;

            items[size] = new Dictionary<char, Symbol>();
            char[] vc = Symbol.arrayKey;
            for (int i = 0; i < vc.Length; i++)
            {
                Symbol symbol = new Symbol(vc[i], size);
                symbol.Initialize(textureFont);
                if (!items[size].ContainsKey(vc[i])) items[size].Add(symbol.Symb, symbol);
            }
        }

        /// <summary>
        /// Получить объект символа
        /// </summary>
        public static Symbol Get(char key, int size) => items[size].ContainsKey(key) ? items[size][key] : null;
    }
}
