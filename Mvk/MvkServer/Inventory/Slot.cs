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
        /// Объект одной ячейки
        /// </summary>
        public ItemStack Stack { get; private set; }

        public Slot(int index = -1, ItemStack stack = null)
        {
            Index = index;
            Stack = stack;
        }

        public Slot Clone()
        {
            return new Slot(Index, Stack?.Copy());
        }

        public void Clear()
        {
            Index = -1;
            Stack = null;
        }

        public void Set(ItemStack stack = null) => Stack = stack;

        public void SetIndex(int index) => Index = index;

        public bool Empty() => Stack == null;
    }
}
