namespace MvkServer.Util
{
    /// <summary>
    /// Варианты типов окон для открытия во время игры
    /// </summary>
    public enum EnumWindowType
    {
        /// <summary>
        /// Нет окна, такого не должно быть
        /// </summary>
        None = 0,
        /// <summary>
        /// Окно первого крафта
        /// </summary>
        CraftFirst = 1,
        /// <summary>
        /// Окно столярного верстака
        /// </summary>
        Carpentry = 2,
        /// <summary>
        /// Окно инструментальщика
        /// </summary>
        Toolmaker = 3,
        /// <summary>
        /// Окно ящика
        /// </summary>
        Box = 4

    }
}