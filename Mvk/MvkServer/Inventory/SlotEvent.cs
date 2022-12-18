namespace MvkServer.Inventory
{
    public delegate void SlotEventHandler(object sender, SlotEventArgs e);
    public class SlotEventArgs
    {
        /// <summary>
        /// Объект измёнённого слота
        /// </summary>
        public int IndexSlot { get; private set; }

        public SlotEventArgs(int slot) => IndexSlot = slot;
    }
}
