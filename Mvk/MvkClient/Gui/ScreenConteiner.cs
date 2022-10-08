using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Network.Packets.Client;
using MvkServer.World.Block;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    /// <summary>
    /// У гульні запушчана меню кантэйнера
    /// </summary>
    public class ScreenConteiner : Screen
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
            icon = new CursorIcon();
            theSlot = new Slot();
            button = new ButtonSlot[pageCount];
            buttonInv = new ButtonSlot[8];
            IsConteiner = true;
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
                button[i] = new ButtonSlot(c < Blocks.blocksInventory.Length 
                    ? new Slot(c + 100, ItemBase.GetItemById(Blocks.blocksInventory[c++]))
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
            int w = Width / 2;
            int h = Height / 2;
            buttonClose.Position = new vec2i(w + 214 * sizeInterface, h - 221 * sizeInterface);
            labelPage.Position = new vec2i(w + 50 * sizeInterface, h - 213 * sizeInterface);
            buttonBack.Position = new vec2i(w + 18 * sizeInterface, h - 213 * sizeInterface);
            buttonNext.Position = new vec2i(w + 100 * sizeInterface, h - 213 * sizeInterface);
            int x0 = w - 250 * sizeInterface;
            int y0 = h - 179 * sizeInterface;
            int x = x0;
            int y = y0;
            int c = 0;
            for (int i = 0; i < pageCount; i++)
            {
                if (button[i] != null)
                {
                    button[i].Position = new vec2i(x, y);
                    c++;
                    x += 50 * sizeInterface;
                    if (c == 10)
                    {
                        c = 0;
                        y += 50 * sizeInterface;
                        x = x0;
                    }
                }
            }
            y = h + 85 * sizeInterface;
            x = w - 200 * sizeInterface;
            for (int i = 0; i < 8; i++)
            {
                buttonInv[i].Position = new vec2i(x, y);
                x += 50 * sizeInterface;
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
        /// Генерацыя фону
        /// </summary>
        protected override void Render()
        {
            // Фон
            RenderBackground();

            GLRender.PushMatrix();
            vec4 colBg = new vec4(1f, 1f, 1f, 1f);
            vec4 colPr = new vec4(.13f, .44f, .91f, 1f);
            // назва
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.ConteinerItems);
            gl.Color(1f, 1f, 1f, 1f);
            int wh = Width / 2;
            int hh = Height / 2;
            GLRender.Translate(wh, hh - 36 * sizeInterface, 0);
            GLRender.Scale(Setting.SizeInterface);

            GLRender.Rectangle(-256, -177, 256, 177, 0, 0, 1f, 0.69140625f);

            GLWindow.Texture.BindTexture(AssetsTexture.Font12);
            string text = Language.T("gui.conteiner.inventory");
            FontRenderer.RenderString(-242, -166, new vec4(.1f, .1f, .1f, 1f), text, FontSize.Font12);
            FontRenderer.RenderString(-244, -167, new vec4(1f), text, FontSize.Font12);

            GLRender.PopMatrix();

            // Контролы
            RenderControls();
        }

        protected override void DrawAdd()
        {
            // Иконка
            GLRender.PushMatrix();
            GLRender.Translate(icon.Position.x, icon.Position.y, 0);
            GLRender.Scale(sizeInterface, sizeInterface, 1);
            icon.Draw();
            GLRender.PopMatrix();
        }

        private void ButtonBackNextClick(bool next)
        {
            page += next ? pageCount : -pageCount;

            int c = page;

            for (int i = 0; i < pageCount; i++)
            {
                if (c < Blocks.blocksInventory.Length)
                {
                    button[i].GetSlot().SetIndex(c + 100);
                    button[i].GetSlot().Set(ItemBase.GetItemById(Blocks.blocksInventory[c++]));
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
            int max = Blocks.blocksInventory.Length;
            
            labelPage.SetText((page / pageCount + 1).ToString() + " / " + (max / pageCount + 1).ToString());
            if (page <= 0) buttonBack.Enabled = false;
            else buttonBack.Enabled = true;
            if (Blocks.blocksInventory.Length > page + pageCount) buttonNext.Enabled = true;
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
