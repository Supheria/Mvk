namespace MvkServer.Util
{
    /// <summary>
    /// Перечень источников урона
    /// </summary>
    public enum EnumDamageSource
    {
        /// <summary>
        /// Лава
        /// </summary>
        Lava,
        /// <summary>
        /// В стене
        /// </summary>
        InWall,
        /// <summary>
        /// Тонем
        /// </summary>
        Drown,
        /// <summary>
        /// Падение
        /// </summary>
        Fall,
        /// <summary>
        /// Вне мира
        /// </summary>
        OutOfWorld,
        /// <summary>
        /// В огне, когда зашёл в него
        /// </summary>
        InFire,
        /// <summary>
        /// От огня, когда долго горел из-за огня
        /// </summary>
        OnFire,
        /// <summary>
        /// Соприкосновение с кактусом
        /// </summary>
        Cactus,
        /// <summary>
        /// Источник взрыва
        /// </summary>
        ExplosionSource,
        /// <summary>
        /// Моб
        /// </summary>
        Mob,
        /// <summary>
        /// Игрок
        /// </summary>
        Player

    }
}
