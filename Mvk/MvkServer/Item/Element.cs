using MvkServer.World.Block;

namespace MvkServer.Item
{
    /// <summary>
    /// Элемент для рецепта крафта
    /// </summary>
    public struct Element
    {
        /// <summary>
        /// ID предмета
        /// </summary>
        private readonly int idItem;
        /// <summary>
        /// Количество предметов
        /// </summary>
        private readonly int amount;

        public Element(int idItem, int amount = 1)
        {
            this.idItem = idItem;
            this.amount = amount;
        }

        public Element(EnumItem eItem, int amount = 1)
        {
            idItem = (int)eItem + 5000;
            this.amount = amount;
        }

        public Element(EnumBlock eBlock, int amount = 1)
        {
            idItem = (int)eBlock;
            this.amount = amount;
        }

        /// <summary>
        /// Получить количество
        /// </summary>
        public int GetAmount() => amount;
        /// <summary>
        /// Получить id предмета
        /// </summary>
        public int GetIdItem() => idItem;
        /// <summary>
        /// Получить объект предмета
        /// </summary>
        public ItemBase GetItem() => Items.GetItemCache(idItem);
    }
}
