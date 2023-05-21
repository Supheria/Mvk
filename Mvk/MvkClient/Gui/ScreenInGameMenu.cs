using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenInGameMenu : Screen
    {
        protected Button buttonBack;
        protected Button buttonOptions;
        protected Button buttonExit;

        public ScreenInGameMenu(Client client) : base(client)
        {
            background = EnumBackground.GameWindow;

            buttonBack = new Button(EnumScreenKey.World, Language.T("gui.back.game"));
            InitButtonClick(buttonBack);
            buttonOptions = new Button(EnumScreenKey.Options, Language.T("gui.options"));
            buttonOptions.Click += (sender, e) 
                => OnFinished(new ScreenEventArgs(EnumScreenKey.Options, EnumScreenKey.InGameMenu));
            buttonExit = new Button(Language.T("gui.exit.world"));
            buttonExit.Click += (sender, e) => ClientMain.ExitingWorld("");
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public override void KeyDown(int key)
        {
            if (key == 27)// ESC
            {
                ClientMain.Screen.GameMode();
            }
        }

        protected override void Init()
        {
            AddControls(buttonBack);
            AddControls(buttonOptions);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = Height / 4 + 48 * SizeInterface;
            int hMax = h + 248 * SizeInterface;
            if (hMax > Height) h -= hMax - Height;

            buttonBack.Position = new vec2i(Width / 2 - 200 * SizeInterface, h);
            buttonOptions.Position = new vec2i(Width / 2 - 200 * SizeInterface, h + 44 * SizeInterface);
            buttonExit.Position = new vec2i(Width / 2 - 200 * SizeInterface, h + 144 * SizeInterface);
        }
    }
}
