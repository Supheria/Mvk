using MvkAssets;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenSingle : Screen
    {
        private Label label;
        private Button buttonCancel;
        private Button[] buttonSlots = new Button[5];
        private Button[] buttonSlotsDel = new Button[5];
        private Client client;

        public ScreenSingle(Client client, int slotDel) : base(client)
        {
            this.client = client;
            if (slotDel > 0) DelSlot(slotDel);
            client.ListSingle.Initialize(slotDel);

            label = new Label(Language.T("gui.singleplayer"), FontSize.Font16);

            for (int i = 0; i < buttonSlots.Length; i++)
            {
                buttonSlots[i] = new Button(Language.T(client.ListSingle.NameWorlds[i])) { Width = 356 };
                buttonSlots[i].Tag = i + 1; // Номер слота
                buttonSlots[i].Click += ButtonSlots_Click;
                buttonSlotsDel[i] = new Button("X")
                {
                    Width = 40,
                    Enabled = !client.ListSingle.EmptyWorlds[i]
                };
                buttonSlotsDel[i].Tag = i + 1; // Номер слота
                buttonSlotsDel[i].Click += ButtonSlotsDel_Click;
            }
            buttonCancel = new Button(EnumScreenKey.Main, Language.T("gui.cancel"));
            InitButtonClick(buttonCancel);
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(buttonCancel);
            for (int i = 0; i < buttonSlots.Length; i++)
            {
                AddControls(buttonSlots[i]);
                AddControls(buttonSlotsDel[i]);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = 120 * sizeInterface;
            int hMax = h + 292 * sizeInterface;
            if (hMax > Height) h -= hMax - Height;

            label.Position = new vec2i(Width / 2 - 200 * sizeInterface, h - 48 * sizeInterface);
            for (int i = 0; i < buttonSlots.Length; i++)
            {
                buttonSlots[i].Position = new vec2i(Width / 2 - 200 * sizeInterface, h + (i * 44) * sizeInterface);
                buttonSlotsDel[i].Position = new vec2i(Width / 2 + 160 * sizeInterface, h + (i * 44) * sizeInterface);
            }
            buttonCancel.Position = new vec2i(Width / 2 - 200 * sizeInterface, h + 240 * sizeInterface);
        }

        private void ButtonSlots_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int slot = (int)button.Tag;
            OnFinished(new ScreenEventArgs(EnumScreenKey.WorldBegin, EnumScreenKey.SinglePlayer, slot));
        }

        private void ButtonSlotsDel_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int slot = (int)button.Tag;
            OnFinished(new ScreenEventArgs(EnumScreenKey.YesNo, EnumScreenKey.SinglePlayer, slot,
               Language.T("gui.world.delete")));
        }

        /// <summary>
        /// Удалить слот мира
        /// </summary>
        /// <param name="slot">Слот 1-5</param>
        private void DelSlot(int slot)
        {
            if (slot > 0 && slot < 6)
            {
                client.ListSingle.WorldRemove(slot);
            }
        }
    }
}
