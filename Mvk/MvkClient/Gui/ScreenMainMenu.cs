using MvkAssets;
using MvkClient.Setitings;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenMainMenu : Screen
    {
        protected Button buttonSingle;
        protected Button buttonMultiplayer;
        protected Button buttonOptions;
        protected Button buttonExit;

        public ScreenMainMenu(Client client) : base(client)
        {
            background = EnumBackground.TitleMain;

            buttonSingle = new Button(EnumScreenKey.SinglePlayer, Language.T("gui.singleplayer")) { Width = 300 };
            InitButtonClick(buttonSingle);
            buttonMultiplayer = new Button(EnumScreenKey.Multiplayer, Language.T("gui.multiplayer")) { Width = 300 };
            InitButtonClick(buttonMultiplayer);
            buttonOptions = new Button(EnumScreenKey.Options, Language.T("gui.options")) { Width = 300 };
            buttonOptions.Click += (sender, e)
                => OnFinished(new ScreenEventArgs(EnumScreenKey.Options, EnumScreenKey.Main));
            buttonExit = new Button(Language.T("gui.exit")) { Width = 300 };
            buttonExit.Click += ButtonExit_Click;

        }

        protected override void Init()
        {
            AddControls(buttonSingle);
            AddControls(buttonMultiplayer);
            AddControls(buttonOptions);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = Height / 4 + 92 * SizeInterface;
            int hMax = h + 208 * SizeInterface;
            if (hMax > Height) h -= hMax - Height;

            buttonSingle.Position = new vec2i(100 * SizeInterface, h);
            buttonMultiplayer.Position = new vec2i(100 * SizeInterface, h + 44 * SizeInterface);
            buttonOptions.Position = new vec2i(100 * SizeInterface, h + 88 * SizeInterface);
            buttonExit.Position = new vec2i(100 * SizeInterface, h + 148 * SizeInterface);
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            ClientMain.WindowClosing();
        }
    }
}
