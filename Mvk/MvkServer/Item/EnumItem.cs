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
        /// Топор железный
        /// </summary>
        AxeIron = 23,
        /// <summary>
        /// Лопата железная
        /// </summary>
        ShovelIron = 24,
        /// <summary>
        /// Кирка железная
        /// </summary>
        PickaxeIron = 25,
        /// <summary>
        /// Земляной кусочек
        /// </summary>
        PieceDirt = 26,
        /// <summary>
        /// Каменный кусочек
        /// </summary>
        PieceStone = 27,
        /// <summary>
        /// Спавн курицы
        /// </summary>
        SpawnChicken = 28,
        /// <summary>
        /// Маленькая взрывчатка
        /// </summary>
        ExplosivesMin = 29,
        /// <summary>
        /// Взрывчатка
        /// </summary>
        Explosives = 30,
        /// <summary>
        /// Большая взрывчатка
        /// </summary>
        ExplosivesMax = 31,
        /// <summary>
        /// Спавн чемоглота
        /// </summary>
        SpawnChemoglot = 32,
        /// <summary>
        /// Спавн пакана
        /// </summary>
        SpawnPakan = 33,
        /// <summary>
        /// Яйцо
        /// </summary>
        Egg = 34,
        /// <summary>
        /// Песчаный кусочек
        /// </summary>
        PieceSand = 35,
        /// <summary>
        /// Песчаник кусочек
        /// </summary>
        PieceSandstone = 36,
        /// <summary>
        /// Глина кусочек
        /// </summary>
        PieceClay = 37,
        /// <summary>
        /// Керамики кусочек
        /// </summary>
        PieceTerracotta = 38,
        /// <summary>
        /// Базальт кусочек
        /// </summary>
        PieceBasalt = 39,
        /// <summary>
        /// Известняк кусочек
        /// </summary>
        PieceLimestone = 40,
        /// <summary>
        /// Гравий кусочек
        /// </summary>
        PieceGravel = 41,
        /// <summary>
        /// Гранит кусочек
        /// </summary>
        PieceGranite = 42,
        /// <summary>
        /// Спавн книги
        /// </summary>
        SpawnBook = 43,
        /// <summary>
        /// Высокая трава
        /// </summary>
        TallGrass = 44,
        /// <summary>
        /// Кокос
        /// </summary>
        Coconut = 45,
        /// <summary>
        /// Половина кокоса
        /// </summary>
        HalfCoconut = 46,
        /// <summary>
        /// Топор стальной
        /// </summary>
        AxeSteel = 47,
        /// <summary>
        /// Топор каменный
        /// </summary>
        AxeStone = 48,
        /// <summary>
        /// Палка-копалка
        /// </summary>
        DiggingStick = 49,
        /// <summary>
        /// Лопата стальная
        /// </summary>
        ShovelSteel = 50,
        /// <summary>
        /// Палка
        /// </summary>
        Stick = 51,
        /// <summary>
        /// Щепа
        /// </summary>
        WoodChips = 52,
        /// <summary>
        /// Сухая трава
        /// </summary>
        DryGrass = 53,
        /// <summary>
        /// Кирка стальная
        /// </summary>
        PickaxeSteel = 54,
        /// <summary>
        /// Столярный верстак
        /// </summary>
        CraftingTableCarpentry = 55,
        /// <summary>
        /// Стол инструментальщика
        /// </summary>
        ToolmakerTable = 56
    }

    /// <summary>
    /// Количество предметов
    /// </summary>
    public class ItemsCount
    {
        public const int COUNT = 56;
    }
}
