using MvkAssets;
using MvkClient.Renderer;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using System;

namespace MvkClient.Gui
{
    /// <summary>
    /// У гульні запушчана меню кантэйнера
    /// </summary>
    public abstract class ScreenConteinerItems : ScreenWindow
    {
        protected Label labelPage;
        protected Button buttonClose;
        protected ButtonBox buttonBack;
        protected ButtonBox buttonNext;
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

        /// <summary>
        /// Массив предметов склада
        /// </summary>
        private int[] arrayItems = new int[0];
        /// <summary>
        /// Количество предметов склада
        /// </summary>
        private int countItemsStorage = 0;

        /// <summary>
        /// Бягучая старонка
        /// </summary>
        private int page = 0;

        public ScreenConteinerItems(Client client) : base(client)
        {
            textTitle = "gui.conteiner.inventory";
            IsFpsMin = false;
            background = EnumBackground.GameWindow;
            client.Player.Inventory.Changed += InventoryChanged;
        }

        /// <summary>
        /// Задать массив склада предметов
        /// </summary>
        protected void SetArrayItems(int[] array)
        {
            countItemsStorage = array.Length;
            arrayItems = array;

            UpdateButtonStorage(0);
        }

        protected override void Init()
        {
            theSlot = new Slot();

            icon = new CursorIcon();
            icon.Init(this);
            icon.SetSlot(theSlot);
            
            buttonStorage = new ButtonSlot[pageCount];
            buttonInventory = new ButtonSlot[8];
            labelPage = new Label("", FontSize.Font12) { Alight = EnumAlight.Center, Width = 50 };
            AddControls(labelPage);
            
            buttonClose = new Button(EnumScreenKey.World, "X") { Width = 42 };
            AddControls(buttonClose);
            InitButtonClick(buttonClose);

            buttonBack = new ButtonBox("<") { Enabled = false };
            buttonBack.Click += (sender, e) => ButtonBackNextClick(false);
            AddControls(buttonBack);
            buttonNext = new ButtonBox(">");
            buttonNext.Click += (sender, e) => ButtonBackNextClick(true);
            AddControls(buttonNext);

            PageUpdate();
            for (int i = 0; i < pageCount; i++)
            {
                buttonStorage[i] = new ButtonSlot(new Slot(99));
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
        /// Обновить кнопки склада от какой страницы
        /// </summary>
        private void UpdateButtonStorage(int c)
        {
            for (int i = 0; i < pageCount; i++)
            {
                if (c < countItemsStorage)
                {
                    buttonStorage[i].GetSlot().SetIndex(c + 100);
                    buttonStorage[i].GetSlot().Set(GetItemStackById(arrayItems[c++]));
                }
                else
                {
                    buttonStorage[i].GetSlot().SetIndex(99);
                    buttonStorage[i].GetSlot().Set();
                }
            }
            PageUpdate();
        }

        /// <summary>
        /// По id предмета получить стак
        /// </summary>
        protected ItemStack GetItemStackById(int id)
        {
            ItemBase item = ItemBase.GetItemById(id);
            return item != null ? new ItemStack(item) : null;
        }

        private ItemStack CloneItemStackPlayer(int id)
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
            y = h + 298 * SizeInterface;
            x = w + 56 * SizeInterface;
            for (int i = 0; i < 8; i++)
            {
                buttonInventory[i].Position = new vec2i(x, y);
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
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public override void MouseWheel(int delta, int x, int y)
        {
            bool next = delta < 0;
            if ((next && countItemsStorage > page + pageCount - 1)
                || (!next && page > 0))
            {
                ButtonBackNextClick(next);
                isRender = true;
            }
        }

        /// <summary>
        /// Дополнительная прорисовка сверх основной
        /// </summary>
        protected override void DrawAdd(float timeIndex)
        {
            base.DrawAdd(timeIndex);
            // Иконка
            GLRender.PushMatrix();
            GLRender.Translate(icon.Position.x, icon.Position.y, 0);
            GLRender.Scale(SizeInterface, SizeInterface, 1);
            icon.Draw(timeIndex);
            GLRender.PopMatrix();
        }

        private void ButtonBackNextClick(bool next)
        {
            page += next ? pageCount : -pageCount;
            UpdateButtonStorage(page);
        }

        private void PageUpdate()
        {
            labelPage.SetText((page / pageCount + 1).ToString() + " / " + (countItemsStorage / pageCount + 1).ToString());
            if (page <= 0) buttonBack.Enabled = false;
            else buttonBack.Enabled = true;
            if (countItemsStorage > page + pageCount) buttonNext.Enabled = true;
            else buttonNext.Enabled = false;
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
