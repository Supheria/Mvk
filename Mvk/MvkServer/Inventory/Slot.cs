using MvkServer.Item;

namespace MvkServer.Inventory
{
    /// <summary>
    /// Слот инвентаря, для блоков и предметов
    /// </summary>
    public class Slot
    {
        /// <summary>
        /// Индекс слота
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Предмет в контейнере
        /// </summary>
        public ItemBase Item { get; private set; }
        /// <summary>
        /// Количество если 1 не будет прорисован
        /// </summary>
        public int Amount { get; private set; } = 1;

        public Slot(int index = -1, ItemBase item = null, int amount = 1)
        {
            Index = index;
            Amount = amount;
            Item = item;
        }

        public Slot Clone()
        {
            return new Slot(Index, Item, Amount);
        }

        public Slot Clone(int amount)
        {
            return new Slot(Index, Item, amount);
        }

        public void Clear()
        {
            Index = -1;
            Item = null;
            Amount = 1;
        }

        public void Set(ItemBase item = null, int amount = 1)
        {
            Amount = amount;
            Item = item;
        }

        public void SetAmount(int amount) => Amount = amount;

        public void SetIndex(int index) => Index = index;

        public bool Empty() => Item == null;
    }
}
