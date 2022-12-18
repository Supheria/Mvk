namespace MvkServer.Util
{
    /// <summary>
    /// Объект параметров перемещения
    /// </summary>
    public class MovementInput
    {
        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        public bool Forward { get; protected set; } = false;
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public bool Back { get; protected set; } = false;
        /// <summary>
        /// Шаг влево
        /// </summary>
        public bool Left { get; protected set; } = false;
        /// <summary>
        /// Шаг вправо
        /// </summary>
        public bool Right { get; protected set; } = false;
        /// <summary>
        /// Прыжок
        /// </summary>
        public bool Jump { get; protected set; } = false;
        /// <summary>
        /// Присесть
        /// </summary>
        public bool Sneak { get; protected set; } = false;
        /// <summary>
        /// Ускорение
        /// </summary>
        public bool Sprinting { get; protected set; } = false;
        /// <summary>
        /// Коэффициент ускорения вперёд, для мобов
        /// </summary>
        public float Speed { get; protected set; } = 1;

        /// <summary>
        /// Управление игроком
        /// </summary>
        public virtual void UpdatePlayerMoveState() { }
        /// <summary>
        /// Задать управление мобом
        /// </summary>
        public void SetAIMoveState(bool forward, bool jump, bool sneak, bool sprinting)
        {
            Back = Left = Right = false;
            Forward = forward;
            Jump = jump;
            Sneak = sneak;
            Sprinting = sprinting;
        }

        /// <summary>
        /// Задать перемещение вперёд мобом
        /// </summary>
        public void SetAIForward(float speed)
        {
            Forward = true;
            Speed = speed;
        }

        /// <summary>
        /// Задать прыжок мобом
        /// </summary>
        public void SetAIJamp() => Jump = true;

        /// <summary>
        /// Задать присесть мобом
        /// </summary>
        public void SetAISneak() => Sneak = true;

        /// <summary>
        /// Задать ускорение мобом
        /// </summary>
        public void SetAISprinting() => Sprinting = true;

        /// <summary>
        /// Задать остановиться мобом
        /// </summary>
        public void SetAIStop() => Forward = Sprinting = Sneak = Jump = Back = Left = Right = false;

        /// <summary>
        /// Перемещение шага влево -1.0 .. +1.0 право
        /// </summary>
        public float GetMoveStrafe() => (Right ? 1f : 0) - (Left ? 1f : 0);

        /// <summary>
        /// Перемещение назад 1.0 .. -1.0 вперёд
        /// </summary>
        public float GetMoveForward() => (Back ? 1f : 0f) - (Forward ? Speed : 0f);

        /// <summary>
        /// Перемещение вертикали вверх 1.0 .. -1.0 вниз
        /// </summary>
        public float GetMoveVertical() => (Jump ? 1f : 0f) - (Sneak ? 1f : 0);
    }
}
