using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Network;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using System;

namespace MvkClient.Gui
{
    /// <summary>
    /// У гульні запушчана меню кантэйнера
    /// </summary>
    public class ScreenCraft : ScreenConteinerItemsPage
    {
        protected Label labelItemTitle;
        protected Label labelItemDis;
        protected Button buttonCraft;
        protected ButtonSlot buttonTool;
        private ViewItem viewItem;

        /// <summary>
        /// Тип окна крафта
        /// </summary>
        private readonly EnumWindowType window;

        /// <summary>
        /// Статус идёт ли крафт
        /// </summary>
        private bool isCraft = false;
        /// <summary>
        /// Время крафта максимальное
        /// </summary>
        private int timeCraftMax = 1;
        /// <summary>
        /// Время крафта
        /// </summary>
        private int timeCraft = 0;
        
        public ScreenCraft(Client client, EnumWindowType window) : base(client)
        {
            this.window = window;
            client.Player.Inventory.Crafted += InventoryCrafted;
            assetsTexture = AssetsTexture.ConteinerCraft;
            textTitle = "gui.conteiner.craft." + window;
            pageCount = 20;
        }

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public override void AcceptNetworkPackage(IPacket packet)
        {
            if (packet is PacketS2FSetSlot packetS2F)
            {
                // Изменился один слот
                if (packetS2F.GetSlot() == 100)
                {
                    buttonTool.SetSlot(new Slot(0, packetS2F.GetItemStack()));
                    isRender = true;
                }
            }
            else if (packet is PacketS30WindowItems packetS30)
            {
                // загрузить все слоты
                ItemStack[] stacks = packetS30.GetStacks();
                if (stacks.Length == 1)
                {
                    buttonTool.SetSlot(new Slot(0, stacks[0]));
                    isRender = true;
                }
            }
            else if (packet is PacketS31WindowProperty packetS31)
            {
                SetArrayItems(packetS31.GetRecipe());
                isRender = true;
            }
        }

        protected override void Init()
        {
            base.Init();
            viewItem = new ViewItem();
            viewItem.Init(this);
            labelItemTitle = new Label("", FontSize.Font12) { Alight = EnumAlight.Left };
            AddControls(labelItemTitle);
            labelItemDis = new Label("", FontSize.Font8) { Alight = EnumAlight.Left, Width = 378 };
            AddControls(labelItemDis);
            buttonCraft = new Button(Language.T("gui.button.craft")) { Width = 112, Enabled = false };
            buttonCraft.Click += ButtonCraftClick;
            AddControls(buttonCraft);
            if (window != EnumWindowType.CraftFirst)
            {
                labelItemDis.Width = 324;
                buttonTool = new ButtonSlot(new Slot(0));
                buttonTool.Click += ButtonToolClick;
                AddControls(buttonTool);
            }
            ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(PacketC0EPacketClickWindow.EnumAction.Open));
        }

        /// <summary>
        /// Зменены памер акна
        /// </summary>
        protected override void ResizedScreen()
        {
            base.ResizedScreen();

            int w = position.x;
            int h = position.y;

            labelItemTitle.Position = new vec2i(w + 118 * SizeInterface, h + 136 * SizeInterface);
            viewItem.Position = new vec2i(w + 58 * SizeInterface, h + 200 * SizeInterface);
            labelItemDis.Position = new vec2i(w + 118 * SizeInterface, h + 164 * SizeInterface);
            labelItemDis.Transfer();
            buttonCraft.Position = new vec2i(w + 393 * SizeInterface, h + 248 * SizeInterface);
            //TODO::2023-08-06 buttonTool
            if (window != EnumWindowType.CraftFirst)
            {
                buttonTool.Position = new vec2i(w + 455 * SizeInterface, h + 198 * SizeInterface);
            }
        }

        /// <summary>
        /// Дополнительная прорисовка сверх основной
        /// </summary>
        protected override void DrawAdd(float timeIndex)
        {
            base.DrawAdd(timeIndex);
            // 3д предмет
            GLRender.PushMatrix();
            GLRender.Translate(viewItem.Position.x, viewItem.Position.y, 0);
            GLRender.Scale(SizeInterface, SizeInterface, 1);
            viewItem.Draw(timeIndex);
            GLRender.PopMatrix();

            if (isCraft)
            {
                GLRender.PushMatrix();
                GLRender.Texture2DDisable();
                int w = position.x + 393 * SizeInterface;
                int h = position.y + 248 * SizeInterface;
                int w2 = buttonCraft.Width * SizeInterface;
                GLRender.Rectangle(w, h, w + w2 * timeCraft / timeCraftMax, h + buttonCraft.Height * SizeInterface, 
                    new vec4(1f, .5f, .3f, .3f));
                GLRender.Texture2DEnable();
                GLRender.PopMatrix();
            }
        }

