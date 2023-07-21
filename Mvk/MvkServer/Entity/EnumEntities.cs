namespace MvkServer.Entity
{
    /// <summary>
    /// Перечень разновидностей сущностей
    /// </summary>
    public enum EnumEntities
    {
        None = 0,
        /// <summary>
        /// Игрок
        /// </summary>
        Player = 1,
        /// <summary>
        /// Рука игрока
        /// </summary>
        PlayerHand = 2,
        /// <summary>
        /// Курица
        /// </summary>
        Chicken = 3,
        /// <summary>
        /// Предмет
        /// </summary>
        Item = 4,
        /// <summary>
        /// Предмет кусочек
        /// </summary>
        Piece = 5,
        /// <summary>
        /// Чемоглот
        /// </summary>
        Chemoglot = 6,
        /// <summary>
        /// Пакан
        /// </summary>
        Pakan = 7,
        /// <summary>
        /// Книга
        /// </summary>
        Book = 8,
        /// <summary>
        /// Игрок в режиме невидимки
        /// </summary>
        PlayerInvisible = 9

    }

    /// <summary>
    /// Количество разновидностей сущностей
    /// </summary>
    public class EntitiesCount
    {
        public const int COUNT = 9;
    }
}
