using MvkAssets;
using MvkServer.Glm;
using System;

namespace MvkClient.Renderer.Font
{
    public class FontRenderer
    {
        /// <summary>
        /// Перечень 16-ти цветов как в майне
        /// </summary>
        public static vec3[] ColorCode = new vec3[]
        {
            new vec3(0),
            new vec3(0, 0, .67f),
            new vec3(0, .67f, 0),
            new vec3(0, .67f, .67f),
            new vec3(.67f, 0, 0),
            new vec3(.67f, 0, .67f),
            new vec3(1, .67f, 0),
            new vec3(.67f),
            new vec3(.33f),
            new vec3(.33f, .33f, 1),
            new vec3(.33f, 1, .33f),
            new vec3(.33f, 1, 1),
            new vec3(1, .33f, .33f),
            new vec3(1, .33f, 1),
            new vec3(1, 1, .33f),
            new vec3(1)
        };

        /// <summary>
        /// Перечень 16-ти цветов тени как в майне
        /// </summary>
        public static vec3[] ColorBgCode = new vec3[]
        {
            new vec3(0),
            new vec3(0, 0, .16f),
            new vec3(0, .16f, 0),
            new vec3(0, .16f, .16f),
            new vec3(.16f, 0, 0),
            new vec3(.16f, 0, .16f),
            new vec3(.16f, .16f, 0),
            new vec3(.16f),
            new vec3(.08f),
            new vec3(.08f, .08f, .25f),
            new vec3(.08f, .25f, .08f),
            new vec3(.08f, .25f, .25f),
            new vec3(.25f, .08f, .08f),
            new vec3(.25f, .08f, .25f),
            new vec3(.25f, .25f, .08f),
            new vec3(.25f)
        };

        /// <summary>
        /// Перечень команд цвета
        /// </summary>
        public static string[] CodeColors = new string[]
        {
            "§0", "§1", "§2", "§3", "§4", "§5", "§6", "§7", "§8", "§9", "§a", "§b", "§c", "§d", "§e", "§f"
        };

        /// <summary>
        /// Цвет шрифта §0..f
        /// </summary>
        private static int styleColor = -1;
        /// <summary>
        /// Выделен ли шрифт §l
        /// </summary>
        private static bool styleBolb = false;
        /// <summary>
        /// Наклонный шрифт §o
        /// </summary>
        private static bool styleItalic = false;
        /// <summary>
        /// Подчеркнутый шрифт §n
        /// </summary>
        private static bool styleUnderline = false;
        /// <summary>
        /// Зачеркнутый шрифт §m
        /// </summary>
        private static bool styleStrikethrough = false;

        /// <summary>
        /// Прорисовка символа
        /// </summary>
        private static int RenderChar(char letter, int size)
        {
            Symbol symbol = FontAdvance.Get(letter, size);
            if (symbol == null) return 0;
            if (styleItalic) symbol.DrawItalic();
            else symbol.Draw();
            return symbol.Width;
        }

        /// <summary>
        /// Узнать ширину символа
        /// </summary>
        private static int WidthChar(char letter, int size)
        {
            Symbol symbol = FontAdvance.Get(letter, size);
            if (symbol == null) return 0;
            return symbol.Width;
        }

        /// <summary>
        /// Сбросить параметры цвета
        /// </summary>
        public static void ResetFont()
        {
            styleBolb = false;
            styleItalic = false;
            styleStrikethrough = false;
            styleUnderline = false;
            styleColor = -1;
        }

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public static void RenderText(float x, float y, string text, FontSize size, vec3 color, float alpha = 1f, bool isShadow = false, float background = 0, int biasX = 2)
        {
            ResetFont();
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);
            int h = 0;
            int sizeVert = FontAdvance.VertAdvance[(int)size];

            foreach (string str in strs)
            {
                RenderString(x, y + h, str, size, color, alpha, isShadow, background, biasX);
                h += sizeVert + 4;
            }
        }

        /// <summary>
        /// Прорисовка строки, если надо то с тенью
        /// </summary>
        public static void RenderString(float x, float y, string text, FontSize size, vec3 color, float alpha = 1f, bool isShadow = false, float background = 0, int biasX = 2)
        {
            if (isShadow) RenderStringOne(x + biasX, y + 1, text, size, new vec3(background), alpha, true);
            RenderStringOne(x, y, text, size, color, alpha, false);
        }

