using MvkAssets;
using MvkServer.Glm;
using MvkServer.Network.Packets.Client;
using MvkServer.Util;
using System;
using System.Collections.Generic;

namespace MvkClient.Gui
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChat : ScreenWindow
    {
        /// <summary>
        /// Максимальное количество строк в чате
        /// </summary>
        private const int COUNT_MAX_LINE = 16;

        /// <summary>
        /// Строка списка сообщений
        /// </summary>
        private Label labelMessages;
        /// <summary>
        /// Кнопка закрыть окно
        /// </summary>
        private Button buttonClose;
        /// <summary>
        /// Кнопка страница назад
        /// </summary>
        private ButtonBox buttonBack;
        /// <summary>
        /// Кнопка страница вперёд
        /// </summary>
        private ButtonBox buttonNext;
        /// <summary>
        /// Кнопка смены консоль на чат или на оборот
        /// </summary>
        private ButtonBox buttonType;
        /// <summary>
        /// Кнопка отправить сообщение
        /// </summary>
        private Button buttonSend;
        /// <summary>
        /// Контрол написания текста
        /// </summary>
        private TextBox textBoxMessage;
        /// <summary>
        /// Объект визуализации чата на экране
        /// </summary>
        private readonly GuiChatList chatList;

        /// <summary>
        /// Является ли чат консолем
        /// </summary>
        private bool isConsole = false;
        /// <summary>
        /// Позиция скролинга текста
        /// </summary>
        private int scrollPos = 0;
        /// <summary>
        /// Массив списка строк чата или консоля
        /// </summary>
        private List<string> listMessages;
        /// <summary>
        /// Массив списка истории набора текста чата или консоля
        /// </summary>
        private List<string> listSendMessages;

        private string historyBufferChat = "";
        private string historyBufferConsole = "";
        /// <summary>
        /// Сохраняет положение того, какое сообщение чата вы выберете при нажатии вверх
        /// (не увеличивается для дублированных сообщений, отправленных сразу после друг друга)
        /// </summary>
        private int sentHistoryCursor = -1;

        public ScreenChat(Client client, bool isConsole) : base(client)
        {
            alpha = .7f;
            IsFpsMin = false;
            this.isConsole = isConsole;
            chatList = client.World.WorldRender.ScreenGame.PersistantChatGUI;
            background = EnumBackground.Game;
            labelMessages = new Label("", FontSize.Font12) { Alight = EnumAlight.Left, Width = 496 };
            buttonType = new ButtonBox("") { Alpha = alpha };
            buttonType.Click += (sender, e) => TypeChat();
            buttonBack = new ButtonBox("<") { Alpha = alpha };
            buttonBack.Click += (sender, e) => PageBackNext(false);
            buttonNext = new ButtonBox(">") { Alpha = alpha };
            buttonNext.Click += (sender, e) => PageBackNext(true);
            textBoxMessage = new TextBox("", TextBox.EnumRestrictions.All, 200) { Width = 470 };
            textBoxMessage.FixFocus();
            buttonSend = new Button(">>") { Alpha = alpha, Width = 42 };
            buttonSend.Click += ButtonDone_Click;
            buttonClose = new Button(EnumScreenKey.World, "X") { Alpha = alpha, Width = 42 };
            InitButtonClick(buttonClose);
            TypeUpdate();
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public override void KeyDown(int key)
        {
            if (key == 27) // ESC
            {
                ClientMain.Screen.GameMode();
            }
            else if (key == 13) // Enter
            {
                ButtonDone_Click(this, new EventArgs());
            }
            else if (key == 33) // PageUp
            {
                PageBackNext(true);
            }
            else if (key == 34) // PageDown
            {
                PageBackNext(false);
            }
            else if (key == 38) // Up
            {
                SentHistory(-1);
            }
            else if (key == 40) // Down
            {
                SentHistory(1);
            }
        }

        protected override void Init()
        {
            AddControls(textBoxMessage);
            AddControls(buttonSend);
            AddControls(labelMessages);
            AddControls(buttonClose);
            AddControls(buttonBack);
            AddControls(buttonNext);
            AddControls(buttonType);

            UpMessages();
        }
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            position = new vec2i(4 * SizeInterface, Height - (354 + 80) * SizeInterface);
            int w = position.x;
            int h = position.y;

            buttonClose.Position = new vec2i(w + 470 * SizeInterface, h);
            buttonBack.Position = new vec2i(w + 324 * SizeInterface, h);
            buttonNext.Position = new vec2i(w + 358 * SizeInterface, h);
            buttonType.Position = new vec2i(w + 260 * SizeInterface, h);

            labelMessages.Position = new vec2i(w + 8 * SizeInterface, h + 34 * SizeInterface);
            textBoxMessage.Position = new vec2i(w, h + 314 * SizeInterface);
            buttonSend.Position = new vec2i(w + 470 * SizeInterface, textBoxMessage.Position.y);
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public override void MouseWheel(int delta, int x, int y)
        {
            if (delta != 0) PageBackNext(delta > 0);
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            string message = textBoxMessage.Text;
            if (message != "" && ClientMain.World != null)
            {
                chatList.AddMessageHistory(message, isConsole);
                ClientMain.TrancivePacket(isConsole 
                    ? new PacketC14Message(textBoxMessage.Text, ClientMain.Player.MovingObject)
                    : new PacketC14Message(textBoxMessage.Text));
            }
            OnFinished(EnumScreenKey.World);
        }

        private void PageBackNext(bool next)
        {
            int old = scrollPos;
            int countMax = listMessages.Count;
            scrollPos += next ? 1 : -1;
            if (scrollPos > countMax - COUNT_MAX_LINE) scrollPos = countMax - COUNT_MAX_LINE;
            if (scrollPos < 0) scrollPos = 0;
            if (old != scrollPos)
            {
                UpMessages();
                PageUpdate();
                isRender = true;
            }
        }

        private void PageUpdate()
        {
            if (scrollPos > 0) buttonBack.Enabled = true;
            else buttonBack.Enabled = false;
            if (scrollPos + COUNT_MAX_LINE < listMessages.Count) buttonNext.Enabled = true;
            else buttonNext.Enabled = false;
        }

        private void TypeChat()
        {
            isConsole = !isConsole;
            TypeUpdate();
        }

        private void TypeUpdate()
        {
            buttonType.SetText(isConsole ? "..." : "~");
            scrollPos = 0;
            if (isConsole)
            {
                color = new vec3(1, .7f, .7f);
                textTitle = "gui.conteiner.console";
                listMessages = chatList.MessagesConsole;
                listSendMessages = chatList.SentMessagesConsole;
                textBoxMessage.SetText(historyBufferConsole);
            }
            else
            {
                color = new vec3(1);
                textTitle = "gui.conteiner.chat";
                listMessages = chatList.MessagesChat;
                listSendMessages = chatList.SentMessagesChat;
                textBoxMessage.SetText(historyBufferChat);
            }
            sentHistoryCursor = listSendMessages.Count;
            PageUpdate();
        }

        /// <summary>
        /// Такт игрового времени
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            UpMessages();
        }

        /// <summary>
        /// Обновить сообщения чата
        /// </summary>
        private void UpMessages()
        {
            string message = "";
            string textResult = "";

            int count = listMessages.Count;
            if (count > 0)
            {
                int i;
                int countMax = 0;
                count--;
                count -= scrollPos;
                for (i = count; i >= 0; i--)
                {
                    message = listMessages[i];
                    countMax++;
                    textResult = message + "\r\n" + textResult;
                    if (countMax >= COUNT_MAX_LINE) { break; }
                }
            }
            labelMessages.SetText(textResult);
        }

        /// <summary>
        /// Показать шаг истории
        /// </summary>
        private void SentHistory(int step)
        {
            int cursor = sentHistoryCursor + step;
            int count = listSendMessages.Count;
            cursor = Mth.Clamp(cursor, 0, count);

            if (cursor != sentHistoryCursor)
            {
                if (cursor == count)
                {
                    sentHistoryCursor = count;
                    textBoxMessage.SetText(isConsole ? historyBufferConsole : historyBufferChat);
                }
                else
                {
                    if (sentHistoryCursor == count)
                    {
                        if (isConsole) historyBufferConsole = textBoxMessage.Text;
                        else historyBufferChat = textBoxMessage.Text;
                    }
                    textBoxMessage.SetText(listSendMessages[cursor]);
                    sentHistoryCursor = cursor;
                }
            }
        }
    }
}
