using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Network;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;

namespace MvkClient.Gui
{
    /// <summary>
    /// У гульні запушчана меню кантэйнера
    /// </summary>
    public class ScreenConteinerBox : ScreenConteinerItems
    {
        public ScreenConteinerBox(Client client) : base(client)
        {
            textTitle = "gui.conteiner.box";
            pageCount = 4;
        }

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public override void AcceptNetworkPackage(IPacket packet)
        {
            if (packet is PacketS2FSetSlot packetS2F)
            {
                // Изменился один слот
                int slot = packetS2F.GetSlot() - 100;
                buttonStorage[slot].SetSlot(new Slot(slot, packetS2F.GetItemStack()));
                isRender = true;
            }
            else if (packet is PacketS30WindowItems packetS30)
            {
                // загрузить все слоты
                ItemStack[] stacks = packetS30.GetStacks();
                if (pageCount == stacks.Length)
                {
                    for (int i = 0; i < pageCount; i++)
                    {
                        buttonStorage[i].SetSlot(new Slot(i, stacks[i]));
                    }
                    isRender = true;
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(PacketC0EPacketClickWindow.EnumAction.Open));
        }

        /// <summary>
        /// Расположение на окне склада
        /// </summary>
        protected override void ResizedScreenStorage(int w, int h)
        {
            int i, x, y, x0, c;
            x = x0 = w + 156 * SizeInterface;
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
        /// Клик по складу
        /// </summary>
        protected override void StorageClick(ButtonSlot button, bool isShift, bool isRight) 
            => ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(
                PacketC0EPacketClickWindow.EnumAction.ClickSlot, isShift, isRight, button.GetSlot().Index + 100));

        /// <summary>
        /// Клик по инвентарю
        /// </summary>
        protected override void InventoryClick(ButtonSlot button, bool isShift, bool isRight) 
            => ClientMain.TrancivePacket(new PacketC0EPacketClickWindow(
                PacketC0EPacketClickWindow.EnumAction.ClickSlot, isShift, isRight, button.GetSlot().Index));

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
    }
}
