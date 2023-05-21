using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Network.Packets.Client;
using System;

namespace MvkClient.Gui
{
    /// <summary>
    /// У гульні запушчана меню кантэйнера
    /// </summary>
    public class ScreenConteiner : ScreenWindow
    {
        protected Label labelPage;
        protected Button buttonClose;
        protected ButtonBox buttonBack;
        protected ButtonBox buttonNext;
        protected ButtonSlot[] button;
        protected ButtonSlot[] buttonInv;

        /// <summary>
        /// Абразок каля курсора мышы
        /// </summary>
        protected CursorIcon icon;

        /// <summary>
        /// Бягучая старонка
        /// </summary>
        private int page = 0;
        /// <summary>
        /// Колькасць на старонцы
        /// </summary>
        private readonly int pageCount = 50;

        /// <summary>
        /// Утрымлівае слот, які ў дадзены момант завісае
        /// </summary>
        private Slot theSlot;

        public ScreenConteiner(Client client) : base(client)
        {
            textTitle = "gui.conteiner.inventory";
            IsFpsMin = false;
            icon = new CursorIcon();
            theSlot = new Slot();
            button = new ButtonSlot[pageCount];
            buttonInv = new ButtonSlot[8];
            labelPage = new Label("", FontSize.Font12) { Alight = EnumAlight.Center, Width = 50 };
            background = EnumBackground.GameWindow;
            buttonClose = new Button(EnumScreenKey.World, "X") { Width = 42 };
            InitButtonClick(buttonClose);
            buttonBack = new ButtonBox("<") { Enabled = false };
            buttonBack.Click += (sender, e) => ButtonBackNextClick(false);
            buttonNext = new ButtonBox(">");
            buttonNext.Click += (sender, e) => ButtonBackNextClick(true);
            PageUpdate();
            int c = 0;
            for (int i = 0; i < pageCount; i++)
            {
                button[i] = new ButtonSlot(c < Items.inventory.Length 
                    ? new Slot(c + 100, ItemBase.GetItemById(Items.inventory[c++]))
                    : new Slot(99)
                    );
                button[i].Click += ButtonSlotClick;
                button[i].ClickRight += ButtonSlotClickRight;
            }
            for (int i = 0; i < 8; i++)
            {
                ItemStack itemStack = ClientMain.Player.Inventory.GetStackInSlot(i);
                buttonInv[i] = new ButtonSlot(itemStack != null
                    ? new Slot(i, itemStack.Item, itemStack.Amount)
                    : new Slot(i));
                buttonInv[i].Click += ButtonInvSlotClick;
                buttonInv[i].ClickRight += ButtonInvSlotClickRight;
            }
            client.Player.Inventory.Changed += InventoryChanged;
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public override void KeyDown(int key)
        {
            if (key == 27 || key == 69) // ESC || E
            {
                ClientMain.Screen.GameMode();
            }
        }

        private void InventoryChanged(object sender, SlotEventArgs e)
        {
            int slot = e.IndexSlot;
            if (slot >= 0 && slot < 8)
            {
                ItemStack itemStack = ClientMain.Player.Inventory.GetStackInSlot(slot);
                buttonInv[slot].SetSlot(itemStack != null
                    ? new Slot(slot, itemStack.Item, itemStack.Amount)
                    : new Slot(slot));
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    ItemStack itemStack = ClientMain.Player.Inventory.GetStackInSlot(i);
                    buttonInv[i].SetSlot(itemStack != null
                        ? new Slot(i, itemStack.Item, itemStack.Amount)
                        : new Slot(i));
                }
            }
            isRender = true;
        }

        protected override void Init()
        {
            icon.Init(this);
            AddControls(labelPage);
            AddControls(buttonClose);
            AddControls(buttonBack);
            AddControls(buttonNext);
            for (int i = 0; i < pageCount; i++) if (button[i] != null) AddControls(button[i]);
            for (int i = 0; i < 8; i++) AddControls(buttonInv[i]);
        }

