using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkClient.Util;
using MvkServer;
using MvkServer.Glm;
using MvkServer.Util;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Gui
{
    /// <summary>
    /// Объект визуализации чата на экране в ScreenInGame
    /// </summary>
    public class GuiChatList
    {
        /// <summary>
        /// Все строки чата разбитые под контейнер чата
        /// </summary>
        public List<string> MessagesChat { get; private set; } = new List<string>();
        /// <summary>
        /// Все строки чата разбитые под контейнер консоля
        /// </summary>
        public List<string> MessagesConsole { get; private set; } = new List<string>();

        /// <summary>
        /// Список сообщений, ранее отправленных через графический интерфейс чата
        /// </summary>
        public List<string> SentMessagesChat { get; private set; } = new List<string>();
        /// <summary>
        /// Список сообщений, ранее отправленных через графический интерфейс консоля
        /// </summary>
        public List<string> SentMessagesConsole { get; private set; } = new List<string>();
        /// <summary>
        /// Строки чата, которые будут отображаться в окне чата
        /// </summary>
        private List<ChatLine> chatLines = new List<ChatLine>();

        private readonly TransferText transfer;

        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        private readonly Client client;
        /// <summary>
        /// Объект скрина во время игры
        /// </summary>
        private readonly ScreenInGame inGame;
        /// <summary>
        /// Нужен свой счётчик времени, чтоб чат не слетал если на сервере была корректировка по времени
        /// </summary>
        private uint tickCounter = 0;

        public GuiChatList(ScreenInGame inGame)
        {
            this.inGame = inGame;
            client = inGame.ClientMain;
            transfer = new TransferText(FontSize.Font12, 496);
            //AddMessage("Test");
            //AddMessage("§nПроверка кирилицы очень длинной строки, §l§oонда должна быть очень длинная, чтоб можно было проверить примерно в три переноса, так будет наверно правильно");
            //AddMessage("§l§bAnt§r: типа Ант пишет.!");
            //AddMessage("Игрок получил §6ожёг§f!", true);
            //AddMessage("Игрок получил §9ожёг!", true);
            //AddMessage("§e§lPatientZero§f: На этом этапе ландшафт уже выглядит знакомым, но ему недостаёт вариативности в отдельных биомах. Чтобы исправить это, в данном слое имеется небольшая вероятность превращения биома в его «холмистый» вариант. Например, он может превратить пустыню в пустынные холмы, лес в лесистые холмы, а саванну в плато.");
            //AddMessage("§d§mКак применять §nдубовые кубики: §o§6Кубики добавляются в сосуд с дистиллятом в соотношении 2-4 г/литр напитка. Желаемая крепость напитка – 45-55°. Выдержку рекомендуется производить в течение 1-3 месяцев, за это время кубики успеют облагородить дистиллят и придать ему приятный вкус.");
            //AddMessage("§bAnt§f: §nДолжна быть 17-ая строка для теста!");
            //AddMessage("§bAnt§r: §kДолжна быть 17-ая строка для теста!");
        }

        public void Tick() => tickCounter++; 

        /// <summary>
        ///  Для истории написания, чтоб можно было повторно повторить текст
        /// </summary>
        public void AddMessageHistory(string message, bool isConsole = false)
        {
            List<string> vs = isConsole ? SentMessagesConsole : SentMessagesChat;
            int count = vs.Count;
            if (count == 0 || !vs[count - 1].Equals(message))
            {
                vs.Add(message);
            }
        }

        /// <summary>
        /// Занести в массив сообщение
        /// </summary>
        public void AddMessage(string message, bool isConsole = false)
        {
            if (message != "")
            {
                message = ChatStyle.Reset + message;
                // Занести в массив чата
                List<string> vs = isConsole ? MessagesConsole : MessagesChat;
                chatLines.Add(new ChatLine(message, tickCounter));
                transfer.Run(message);
                int nl = transfer.NumberLines;
                if (nl == 1)
                {
                    vs.Add(message);
                }
                else
                {
                    string[] strs = transfer.OutText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    FontRenderer.ResetFont();
                    message = "";
                    string code = "";
                    for (int i = 0; i < nl; i++)
                    {
                        message = strs[i];
                        if (message != "")
                        {
                            vs.Add(code + message);
                            code = FontRenderer.MessageToCodeFont(message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Прорисовать строки чата в окне InGame
        /// </summary>
        public void DrawInGame()
        {
            int count = chatLines.Count;
            if (count > 0)
            {
                uint time = tickCounter;
                int sizeInterface = inGame.SizeInterface;
                FontSize fontSize = FontSize.Font12;
                int sizeVert = FontAdvance.VertAdvance[(int)fontSize] + 4;
                GLRender.PushMatrix();
                GLRender.Translate(0, inGame.Height, 0);
                GLRender.Scale(sizeInterface, sizeInterface, 1);
                GLRender.Translate(8, -80, 0);

                int i, width, nl, y2, y1;
                string message;
                y1 = 0;
                TransferText transfer = new TransferText(fontSize, inGame.Width / 2, sizeInterface);
                count--;

                for (i = count; i >= 0; i--)
                {
                    if (time - chatLines[i].timeCreated > MvkGlobal.CHAT_LINE_TIME_LIFE)
                    {
                        // Если поподается строка старая, далее уже не расматриваем
                        break;
                    }

                    message = chatLines[i].message;

                    transfer.Run(message);
                    message = transfer.OutText;
                    width = transfer.OutWidth - 8;
                    nl = transfer.NumberLines;
                    y1 -= nl * sizeVert;
                    y2 = y1 + nl * sizeVert;

                    GLRender.Texture2DDisable();
                    GLRender.Rectangle(0, y1, width, y2, new vec4(0, 0, 0, .3f));
                    // Создадим эффект затухания в конце строки, фон
                    GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                    GLRender.Color(0, 0, 0, .3f);
                    GLRender.Vertex(width, y2);
                    GLRender.Color(0, 0, 0, 0);
                    GLRender.Vertex(width + 40, y2);
                    GLRender.Color(0, 0, 0, .3f);
                    GLRender.Vertex(width, y1);
                    GLRender.Color(0, 0, 0, 0);
                    GLRender.Vertex(width + 40, y1);
                    GLRender.End();

                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                    GLRender.Texture2DEnable();
                    FontRenderer.RenderText(4, y1 + 2, message, fontSize, new vec3(1), 1, true, .2f, 1);
                }

                GLRender.PopMatrix();
            }
        }
    }
}
