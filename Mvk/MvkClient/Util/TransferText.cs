using MvkAssets;
using MvkClient.Renderer.Font;
using System;
using System.Collections.Generic;

namespace MvkClient.Util
{
    /// <summary>
    /// Класс разбития длинный строки на переносы
    /// </summary>
    public class TransferText
    {
        /// <summary>
        /// Размер шрифта
        /// </summary>
        private readonly FontSize size = FontSize.Font12;
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        private readonly int sizeInterface;
        /// <summary>
        /// Задаваемая ширина
        /// </summary>
        private readonly int inWidth;
        /// <summary>
        /// Задаваемый текст
        /// </summary>
        private string inText;

        /// <summary>
        /// Результат максимальной ширины строки
        /// </summary>
        public int OutWidth { get; private set; }
        /// <summary>
        /// Результат текста
        /// </summary>
        public string OutText { get; private set; }
        /// <summary>
        /// Количество строк
        /// </summary>
        public int NumberLines { get; private set; }

        public TransferText(FontSize size, int width, int sizeInterface = 1)
        {
            this.sizeInterface = sizeInterface;
            this.size = size;
            inWidth = width * sizeInterface;
        }

        public void Run(string text)
        {
            inText = text;
            Run();
        }

        private void Run()
        {
            OutWidth = 0;
            NumberLines = 1;
            string[] strs = inText.Split(new string[] { "\r\n", "§u" }, StringSplitOptions.None);

            int wspase = FontRenderer.WidthString(" ", size) * sizeInterface;
            int w;
            string text = "";
            string[] symbols;
            bool first = true;
            foreach (string str in strs)
            {
                symbols = str.Split(' ');
                w = 0;
                if (!first) text += "\r\n"; else first = false;

                foreach (string symbol in symbols)
                {
                    int ws = FontRenderer.WidthString(symbol, size, false) * sizeInterface;
                    if (w + wspase + ws > inWidth)
                    {
                        if (w > OutWidth) OutWidth = w;
                        NumberLines++;
                        w = ws;
                        text += "\r\n" + symbol;
                    }
                    else
                    {
                        if (w > 0) text += " ";
                        text += symbol;
                        w += wspase + ws;
                    }
                }
                if (w > OutWidth) OutWidth = w;
            }
            OutText = text;
            OutWidth /= sizeInterface;
        }
    }
}
