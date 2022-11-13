namespace MvkServer.Item
{
    /// <summary>
    /// Тип предмета
    /// </summary>
    public enum EnumItem
    {
        /// <summary>
        /// Блок
        /// </summary>
        Block = 0,
        /// <summary>
        /// Яблоко
        /// </summary>
        Apple = 1,
        /// <summary>
        /// Дверь дубовая
        /// </summary>
        DoorOak = 2,
        /// <summary>
        /// Дверь берёзовая
        /// </summary>
        DoorBirch = 3,
        /// <summary>
        /// Дверь еловая
        /// </summary>
        DoorSpruce = 4,
        /// <summary>
        /// Дверь плодовая
        /// </summary>
        DoorFruit = 5,
        /// <summary>
        /// Дверь из пальмы
        /// </summary>
        DoorPalm = 6,
        /// <summary>
        /// Дверь железная
        /// </summary>
        DoorIron = 7,
        /// <summary>
        /// Люк дубовый
        /// </summary>
        TrapdoorOak = 8,
        /// <summary>
        /// Люк берёзовый
        /// </summary>
        TrapdoorBirch = 9,
        /// <summary>
        /// Люк еловый
        /// </summary>
        TrapdoorSpruce = 10,
        /// <summary>
        /// Люк плодовый
        /// </summary>
        TrapdoorFruit = 11,
        /// <summary>
        /// Люк из пальмы
        /// </summary>
        TrapdoorPalm = 12,
        /// <summary>
        /// Люк железный
        /// </summary>
        TrapdoorIron = 13,
        /// <summary>
        /// Шлем
        /// </summary>
        Helmet = 14,
        /// <summary>
        /// Нагрудник
        /// </summary>
        Chestplate = 15,
        /// <summary>
        /// Штаны 
        /// </summary>
        Leggings = 16,
        /// <summary>
        /// Сапоги
        /// </summary>
        Boots = 17,
        /// <summary>
        /// Ведро
        /// </summary>
        Bucket = 18,
        /// <summary>
        /// Ведро с водой
        /// </summary>
        BucketWater = 19,
        /// <summary>
        /// Ведро с лавой
        /// </summary>
        BucketLava = 20,
        /// <summary>
        /// Ведро с нефтью
        /// </summary>
        BucketOil = 21,
        /// <summary>
        /// Кремень и сталь
        /// </summary>
        FlintAndSteel = 22,
        /// <summary>
        /// Топор
        /// </summary>
        Axe = 23,
        /// <summary>
        /// Лопата
        /// </summary>
        Shovel = 24,
        /// <summary>
        /// Кирка
        /// </summary>
        Pickaxe = 25,
        /// <summary>
        /// Земляной кусочек
        /// </summary>
        PieceDirt = 26

    }

    /// <summary>
    /// Количество предметов
    /// </summary>
    public class ItemsCount
    {
        public const int COUNT = 26;
    }
}
