using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Network.Packets.Client;

namespace MvkClient.Gui
{
    /// <summary>
    /// У гульні запушчана меню кантэйнера
    /// </summary>
    public class ScreenConteinerCreative : ScreenConteinerItems
    {
        public ScreenConteinerCreative(Client client) : base(client)
        {
            textTitle = "gui.conteiner.inventory";
            pageCount = 50;
        }

        protected override void Init()
        {
            base.Init();
            SetArrayItems(Items.inventory);
        }

        /// <summary>
        /// Клик по складу
        /// </summary>
        protected override void StorageClick(ButtonSlot button, bool isShift, bool isRight)
        {
            // количество для руки
            int amount;
            if (button.GetSlot().Empty())
            {
                // удалить что в руке
                // что-то в руке есть но кликнули на другой
                amount = (!theSlot.Empty() && isRight) ? theSlot.Stack.Amount - 1 : 0;
            }
            else
            {
                // клик на что-то
                if (theSlot.Empty())
                {
                    // Берём предмет
                    amount = isShift ? button.GetSlot().Stack.Item.MaxStackSize : 1;
                }
                else if (theSlot.Stack.Item.Id == button.GetSlot().Stack.Item.Id)
                {
                    // Кликнули на тоже самое, что в руке
                    amount = theSlot.Stack.Amount + (isRight ? -1 : (isShift ? button.GetSlot().Stack.Item.MaxStackSize : 1));
                    if (amount > theSlot.Stack.Item.MaxStackSize) amount = theSlot.Stack.Item.MaxStackSize;
                }
                else
                {
                    // что-то в руке есть но кликнули на другой
                    amount = isRight ? theSlot.Stack.Amount - 1 : 0;
                }
            }

            if (amount <= 0) theSlot.Clear();
            else
            {
                theSlot.Set(theSlot.Empty() ? button.GetSlot().Stack.Copy() : theSlot.Stack);
                theSlot.Stack.SetAmount(amount);
            }

            icon.PereRender();
        }

        /// <summary>
        /// Клик по инвентарю
        /// </summary>
        protected override void InventoryClick(ButtonSlot button, bool isShift, bool isRight)
        {
            // количество для руки
            int amount;
            ItemStack stack = theSlot.Stack;
            Slot slot = button.GetSlot().Clone();
            bool isSlot = false;
            if (button.GetSlot().Empty())
            {
                // удалить что в руке
                // что-то в руке есть но кликнули на другой
                amount = (!theSlot.Empty() && isRight) ? theSlot.Stack.Amount - 1 : 0;

                if (!theSlot.Empty())
                {
                    // Оставляем в пустую ячейку или одну или все
                    slot.Set(theSlot.Stack.Copy());
                    slot.Stack.SetAmount(isRight ? 1 : theSlot.Stack.Amount);
                    isSlot = true;
                }
            }
            else
            {
                if (isShift)
                {
                    // Клик на предмет, убираем его не важно что в руке, оставляя предмет в руке
                    slot.Set(null);
                    amount = theSlot.Empty() ? 0 : theSlot.Stack.Amount;
                }
                // клик на что-то
                else if (theSlot.Empty())
                {
                    // Берём предмет
                    amount = button.GetSlot().Stack.Amount;
                    stack = button.GetSlot().Stack.Copy();
                    if (isRight) amount /= 2;
                    slot.Stack.SetAmount(button.GetSlot().Stack.Amount - amount);
                }
                else if (theSlot.Stack.Item.Id == button.GetSlot().Stack.Item.Id)
                {
                    // Кликнули на тоже самое, что в руке

                    // сумма слотов
                    int aw = theSlot.Stack.Amount + button.GetSlot().Stack.Amount;
                    if (aw > theSlot.Stack.Item.MaxStackSize)
                    {
                        // сумма больше слота
                        slot.Stack.SetAmount(slot.Stack.Item.MaxStackSize);
                        amount = aw - slot.Stack.Amount;
                    }
                    else
                    {
                        // сумма меньше слота
                        slot.Stack.SetAmount(aw);
                        amount = 0;
                    }
                }
                else
                {
                    // что-то в руке есть но кликнули на другой, заменяем местами
                    slot.Set(theSlot.Stack);
                    stack = button.GetSlot().Stack;
                    amount = stack.Amount;
                }
                isSlot = true;
            }

            // Корректировка для слота руки
            if (amount <= 0) theSlot.Clear();
            else
            {
                stack.SetAmount(amount);
                theSlot.Set(stack.Copy());
            }
            icon.PereRender();

            if (isSlot)
            {
                // Если надо корректировка для слота инвентаря
                if (slot.Empty() || slot.Stack.Amount <= 0) slot.Set(null);
                button.SetSlot(slot);
                // Отправляем на сервер
                ClientMain.TrancivePacket(new PacketC10CreativeInventoryAction(slot.Index, slot.Stack ?? new ItemStack()));
            }
        }

        /// <summary>
        /// Выбросить слот который в руке
        /// </summary>
        protected override void ThrowTheSlot()
        {
            if (!theSlot.Empty())
            {
                ClientMain.TrancivePacket(new PacketC10CreativeInventoryAction(-1, theSlot.Stack));
                theSlot.Clear();
                icon.PereRender();
            }
        }
    }
}
