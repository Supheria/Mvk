namespace MvkClient.Actions
{
    /// <summary>
    /// Связь нажатых клавиш
    /// </summary>
    public class KeyBinding
    {
        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        public bool forward = false;
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public bool back = false;
        /// <summary>
        /// Перемещение вправо
        /// </summary>
        public bool right = false;
        /// <summary>
        /// Перемещение влево
        /// </summary>
        public bool left = false;
        /// <summary>
        /// Перемещение вверх
        /// </summary>
        public bool up = false;
        /// <summary>
        /// Перемещение вниз
        /// </summary>
        public bool down = false;
        /// <summary>
        /// Ускорение, только в одну сторону 
        /// </summary>
        public bool sprinting = false;

        /// <summary>
        /// Нет нажатий
        /// </summary>
        public void InputNone()
        {
            forward = false;
            back = false;
            right = false;
            left = false;
            up = false;
            down = false;
            sprinting = false;
        }
    }
}