        /// <summary>
        /// Зменены памер акна
        /// </summary>
        protected override void ResizedScreen()
        {
            position = new vec2i(Width / 2 - size.x / 2 * SizeInterface, Height / 2 - size.y / 2 * SizeInterface);
            int w = position.x;
            int h = position.y;

            buttonClose.Position = new vec2i(w + 470 * SizeInterface, h - 8 * SizeInterface);
            labelPage.Position = new vec2i(w + 306 * SizeInterface, h);
            buttonBack.Position = new vec2i(w + 274 * SizeInterface, h);
            buttonNext.Position = new vec2i(w + 356 * SizeInterface, h);
            int x0 = w + 6 * SizeInterface;
            int y0 = h + 38 * SizeInterface;
            int x = x0;
            int y = y0;
            int c = 0;
            for (int i = 0; i < pageCount; i++)
            {
                if (button[i] != null)
                {
                    button[i].Position = new vec2i(x, y);
                    c++;
                    x += 50 * SizeInterface;
                    if (c == 10)
                    {
                        c = 0;
                        y += 50 * SizeInterface;
                        x = x0;
                    }
                }
            }
            y = h + 298 * SizeInterface;
            x = w + 56 * SizeInterface;
            for (int i = 0; i < 8; i++)
            {
                buttonInv[i].Position = new vec2i(x, y);
                x += 50 * SizeInterface;
            }

        }

        /// <summary>
        /// Перамяшчэнне мышкі
        /// </summary>
        public override void MouseMove(int x, int y)
        {
            base.MouseMove(x, y);
            icon.MouseMove(x, y);
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void MouseDown(MouseButton button, int x, int y)
        {
            bool b = IsOutsideWindow(x, y);
            if (theSlot.Item != null && b)
            {
                ClientMain.TrancivePacket(new PacketC10CreativeInventoryAction(-1, new ItemStack(theSlot.Item, theSlot.Amount)));
                theSlot.Clear();
                icon.SetSlot(theSlot);
            }
            else
            {
                base.MouseDown(button, x, y);
            }
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public override void MouseWheel(int delta, int x, int y)
        {
            bool next = delta > 0;
            if ((next && Items.inventory.Length > page + pageCount - 1)
                || (!next && page > 0))
            {
                ButtonBackNextClick(next);
                isRender = true;
            }
        }

        protected override void DrawAdd()
        {
            base.DrawAdd();
            // Иконка
            GLRender.PushMatrix();
            GLRender.Translate(icon.Position.x, icon.Position.y, 0);
            GLRender.Scale(SizeInterface, SizeInterface, 1);
            icon.Draw();
            GLRender.PopMatrix();
        }

        private void ButtonBackNextClick(bool next)
        {
            page += next ? pageCount : -pageCount;

            int c = page;

            for (int i = 0; i < pageCount; i++)
            {
                if (c < Items.inventory.Length)
                {
                    button[i].GetSlot().SetIndex(c + 100);
                    button[i].GetSlot().Set(ItemBase.GetItemById(Items.inventory[c++]));
                }
                else
                {
                    button[i].GetSlot().SetIndex(99);
                    button[i].GetSlot().Set();
                }
            } 
            PageUpdate();
        }

        private void PageUpdate()
        {
            int max = Items.inventory.Length;
            
            labelPage.SetText((page / pageCount + 1).ToString() + " / " + (max / pageCount + 1).ToString());
            if (page <= 0) buttonBack.Enabled = false;
            else buttonBack.Enabled = true;
            if (Items.inventory.Length > page + pageCount) buttonNext.Enabled = true;
            else buttonNext.Enabled = false;
        }

        private void ButtonSlotClick(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ButtonSlot)) CreativeInventoryClick(sender as ButtonSlot, ClientMain.World.Key.KeyShift, false);
        }

