﻿using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenYesNo : Screen
    {
        protected Label label;
        protected Button buttonNo;
        protected Button buttonYes;
        protected int slot;

        public ScreenYesNo(Client client, string text, EnumScreenKey where, int slot) : base(client)
        {
            this.slot = slot;
            this.where = where;
            label = new Label(text, FontSize.Font16);
            buttonNo = new Button(EnumScreenKey.Main, Language.T("gui.no")) { Width = 198 };
            buttonNo.Click += (sender, e) => OnFinished(new ScreenEventArgs(where, EnumScreenKey.YesNo, -1));
            buttonYes = new Button(EnumScreenKey.Main, Language.T("gui.yes")) { Width = 198 };
            buttonYes.Click += (sender, e) => OnFinished(new ScreenEventArgs(where, EnumScreenKey.YesNo, slot));
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(buttonNo);
            AddControls(buttonYes);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = Height / 4 + 44 * SizeInterface;
            int hMax = h + 208 * SizeInterface;
            if (hMax > Height) h -= hMax - Height;

            label.Position = new vec2i(Width / 2 - 200 * SizeInterface, h);
            buttonYes.Position = new vec2i(Width / 2 - 200 * SizeInterface, h + 148 * SizeInterface);
            buttonNo.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 148 * SizeInterface);
        }
    }
}
