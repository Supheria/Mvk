﻿namespace MvkServer.Entity
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
    }
}
