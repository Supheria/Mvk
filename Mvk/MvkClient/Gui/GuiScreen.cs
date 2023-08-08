using MvkClient.Actions;
using MvkClient.Setitings;
using MvkServer.Network;
using MvkServer.Util;
using System;

namespace MvkClient.Gui
{
    public class GuiScreen
    {
        /// <summary>
        /// Активный скрин
        /// </summary>
        protected Screen screen;

        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        public GuiScreen(Client client)
        {
            ClientMain = client;
            screen = new ScreenBeginLoading(client);
        }

        /// <summary>
        /// Пустой ли скрин
        /// </summary>
        public bool IsEmptyScreen() => screen == null;
        /// <summary>
        /// Получить тип скрина
        /// </summary>
        public Type GetTypeScreen() => screen.GetType();

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void Resized()
        {
            if (screen != null) screen.Resized();
        }

        /// <summary>
        /// Такт игрового времени
        /// </summary>
        public void Tick()
        {
            if (screen != null) screen.Tick();
        }

        /// <summary>
        /// Прорисовка нужного скрина если это надо
        /// </summary>
        public void DrawScreen(float timeIndex)
        {
            if (screen != null) screen.Draw(timeIndex);
        }

        /// <summary>
        /// Запуск первого скрина
        /// </summary>
        public void Begin() => screen.Initialize();

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public void MouseMove(int x, int y)
        {
            if (screen != null) screen.MouseMove(x, y);
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public bool MouseDown(MouseButton button, int x, int y)
        {
            if (screen != null)
            {
                screen.MouseDown(button, x, y);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y)
        {
            if (screen != null) screen.MouseUp(button, x, y);
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public void MouseWheel(int delta, int x, int y)
        {
            if (screen != null) screen.MouseWheel(delta, x, y);
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void KeyPress(char key)
        {
            if (screen != null) screen.KeyPress(key);
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public void KeyDown(int key)
        {
            if (screen != null) screen.KeyDown(key);
        }

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public void AcceptNetworkPackage(IPacket packet)
        {
            if (screen != null) screen.AcceptNetworkPackage(packet);
        }

        /// <summary>
        /// Заменить экран на другое меню
        /// </summary>
        public void Exchange(Screen screenNew)
        {
            if (screen != null) screen.Delete();
            ClientMain.SetWishFps(screenNew.IsFpsMin ? 20 : Setting.Fps);
            if (ClientMain.World != null) ClientMain.Player.MovementNone();
            screen = screenNew;
            screen.Finished += MenuScreen_Finished;
            screen.Initialize();
            OnChanged();
        }

        /// <summary>
        /// Активация контейнера во время игры
        /// </summary>
        public void InGameConteinerCreative()
        {
            if (ClientMain.Player.IsCreativeMode)
            {
                Exchange(new ScreenConteinerCreative(ClientMain));
            }
        }
        /// <summary>
        /// Активация окна во время игры типа крафта, ящика и прочего
        /// </summary>
        public void InGameWindow(EnumWindowType windowType)
        {
            if (windowType == EnumWindowType.Box)
            {
                Exchange(new ScreenConteinerBox(ClientMain));
            }
            else
            {
                Exchange(new ScreenCraft(ClientMain, windowType));
            }
        }
        /// <summary>
        /// Активация меню во время игры
        /// </summary>
        public void InGameMenu() => Exchange(new ScreenInGameMenu(ClientMain));
        /// <summary>
        /// Активация чат во время игры
        /// </summary>
        public void InGameChat(bool isConsole) => Exchange(new ScreenChat(ClientMain, isConsole));
        /// <summary>
        /// Главного меню
        /// </summary>
        public void MainMenu() => Exchange(new ScreenMainMenu(ClientMain));
        /// <summary>
        /// Сохранение мира
        /// </summary>
        public void ScreenProcess(string text) => Exchange(new ScreenProcess(ClientMain, text));
        /// <summary>
        /// Окно ошибки
        /// </summary>
        public void ScreenError(string text) => Exchange(new ScreenError(ClientMain, text));
        /// <summary>
        /// Конец игры
        /// </summary>
        public void GameOver(string text) => Exchange(new ScreenGameOver(ClientMain, text));

        /// <summary>
        /// Удалить экран
        /// </summary>
        protected void Delete()
        {
            if (screen != null)
            {
                screen.Delete();
                screen = null;
                OnChanged();
            }
        }

        /// <summary>
        /// Убрать Gui, переход в режим игры
        /// </summary>
        public void GameMode() => Delete();

        #region Loading

        /// <summary>
        /// Задать максимальную загрузку компонентов
        /// </summary>
        public void LoadingSetMax(int max)
        {
            if (screen.GetType() == typeof(ScreenBeginLoading))
            {
                ((ScreenBeginLoading)screen).SetMax(max);
            }
            else if (screen.GetType() == typeof(ScreenWorldLoading))
            {
                ((ScreenWorldLoading)screen).SetMax(max);
            }
        }
        /// <summary>
        /// Шаг загрузки
        /// </summary>
        public void LoadingStep()
        {
            if (screen.GetType() == typeof(ScreenBeginLoading))
            {
                ((ScreenBeginLoading)screen).Step();
            }
            else if (screen.GetType() == typeof(ScreenWorldLoading))
            {
                ((ScreenWorldLoading)screen).Step();
            }
        }
        /// <summary>
        /// Окончание загрузки, переходим в главное меню
        /// </summary>
        public void LoadingMainEnd() => Exchange(new ScreenMainMenu(ClientMain));
        
        #endregion

        private void MenuScreen_Finished(object sender, ScreenEventArgs e)
        {
            switch(e.Key)
            {
                case EnumScreenKey.Options: Exchange(new ScreenOptions(ClientMain, e.Where)); break;
                case EnumScreenKey.Main: MainMenu(); break;
                case EnumScreenKey.SinglePlayer: Exchange(new ScreenSingle(ClientMain, e.Slot)); break;
                case EnumScreenKey.Multiplayer: Exchange(new ScreenMultiplayer(ClientMain)); break;
                case EnumScreenKey.Connection: ClientMain.LoadWorldNet(e.Tag.ToString()); break;
                case EnumScreenKey.YesNo: Exchange(new ScreenYesNo(ClientMain, e.Text, e.Where, e.Slot)); break;
                case EnumScreenKey.WorldBegin:
                    // Запуск загрузки мира
                    Exchange(new ScreenWorldLoading(ClientMain, e.Slot));
                    ClientMain.LoadWorld((byte)e.Slot);
                    break;
                case EnumScreenKey.World: Delete(); break;
                case EnumScreenKey.InGameMenu: InGameMenu(); break;
            }
        }

        #region Event

        /// <summary>
        /// Событие изменён скрин
        /// </summary>
        public event EventHandler Changed;
        protected virtual void OnChanged() => Changed?.Invoke(this, new EventArgs());

        #endregion
    }
}