        private void ButtonSlotClickRight(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ButtonSlot)) CreativeInventoryClick(sender as ButtonSlot, ClientMain.World.Key.KeyShift, true);
        }

        private void ButtonInvSlotClick(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ButtonSlot)) InventoryClick(sender as ButtonSlot, ClientMain.World.Key.KeyShift, false);
        }

        private void ButtonInvSlotClickRight(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ButtonSlot)) InventoryClick(sender as ButtonSlot, ClientMain.World.Key.KeyShift, true);
        }

        /// <summary>
        /// Клик по складу
        /// </summary>
        private void CreativeInventoryClick(ButtonSlot button, bool isShift, bool isRight)
        {
            // количество для руки
            int amount;
            if (button.GetSlot().Item == null)
            {
                // удалить что в руке
                // что-то в руке есть но кликнули на другой
                amount = (theSlot.Item != null && isRight) ? theSlot.Amount - 1 : 0;
            }
            else
            {
                // клик на что-то
                if (theSlot.Item == null)
                {
                    // Берём предмет
                    amount = isShift ? button.GetSlot().Item.MaxStackSize : 1;
                }
                else if (theSlot.Item != null && theSlot.Item.Id == button.GetSlot().Item.Id)
                {
                    // Кликнули на тоже самое, что в руке
                    amount = theSlot.Amount + (isRight ? -1 : (isShift ? button.GetSlot().Item.MaxStackSize : 1));
                    if (amount > theSlot.Item.MaxStackSize) amount = theSlot.Item.MaxStackSize;
                }
                else
                {
                    // что-то в руке есть но кликнули на другой
                    amount = isRight ? theSlot.Amount - 1 : 0;
                }
            }

            if (amount <= 0) theSlot.Clear();
            else theSlot.Set(theSlot.Item ?? button.GetSlot().Item, amount);

            icon.SetSlot(theSlot);
        }


        /// <summary>
        /// Клик по инвентарю
        /// </summary>
        private void InventoryClick(ButtonSlot button, bool isShift, bool isRight)
        {
            // количество для руки
            int amount;
            ItemBase item = theSlot.Item;
            Slot slot = button.GetSlot().Clone();
            bool isSlot = false;
            if (button.GetSlot().Item == null)
            {
                // удалить что в руке
                // что-то в руке есть но кликнули на другой
                amount = (theSlot.Item != null && isRight) ? theSlot.Amount - 1 : 0;

                if (theSlot.Item != null)
                {
                    // Оставляем в пустую ячейку или одну или все
                    slot.Set(theSlot.Item, isRight ? 1 : theSlot.Amount);
                    isSlot = true;
                }
            }
            else
            {
                if (isShift)
                {
                    // Клик на предмет, убираем его не важно что в руке, оставляя предмет в руке
                    slot.SetAmount(0);
                    amount = theSlot.Amount;
                }
                // клик на что-то
                else if (theSlot.Item == null)
                {
                    // Берём предмет
                    amount = button.GetSlot().Amount;
                    item = button.GetSlot().Item;
                    if (isRight) amount /= 2;
                    slot.SetAmount(button.GetSlot().Amount - amount);
                }
                else if (theSlot.Item != null && theSlot.Item.Id == button.GetSlot().Item.Id)
                {
                    // Кликнули на тоже самое, что в руке

                    // сумма слотов
                    int aw = theSlot.Amount + button.GetSlot().Amount;
                    if (aw > theSlot.Item.MaxStackSize)
                    {
                        // сумма больше слота
                        slot.SetAmount(slot.Item.MaxStackSize);
                        amount = aw - slot.Amount;
                    }
                    else
                    {
                        // сумма меньше слота
                        slot.SetAmount(aw);
                        amount = 0;
                    }
                }
                else
                {
                    // что-то в руке есть но кликнули на другой, заменяем местами
                    slot.Set(theSlot.Item, theSlot.Amount);
                    item = button.GetSlot().Item;
                    amount = button.GetSlot().Amount;
                }
                isSlot = true;
            }

            // Корректировка для слота руки
            if (amount <= 0) theSlot.Clear(); else theSlot.Set(item, amount);
            icon.SetSlot(theSlot);

            if (isSlot)
            {
                // Если надо корректировка для слота инвентаря
                if (slot.Amount <= 0) slot.Set(null, 0);
                button.SetSlot(slot);
                // Отправляем на сервер
                ClientMain.TrancivePacket(new PacketC10CreativeInventoryAction(slot.Index, new ItemStack(slot.Item, slot.Amount)));
            }
        }
    }
}
