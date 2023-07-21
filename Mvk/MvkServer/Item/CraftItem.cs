namespace MvkServer.Item
{
    /// <summary>
    /// Объект отвечающий за крафт предмета
    /// </summary>
    public class CraftItem
    {
        /// <summary>
        /// Количество сделанного
        /// </summary>
        public int Amount { get; private set; } = 1;
        /// <summary>
        /// Время крафта в тиках (20 тиков = 1 сек)
        /// </summary>
        public int CraftTime { get; private set; } = 0;
        /// <summary>
        /// Рецепт крафта, массив требуемых предметов
        /// </summary>
        public Element[] CraftRecipe { get; private set; } = new Element[0];

        /// <summary>
        /// Задать время крафта
        /// </summary>
        public CraftItem SetTime(int time)
        {
            CraftTime = time;
            return this;
        }

        /// <summary>
        /// Количество сделанного
        /// </summary>
        public CraftItem SetAmount(int amount)
        {
            Amount = amount;
            return this;
        }

        /// <summary>
        /// Задать рецепт
        /// </summary>
        public CraftItem SetRecipe(params Element[] recipe)
        {
            CraftRecipe = recipe;
            return this;
        }
        /// <summary>
        /// Задать рецепт
        /// </summary>
        public CraftItem SetRecipe(int time, params Element[] recipe)
        {
            CraftTime = time;
            CraftRecipe = recipe;
            return this;
        }
        /// <summary>
        /// Задать рецепт
        /// </summary>
        public CraftItem SetRecipe(int amount, int time, params Element[] recipe)
        {
            Amount = amount;
            CraftTime = time;
            CraftRecipe = recipe;
            return this;
        }
    }
}
