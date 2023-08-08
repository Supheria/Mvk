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
        /// Нужен ли инструмент для крафта
        /// </summary>
        public bool IsToolForCraft { get; private set; } = false;
        /// <summary>
        /// Перечень инструментов с которым можно сделать
        /// </summary>
        public EnumItem[] EnumItemTools { get; private set; } = new EnumItem[0];

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

        /// <summary>
        /// Задать требуемые инструменты
        /// </summary>
        public CraftItem SetTools(params EnumItem[] tools)
        {
            IsToolForCraft = true;
            EnumItemTools = tools;
            return this;
        }

        /// <summary>
        /// Проверить может ли этот предмет скрафтится, с enumItemTool инструментом
        /// </summary>
        public bool CheckTool(ItemStack itemStackTool)
        {
            if (IsToolForCraft)
            {
                if (itemStackTool != null && itemStackTool.Item != null)
                {
                    EnumItem enumItem = itemStackTool.Item.EItem;
                    foreach (EnumItem tool in EnumItemTools)
                    {
                        if (tool == enumItem) return true;
                    }
                }
                return false;
            }
            return true;
        }
    }
}
