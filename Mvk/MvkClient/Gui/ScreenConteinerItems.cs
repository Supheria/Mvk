using MvkClient.Renderer;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using System;

namespace MvkClient.Gui
{
    /// <summary>
    /// Абстрактный класс контейнера с ячейками
    /// </summary>
    public abstract class ScreenConteinerItems : ScreenWindow
    {
        protected Button buttonClose;
        protected ButtonSlot[] buttonStorage;
        protected ButtonSlot[] buttonInventory;

        /// <summary>
        /// Абразок каля курсора мышы
        /// </summary>
        protected CursorIcon icon;
        /// <summary>
        /// Колькасць на старонцы
        /// </summary>
        protected int pageCount = 50;
        /// <summary>
        /// Утрымлівае слот, які ў дадзены момант завісае
        /// </summary>
        protected Slot theSlot;
        /// <summary>
        /// Нужен рендер иконки
        /// </summary>
        protected bool isRenderIcon = false;

        public ScreenConteinerItems(Client client) : base(client)
        {
            IsFpsMin = false;
            background = EnumBackground.GameWindow;
            client.Player.Inventory.Changed += InventoryChanged;
        }

        protected override void Init()
        {
            theSlot = new Slot();

            icon = new CursorIcon();
            icon.Init(this);
            icon.SetSlot(theSlot);
            
            buttonStorage = new ButtonSlot[pageCount];
            buttonInventory = new ButtonSlot[8];
            
            buttonClose = new Button(EnumScreenKey.World, "X") { Width = 42 };
            AddControls(buttonClose);
            InitButtonClick(buttonClose);

            for (int i = 0; i < pageCount; i++)
            {
                buttonStorage[i] = new ButtonSlot(new Slot(i));
                buttonStorage[i].Click += ButtonSlotClick;
                buttonStorage[i].ClickRight += ButtonSlotClickRight;
                AddControls(buttonStorage[i]);
            }
            for (int i = 0; i < 8; i++)
            {
                buttonInventory[i] = new ButtonSlot(new Slot(i, CloneItemStackPlayer(i)));
                buttonInventory[i].Click += ButtonInvSlotClick;
                buttonInventory[i].ClickRight += ButtonInvSlotClickRight;
                AddControls(buttonInventory[i]);
            }
        }

        /// <summary>
        /// По id предмета получить стак
        /// </summary>
        protected ItemStack GetItemStackById(int id)
        {
            ItemBase item = ItemBase.GetItemById(id);
            return item != null ? new ItemStack(item) : null;
        }

        protected ItemStack CloneItemStackPlayer(int id)
        {
            ItemStack stack = ClientMain.Player.Inventory.GetStackInSlot(id);
            return stack?.Copy(); // stack != null ? stack.Copy() : null
        }

        /// <summary>
        /// Изменёно что-то в инвентаре игрока
        /// </summary>
        private void InventoryChanged(object sender, SlotEventArgs e)
        {
            int slot = e.IndexSlot;

            if (slot == 255)
            {
                theSlot.Set(ClientMain.Player.Inventory.StackAir);
                isRenderIcon = true;
            }
            else if (slot > 99)
            {
                // тут ненадо, это склад TileEntity блока
            }
            else if (slot >= 0 && slot < 8)
            {
                buttonInventory[slot].SetSlot(new Slot(slot, CloneItemStackPlayer(slot)));
                isRender = true;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    buttonInventory[i].SetSlot(new Slot(i, CloneItemStackPlayer(i)));
                }
                isRender = true;
            }
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

            int i, x, y;
            y = h + 298 * SizeInterface;
            x = w + 56 * SizeInterface;
            for (i = 0; i < 8; i++)
            {
                buttonInventory[i].Position = new vec2i(x, y);
                x += 50 * SizeInterface;
            }
            // Расположение на окне склада
            ResizedScreenStorage(w, h);
        }

        /// <summary>
        /// Расположение на окне склада
        /// </summary>
        protected virtual void ResizedScreenStorage(int w, int h)
        {
            int i, x, y, x0, c;
            x = x0 = w + 6 * SizeInterface;
            y = h + 38 * SizeInterface;
            c = 0;
            for (i = 0; i < pageCount; i++)
            {
                if (buttonStorage[i] != null)
                {
                    buttonStorage[i].Position = new vec2i(x, y);
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
        /// Выбросить слот который в руке
        /// </summary>
        protected virtual void ThrowTheSlot() { }

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void OnClickOutsideWindow() => ThrowTheSlot();

        /// <summary>
        /// Происходит перед закрытием окна
        /// </summary>
        protected override void OnFinishing() => ThrowTheSlot();

        /// <summary>
        /// Дополнительная прорисовка сверх основной
        /// </summary>
        protected override void DrawAdd(float timeIndex)
        {
            if (isRenderIcon)
            {
                isRenderIcon = false;
                icon.PereRender();
            }

            base.DrawAdd(timeIndex);
            // Иконка
            GLRender.PushMatrix();
            GLRender.Translate(icon.Position.x, icon.Position.y, 0);
            GLRender.Scale(SizeInterface, SizeInterface, 1);
            icon.Draw(timeIndex);
            GLRender.PopMatrix();
        }

        private void ButtonSlotClick(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ButtonSlot)) StorageClick(sender as ButtonSlot, ClientMain.World.Key.KeyShift, false);
        }

        private void ButtonSlotClickRight(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ButtonSlot)) StorageClick(sender as ButtonSlot, ClientMain.World.Key.KeyShift, true);
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
        protected virtual void StorageClick(ButtonSlot button, bool isShift, bool isRight) { }

        /// <summary>
        /// Клик по инвентарю
        /// </summary>
        protected virtual void InventoryClick(ButtonSlot button, bool isShift, bool isRight) { }
    }
}
