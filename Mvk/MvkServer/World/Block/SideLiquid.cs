using MvkServer.Util;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Сторона жидкого блока
    /// </summary>
    public struct SideLiquid
    {
        /// <summary>
        /// Смещение текстуры в карте
        /// </summary>
        public float u;
        /// <summary>
        /// Смещение текстуры в карте
        /// </summary>
        public float v;
        /// <summary>
        /// С какой стороны
        /// </summary>
        public int side;
        /// <summary>
        /// Боковое затемнение
        /// </summary>
        public float lightPole;
        /// <summary>
        /// Для анимации блока, указывается количество кадров в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 - нет анимации
        /// </summary>
        public byte animationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 или 1 - нет задержки, каждый такт игры смена кадра
        /// </summary>
        public byte animationPause;

        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода
        /// </summary>
        private readonly byte biomeColor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pole">индекс стороны</param>
        /// <param name="biomeColor">Задать цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода</param>
        /// <param name="frame">количество кадров в игровом времени</param>
        /// <param name="pause">пауза между кадрами в игровом времени</param>
        public SideLiquid(int pole, int numberTexture, byte biomeColor, byte frame = 0, byte pause = 0)
        {
            side = pole;
            lightPole = 1f - MvkStatic.LightPoles[pole];
            u = (numberTexture % 64) * .015625f;
            v = numberTexture / 64 * .015625f;
            this.biomeColor = biomeColor;
            animationFrame = frame;
            animationPause = pause;
            side = 0;
        }

        /// <summary>
        /// Имеется ли у блока смена цвета воды от биома
        /// </summary>
        public bool IsBiomeColorWater() => biomeColor == 3;
    }
}
