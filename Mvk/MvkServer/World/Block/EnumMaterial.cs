namespace MvkServer.World.Block
{
    /// <summary>
    /// Перечень разновидностей материалов блоков
    /// </summary>
    public enum EnumMaterial
    {
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
        /// Твёрдое тело
        /// </summary>
        Solid = 3,
        /// <summary>
        /// Сыпучая порода
        /// </summary>
        Loose = 4,
        /// <summary>
        /// Вода
        /// </summary>
        Water = 5,
        /// <summary>
        /// Лава
        /// </summary>
        Lava = 6,
        /// <summary>
        /// Нефть
        /// </summary>
        Oil = 7,
        /// <summary>
        /// Огонь
        /// </summary>
        Fire = 8,
        /// <summary>
        /// Древесина
        /// </summary>
        Wood = 9,
        /// <summary>
        /// Листва
        /// </summary>
        Leaves = 10,
        /// <summary>
        /// Саженец, трава, цветы
        /// </summary>
        Sapling = 11,
        /// <summary>
        /// Растительный белок 
        /// </summary>
        VegetableProtein = 12,
        /// <summary>
        /// Руда
        /// </summary>
        Ore = 13,
        /// <summary>
        /// Стекло
        /// </summary>
        Glass = 14,
        /// <summary>
        /// Стеклянная панель
        /// </summary>
        GlassPane = 15,
        /// <summary>
        /// Электричество
        /// </summary>
        Electricity = 16,
        /// <summary>
        /// Дверь
        /// </summary>
        Door = 17,
        /// <summary>
        /// Люк
        /// </summary>
        Trapdoor = 18,
        /// <summary>
        /// Кусочек, типа камня
        /// </summary>
        Piece = 19,
        /// <summary>
        /// Интерьер
        /// </summary>
        Interior = 20,
        /// <summary>
        /// Деревяные столы
        /// </summary>
        WoodTable = 21
    }

    /// <summary>
    /// Количество материалов
    /// </summary>
    public class MaterialsCount
    {
        public const int COUNT = 21;
    }
}