        /// <summary>
        /// Нажата кнопка крафта
        /// </summary>
        private void ButtonCraftClick(object sender, EventArgs e)
        {
            ItemBase item = viewItem.Item;
            if (item != null)
            {
                ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(
                    PacketC0EPacketClickWindow.EnumAction.Craft, ClientMain.World.Key.KeyShift, false, item.Id));

                timeCraft = ClientMain.Player.Inventory.GetCountTimeCraft(item, buttonTool?.GetSlot().Stack, ClientMain.World.Key.KeyShift);
                buttonCraft.Enabled = false;
                isRender = true;

                if (timeCraft >= 0)
                {
                    timeCraftMax = timeCraft;
                    isCraft = true;
                }
            }
        }

        /// <summary>
        /// Крафт сделан, ответ от сервера
        /// </summary>
        private void InventoryCrafted(object sender, EventArgs e)
        {
            isCraft = false;
            buttonCraft.Enabled = viewItem.Item != null ? ClientMain.Player.Inventory.CheckItemCraft(viewItem.Item) : false;
            isRender = true;
        }

        /// <summary>
        /// Выбираем активный предмет из списка
        /// </summary>
        private void SetSelectItem(ItemBase item)
        {
            viewItem.SetItem(item);
            if (item != null)
            {
                // Имеется предмет
                buttonCraft.Enabled = ClientMain.Player.Inventory.CheckItemCraft(item);
                int amount = item.GetCraft().Amount;
                string name = item.GetName();
                labelItemTitle.SetText(Language.T(name) + (amount > 1 ? " §ex§l" + amount.ToString() + "§r" : ""));
                labelItemDis.SetText(Language.T(name + ".dis"));
                labelItemDis.Transfer();
            }
            else
            {
                // Нет предмета
                buttonCraft.Enabled = false;
                labelItemTitle.SetText("");
                labelItemDis.SetText("");
            }
        }

        /// <summary>
        /// Клик по складу
        /// </summary>
        protected override void StorageClick(ButtonSlot button, bool isShift, bool isRight)
        { 
            if (!isRight && !isShift && !isCraft) SetSelectItem(button.GetSlot().Empty() ? null : button.GetSlot().Stack.Item);
        }

        /// <summary>
        /// Клик по инвентарю
        /// </summary>
        protected override void InventoryClick(ButtonSlot button, bool isShift, bool isRight)
        {
            ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(
                PacketC0EPacketClickWindow.EnumAction.ClickSlot, isShift, isRight, button.GetSlot().Index));
        }

        /// <summary>
        /// Клик по инструменту
        /// </summary>
        private void ButtonToolClick(object sender, EventArgs e)
        {
            if (!isCraft && sender.GetType() == typeof(ButtonSlot))
            {
                SetSelectItem(null);
                ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(
                    PacketC0EPacketClickWindow.EnumAction.ClickSlot, ClientMain.World.Key.KeyShift, false, 100));
            }
        }

        /// <summary>
        /// Такт игрового времени
        /// </summary>
        public override void Tick()
        {
            if (isCraft)
            {
                timeCraft--;
                if (timeCraft <= 0)
                {
                    isCraft = false;
                }
            }
            base.Tick();
        }

        protected override void OnFinishing()
        {
            base.OnFinishing();
            ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(PacketC0EPacketClickWindow.EnumAction.Close));
        }

        /// <summary>
        /// Выбросить слот который в руке
        /// </summary>
        protected override void ThrowTheSlot()
        {
            if (!theSlot.Empty())
            {
                ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(PacketC0EPacketClickWindow.EnumAction.ThrowOutAir));
                theSlot.Clear();
            }
        }

        /// <summary>
        /// Контролы
        /// </summary>
        protected override void RenderControls()
        {
            base.RenderControls();
            // тут нужно нарисовать список предметов для крафта
            ItemBase itemSelect = viewItem.Item;
            if (itemSelect != null)
            {
                Element[] elements = itemSelect.GetCraft().CraftRecipe;
                int count = elements.Length;
                if (count > 0)
                {
                    int w = position.x;
                    int h = position.y;
                    FontSize size = FontSize.Font12;
                    string text;
                    int x, ws, i;
                    vec3 color = new vec3(1);

                    GLRender.PushMatrix();
                    GLRender.Translate(w + 132 * SizeInterface, h + 264 * SizeInterface, 0);
                    if (SizeInterface > 1) GLRender.Scale(SizeInterface);
                    GLRender.Texture2DEnable();
                    GLRender.Color(1);

                    Element element;
                    ItemBase item;
                    for (i = 0; i < count; i++)
                    {
                        x = i * 36;
                        element = elements[i];
                        item = element.GetItem();
                        if (item.EItem == EnumItem.Block && item is ItemBlock itemBlock)
                        {
                            // Прорисовка блока
                            ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(x, 0, 22);
                        }
                        else
                        {
                            // Прорисовка предмета
                            ClientMain.World.WorldRender.GetItemGui(item.EItem).Render(x, 0);
                        }
                        GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
                        text = element.GetAmount().ToString();
                        ws = FontRenderer.WidthString(text, size);
                        FontRenderer.RenderString(x - ws + 16, 5, text, size, color, 1, true);
                    }

                    GLRender.PopMatrix();
                }
            }

        }
    }
}