        /// <summary>
        /// Прорисовка одной строки
        /// </summary>
        private static void RenderStringOne(float x, float y, string text, FontSize size, vec3 color, float alpha, bool isShadow)
        {
            char[] vc = text.ToCharArray();
            int count = vc.Length;
            if (count > 0)
            {
                int i, h, w0;
                int w = 0;
                int sizeInt = (int)size;
                int stepFontInt = Assets.StepFont(size);
                char symbol;

                vec3 colorDraw = styleColor == -1 ? color : isShadow ? ColorBgCode[styleColor] : ColorCode[styleColor];
                GLRender.Color(colorDraw, alpha);

                for (i = 0; i < count; i++)
                {
                    symbol = vc[i];
                    if (symbol == 167 && i + 1 < count)
                    {
                        // Этап определения стиля шрифта
                        symbol = vc[i + 1];
                        if (symbol == 'r')
                        {
                            // Ресет
                            ResetFont();
                            colorDraw = color;
                            GLRender.Color(colorDraw, alpha);
                        }
                        // Жирный
                        else if (symbol == 'l') styleBolb = true;
                        // Наклонный
                        else if (symbol == 'o') styleItalic = true;
                        // Подчеркнутый
                        else if (symbol == 'n') styleUnderline = true;
                        // Зачеркнутый
                        else if (symbol == 'm') styleStrikethrough = true;
                        else
                        {
                            // Цвет
                            int key = "0123456789abcdef".IndexOf(symbol);
                            if (key >= 0 && key < 17)
                            {
                                styleColor = key;
                                colorDraw = isShadow ? ColorBgCode[styleColor] : ColorCode[styleColor]; 
                                GLRender.Color(colorDraw, alpha);
                            }
                            else
                            {
                                ResetFont();
                            }
                        }
                        i++;
                    }
                    else
                    {
                        // Этап прорисовки символа
                        GLRender.PushMatrix();
                        GLRender.Translate(x + w, y, 0);
                        w0 = RenderChar(symbol, sizeInt);
                        if (w0 > 0)
                        {
                            if (styleBolb)
                            {
                                // Если полужирный то дублируем символ со смещением в один пиксел
                                GLRender.Translate(1, 0, 0);
                                RenderChar(symbol, sizeInt);
                                w0++;
                            }
                            w0 += stepFontInt;
                        }
                        GLRender.PopMatrix();

                        if (styleStrikethrough || styleUnderline)
                        {
                            // Если имеется зачёркивание или подчёркивание
                            GLRender.PushMatrix();
                            GLRender.Texture2DDisable();
                            if (!isShadow) GLRender.Color(colorDraw, alpha);
                            GLRender.Translate(x + w, y + 1, 0);
                            h = FontAdvance.VertAdvance[sizeInt];
                            if (styleUnderline) GLRender.Rectangle(0, h - 1, w0, h);
                            h = h / 2;
                            if (styleStrikethrough) GLRender.Rectangle(0, h - 1, w0, h);
                            GLRender.Texture2DEnable();
                            GLRender.PopMatrix();
                        }
                        w += w0;
                    }
                }
            }
        }

        /// <summary>
        /// Сгенерировать строку кодов по строке и по кэшу стиля
        /// </summary>
        public static string MessageToCodeFont(string message)
        {
            char[] vc = message.ToCharArray();
            int count = vc.Length;
            int i;
            char symbol;
            for (i = 0; i < count; i++)
            {
                symbol = vc[i];
                if (symbol == 167 && i + 1 < count)
                {
                    symbol = vc[i + 1];
                    // Ресет
                    if (symbol == 'r') ResetFont();
                    // Жирный
                    else if (symbol == 'l') styleBolb = true;
                    // Наклонный
                    else if (symbol == 'o') styleItalic = true;
                    // Подчеркнутый
                    else if (symbol == 'n') styleUnderline = true;
                    // Зачеркнутый
                    else if (symbol == 'm') styleStrikethrough = true;
                    else
                    {
                        // Цвет
                        int key = "0123456789abcdef".IndexOf(symbol);
                        if (key >= 0 && key < 17)
                        {
                            styleColor = key;
                        }
                    }
                    i++;
                }
            }

            string code = "";
            if (styleColor != -1) code += CodeColors[styleColor];
            if (styleBolb) code += "§l";
            if (styleItalic) code += "§o";
            if (styleUnderline) code += "§n";
            if (styleStrikethrough) code += "§m";
            return code;
        }
       

        /// <summary>
        /// Узнать ширину текста
        /// </summary>
        public static int WidthString(string text, FontSize size, bool isReset = true)
        {
            if (isReset) ResetFont();

            char[] vc = text.ToCharArray();
            int count = vc.Length;
            int i, w0;
            int w = 0;
            int sizeInt = (int)size;
            int stepFont = Assets.StepFont(size);
            int stepBolb = styleBolb ? 1 : 0;
            char symbol;
            for (i = 0; i < count; i++)
            {
                symbol = vc[i];
                if (symbol == 167 && i + 1 < count)
                {
                    symbol = vc[i + 1];
                    if (symbol == 'l')
                    {
                        styleBolb = true;
                        stepBolb = 1;
                    }
                    else if (symbol == 'r')
                    {
                        styleBolb = false;
                        stepBolb = 0;
                    }
                    i++;
                }
                else
                {
                    w0 = WidthChar(symbol, sizeInt);
                    if (w0 > 0) w += w0 + stepFont + stepBolb;
                }
            }
            return w;
        }

        /// <summary>
        /// Получить строку с конца по заданной ширине
        /// </summary>
        public static string GetStringEndToWidth(string text, int width, FontSize size)
        {
            char[] vc = text.ToCharArray();
            int i, w0;
            int w = 0;
            int sizeInt = (int)size;
            int stepFont = Assets.StepFont(size);
            int count = vc.Length;
            count--;
            for (i = count; i >= 0; i--)
            {
                w0 = WidthChar(vc[i], sizeInt);
                if (w0 > 0)
                {
                    w += w0 + stepFont;
                    if (w > width)
                    {
                        return text.Substring(i + 1);
                    }
                }
            }
            return text;
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        /// <param name="list">ключ листа</param>
        public static void Draw(uint list) => GLRender.ListCall(list);

    }
}
