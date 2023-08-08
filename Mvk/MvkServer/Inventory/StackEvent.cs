using MvkServer.Item;

namespace MvkServer.Inventory
{
    public delegate void StackEventHandler(object sender, StackEventArgs e);
    public class StackEventArgs
    {
        /// <summary>
        /// Индекс слота
        /// </summary>
        public int Slot { get; private set; }
        /// <summary>
        /// Объект нового слота
        /// </summary>
        public ItemStack Stack { get; private set; }

        public StackEventArgs(int slot, ItemStack stack)
        {
            Slot = slot;
            Stack = stack;
        }
    }
}
