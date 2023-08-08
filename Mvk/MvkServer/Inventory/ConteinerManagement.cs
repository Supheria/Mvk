using MvkServer.Item;

namespace MvkServer.Inventory
{
    /// <summary>
    /// Управление контейнером, задача для переноса пачками, через Shift
    /// </summary>
    public class ConteinerManagement
    {
        /// <summary>
        /// Самый максимальный размер. Даже если значение ItemBase.MaxStackSize будет больше,
        /// обрежет по INVENTORY_STACK_LIMIT
        /// </summary>
        public const int INVENTORY_STACK_LIMIT = 255;

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно. С проверкой на креатив
        /// </summary>
        public bool AddItemStackToInventory(ItemStack[] stacks, ItemStack stack, bool isCreativeMode = false)
        {
            if (stack != null && stack.Amount != 0 && stack.Item != null)
            {
                if (stack.IsItemDamaged())
                {
                    int slot = GetFirstEmptyStack(stacks);

                    if (slot >= 0)
                    {
                        stacks[slot] = stack.Copy();
                        //mainInventory[slot].animationsToGo = 5;
                        stack.Zero();
                        OnSendSetSlot(slot, stacks[slot]);
                        return true;
                    }
                    else if (isCreativeMode)
                    {
                        stack.Zero();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    int amount;
                    do
                    {
                        amount = stack.Amount;
                        stack.SetAmount(StorePartialItemStack(stacks, stack));
                    }
                    while (stack.Amount > 0 && stack.Amount < amount);

                    if (stack.Amount == amount && isCreativeMode)
                    {
                        stack.Zero();
                        return true;
                    }
                    else
                    {
                        return stack.Amount < amount;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Возвращает первый пустой стек элементов
        /// -1 нет пустого
        /// </summary>
        private int GetFirstEmptyStack(ItemStack[] stacks)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                if (stacks[i] == null) return i;
            }
            return -1;
        }

        /// <summary>
        /// Эта функция сохраняет как можно больше элементов ItemStack в соответствующем 
        /// слоте и возвращает количество оставшихся элементов.
        /// </summary>
        private int StorePartialItemStack(ItemStack[] stacks, ItemStack itemStack)
        {
            ItemBase item = itemStack.Item;
            int amount = itemStack.Amount;
            int slot = StoreItemStack(stacks, itemStack);

            if (slot < 0) slot = GetFirstEmptyStack(stacks);

            if (slot < 0)
            {
                return amount;
            }
            else
            {
                if (stacks[slot] == null)
                {
                    stacks[slot] = new ItemStack(item, 0, itemStack.ItemDamage);
                }

                int amount2 = amount;

                if (amount > stacks[slot].GetMaxStackSize() - stacks[slot].Amount)
                {
                    amount2 = stacks[slot].GetMaxStackSize() - stacks[slot].Amount;
                }

                if (amount2 > INVENTORY_STACK_LIMIT - stacks[slot].Amount)
                {
                    amount2 = INVENTORY_STACK_LIMIT - stacks[slot].Amount;
                }

                if (amount2 == 0)
                {
                    return amount;
                }
                else
                {
                    amount -= amount2;
                    stacks[slot].AddAmount(amount2);
                    OnSendSetSlot(slot, stacks[slot]);
                    //mainInventory[slot].animationsToGo = 5;
                    return amount;
                }
            }
        }

        /// <summary>
        /// Находим слот с таким же стакам, где можно ещё что-то засунуть. Типа не полный
        /// </summary>
        private int StoreItemStack(ItemStack[] stacks, ItemStack itemStack)
        {
            int count = stacks.Length;
            ItemStack stack;
            for (int i = 0; i < count; i++)
            {
                stack = stacks[i];
                if (stack != null && stack.Item.Id == itemStack.Item.Id
                    && stack.IsStackable() && stack.Amount < stack.GetMaxStackSize()
                    && stack.Amount < INVENTORY_STACK_LIMIT
                    && stack.ItemDamage == itemStack.ItemDamage
                    && ItemStack.AreItemsEqual(stack, itemStack))
                {
                    return i;
                }
            }
            return -1;
        }

        public event StackEventHandler SendSetSlot;
        private void OnSendSetSlot(int slot, ItemStack stack) => OnSendSetSlot(new StackEventArgs(slot, stack));
        private void OnSendSetSlot(StackEventArgs e) => SendSetSlot?.Invoke(this, e);
    }
}
