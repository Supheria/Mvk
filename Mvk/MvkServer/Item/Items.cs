using MvkServer.Item.List;
using MvkServer.World.Block;

namespace MvkServer.Item
{
    /// <summary>
    /// Перечень предметов
    /// </summary>
    public class Items
    {
        /// <summary>
        /// Массив всех кэш предметов
        /// </summary>
        private static ItemBase[] itemsInt;
        /// <summary>
        /// Массив всех кэш предметов блоков
        /// </summary>
        private static ItemBase[] itemsBlockInt;
        /// <summary>
        /// Массив всех предметов которые попадают в креативный инвентарь 
        /// </summary>
        public static int[] inventory;

        private static ItemBase ToItem(EnumItem eItem, BlockBase block = null)
        {
            switch (eItem)
            {
                case EnumItem.Block: return new ItemBlock(block);
                case EnumItem.Apple: return new ItemApple();
                case EnumItem.PieceDirt: return new ItemPieceDirt();
            }
            
            return null;
        }

        /// <summary>
        /// Инициализировать все предметы для кэша
        /// </summary>
        public static void Initialized()
        {
            int countBlock = BlocksCount.COUNT + 1;
            int countItem = ItemsCount.COUNT + 1;

            itemsInt = new ItemBase[countItem];
            itemsBlockInt = new ItemBase[countBlock];

            for (int i = 1; i < countBlock; i++)
            {
                BlockBase block = Blocks.GetBlockCache(i);
                ItemBase item = ToItem(EnumItem.Block, block);
                itemsBlockInt[i] = item;
            }
            for (int i = 1; i < countItem; i++)
            {
                ItemBase item = ToItem((EnumItem)i);
                itemsInt[i] = item;
            }

            inventory = new int[]
            {
                2, 3, 4, 5, 6, 7, 8, 9, 10, 11,
                20, 21, 22, 23, 24, 12, 13, 15, 17, 19,
                25, 26, 27, 28, 29, 40, 41, 42, 43, 44,
                30, 31, 32, 33, 34, 45, 46, 47, 48, 49,
                35, 36, 37, 38, 39, 50, 51, 52, 53, 54,
                55, 56, 57, 59, 60, 61, 62, 63, 0, 0,
                4097, 4098, 4122
            };
        }

        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных НЕ БЛОК!!!
        /// </summary>
        public static ItemBase GetItemCache(EnumItem eItem) => itemsInt[(int)eItem];
        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных
        /// </summary>
        public static ItemBase GetItemCache(int index) 
            => index < 4096 ? itemsBlockInt[index] : itemsInt[index - 4096];
    }
}
