namespace MvkServer.World.Block
{
    /// <summary>
    /// Тип блока
    /// </summary>
    public enum EnumBlock
    {
        /// <summary>
        /// Отсутствие блока, он же воздух, но с коллизией, пройти через него нельзя
        /// </summary>
        None = -1,
        /// <summary>
        /// Воздух
        /// </summary>
        Air = 0,
        /// <summary>
        /// Отладочный блок
        /// </summary>
        Debug = 1,
        /// <summary>
        /// Коренная порода
        /// </summary>
        Bedrock = 2,
        /// <summary>
        /// Камень
        /// </summary>
        Stone = 3,
        /// <summary>
        /// Булыжник
        /// </summary>
        Cobblestone = 4,
        /// <summary>
        /// Известняк
        /// </summary>
        Limestone = 5,
        /// <summary>
        /// Гранит 
        /// </summary>
        Granite = 6,
        /// <summary>
        /// Песчаник
        /// </summary>
        Sandstone = 7,
        /// <summary>
        /// Земля
        /// </summary>
        Dirt = 8,
        /// <summary>
        /// Дёрн
        /// </summary>
        Turf = 9,
        /// <summary>
        /// Песок
        /// </summary>
        Sand = 10,
        /// <summary>
        /// Гравий
        /// </summary>
        Gravel = 11,
        /// <summary>
        /// Глина
        /// </summary>
        Clay = 12,
        /// <summary>
        /// Вода
        /// </summary>
        Water = 13,
        /// <summary>
        /// Текучая вода
        /// </summary>
        WaterFlowing = 14,
        /// <summary>
        /// Лава
        /// </summary>
        Lava = 15,
        /// <summary>
        /// Текучая лава
        /// </summary>
        LavaFlowing = 16,
        /// <summary>
        /// Нефть
        /// </summary>
        Oil = 17,
        /// <summary>
        /// Текучая нефть
        /// </summary>
        OilFlowing = 18,
        /// <summary>
        /// Огонь
        /// </summary>
        Fire = 19,
        /// <summary>
        /// Бревно дуб
        /// </summary>
        LogOak = 20,
        /// <summary>
        /// Бревно берёза
        /// </summary>
        LogBirch = 21,
        /// <summary>
        /// Бревно ель
        /// </summary>
        LogSpruce = 22,
        /// <summary>
        /// Бревно плодовое
        /// </summary>
        LogFruit = 23,
        /// <summary>
        /// Бревно пальмы
        /// </summary>
        LogPalm = 24,
        /// <summary>
        /// Доски дуб
        /// </summary>
        PlanksOak = 25,
        /// <summary>
        /// Доски берёза
        /// </summary>
        PlanksBirch = 26,
        /// <summary>
        /// Доски ель
        /// </summary>
        PlanksSpruce = 27,
        /// <summary>
        /// Доски плодовое
        /// </summary>
        PlanksFruit = 28,
        /// <summary>
        /// Доски пальмы
        /// </summary>
        PlanksPalm = 29,
        /// <summary>
        /// Листва дуб
        /// </summary>
        LeavesOak = 30,
        /// <summary>
        /// Листва берёза
        /// </summary>
        LeavesBirch = 31,
        /// <summary>
        /// Листва ель
        /// </summary>
        LeavesSpruce = 32,
        /// <summary>
        /// Листва плодовое
        /// </summary>
        LeavesFruit = 33,
        /// <summary>
        /// Листва пальмы
        /// </summary>
        LeavesPalm = 34,
        /// <summary>
        /// Саженец дуб
        /// </summary>
        SaplingOak = 35,
        /// <summary>
        /// Саженец берёза
        /// </summary>
        SaplingBirch = 36,
        /// <summary>
        /// Саженец ель
        /// </summary>
        SaplingSpruce = 37,
        /// <summary>
        /// Саженец плодовое
        /// </summary>
        SaplingFruit = 38,
        /// <summary>
        /// Саженец пальмы
        /// </summary>
        SaplingPalm = 39,
        /// <summary>
        /// Кактус
        /// </summary>
        Cactus = 40,
        /// <summary>
        /// Угольная руда
        /// </summary>
        OreCoal = 41,
        /// <summary>
        /// Железная руда
        /// </summary>
        OreIron = 42,
        /// <summary>
        /// Золотая руда
        /// </summary>
        OreGold = 43,
        /// <summary>
        /// Брол
        /// </summary>
        Brol = 44,
        /// <summary>
        /// Блок травы
        /// </summary>
        Grass = 45,
        /// <summary>
        /// Цветок одуванчик
        /// </summary>
        FlowerDandelion = 46,
        /// <summary>
        /// Цветок клевер белый
        /// </summary>
        FlowerClover = 47,
        /// <summary>
        /// Стекло прозрачное
        /// </summary>
        Glass = 48,
        /// <summary>
        /// Стекло белое
        /// </summary>
        GlassWhite = 49,
        /// <summary>
        /// Стекло красное
        /// </summary>
        GlassRed = 50,
        /// <summary>
        /// Стеклянная панель прозрачная
        /// </summary>
        GlassPane = 51,
        /// <summary>
        /// Стеклянная панель белая
        /// </summary>
        GlassPaneWhite = 52,
        /// <summary>
        /// Стеклянная панель красная
        /// </summary>
        GlassPaneRed = 53,
        /// <summary>
        /// Керамика
        /// </summary>
        Terracotta = 54,
        /// <summary>
        /// Базальт
        /// </summary>
        Basalt = 55,
        /// <summary>
        /// Обсидиан
        /// </summary>
        Obsidian = 56,
        /// <summary>
        /// Электричество Провод
        /// </summary>
        ElWire = 57,
        /// <summary>
        /// Электричество Лампа включена
        /// </summary>
        ElLampOn = 58,
        /// <summary>
        /// Электричество Лампа выключена
        /// </summary>
        ElLampOff = 59,
        /// <summary>
        /// Электричество Батарея
        /// </summary>
        ElBattery = 60,
        /// <summary>
        /// Электричество Выключатель
        /// </summary>
        ElSwitch = 61,
        /// <summary>
        /// TNT
        /// </summary>
        Tnt = 62,
        /// <summary>
        /// TNT с часами
        /// </summary>
        TntClock = 63,
        /// <summary>
        /// Дверь дубовая
        /// </summary>
        DoorOak = 64,
        /// <summary>
        /// Дверь берёзовая
        /// </summary>
        DoorBirch = 65,
        /// <summary>
        /// Дверь еловая
        /// </summary>
        DoorSpruce = 66,
        /// <summary>
        /// Дверь плодовая
        /// </summary>
        DoorFruit = 67,
        /// <summary>
        /// Дверь из пальмы
        /// </summary>
        DoorPalm = 68,
        /// <summary>
        /// Дверь железная
        /// </summary>
        DoorIron = 69,
        /// <summary>
        /// Маленький камень
        /// </summary>
        SmallStone = 70,
        /// <summary>
        /// Гнездо
        /// </summary>
        Nest = 71,
        /// <summary>
        /// Тина
        /// </summary>
        Tina = 72,
        /// <summary>
        /// Высокая трава
        /// </summary>
        TallGrass = 73,
        /// <summary>
        /// Кокос
        /// </summary>
        Coconut = 74,
        /// <summary>
        /// Яблоко
        /// </summary>
        Apple = 75,
        /// <summary>
        /// Столярный верстак
        /// </summary>
        CraftingTableCarpentry = 76,
        /// <summary>
        /// Стол инструментальщика
        /// </summary>
        ToolmakerTable = 77
    }

    /// <summary>
    /// Количество блоков
    /// </summary>
    public class BlocksCount
    {
        public const int COUNT = 77;
    }
}
