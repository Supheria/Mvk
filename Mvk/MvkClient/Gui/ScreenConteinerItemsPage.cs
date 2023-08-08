using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    /// <summary>
    /// Абстрактный класс контейнера с ячейками и закладками
    /// </summary>
    public abstract class ScreenConteinerItemsPage : ScreenConteinerItems
    {
        protected Label labelPage;
        protected ButtonBox buttonBack;
        protected ButtonBox buttonNext;

        /// <summary>
        /// Массив предметов склада
        /// </summary>
        private int[] arrayItems = new int[0];
        /// <summary>
        /// Количество предметов склада
        /// </summary>
        private int countItemsStorage = 0;

        /// <summary>
        /// Бягучая старонка
        /// </summary>
        private int page = 0;

        public ScreenConteinerItemsPage(Client client) : base(client)
        {
            textTitle = "gui.conteiner.inventory";
        }

        /// <summary>
        /// Задать массив склада предметов
        /// </summary>
        protected void SetArrayItems(int[] array)
        {
            countItemsStorage = array.Length;
            arrayItems = array;

            UpdateButtonStorage(0);
        }

        protected override void Init()
        {
            base.Init();
            
            labelPage = new Label("", FontSize.Font12) { Alight = EnumAlight.Center, Width = 50 };
            AddControls(labelPage);
            
            buttonBack = new ButtonBox("<") { Enabled = false };
            buttonBack.Click += (sender, e) => ButtonBackNextClick(false);
            AddControls(buttonBack);
            buttonNext = new ButtonBox(">");
            buttonNext.Click += (sender, e) => ButtonBackNextClick(true);
            AddControls(buttonNext);

            PageUpdate();
        }

        /// <summary>
        /// Обновить кнопки склада от какой страницы
        /// </summary>
        private void UpdateButtonStorage(int c)
        {
            for (int i = 0; i < pageCount; i++)
            {
                if (c < countItemsStorage)
                {
                    buttonStorage[i].GetSlot().SetIndex(c + 100);
                    buttonStorage[i].GetSlot().Set(GetItemStackById(arrayItems[c++]));
                }
                else
                {
                    buttonStorage[i].GetSlot().SetIndex(99);
                    buttonStorage[i].GetSlot().Set();
                }
            }
            PageUpdate();
        }
        

        /// <summary>
        /// Зменены памер акна
        /// </summary>
        protected override void ResizedScreen()
        {
            base.ResizedScreen();
            int w = position.x;
            int h = position.y;
            labelPage.Position = new vec2i(w + 306 * SizeInterface, h);
            buttonBack.Position = new vec2i(w + 274 * SizeInterface, h);
            buttonNext.Position = new vec2i(w + 356 * SizeInterface, h);
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public override void MouseWheel(int delta, int x, int y)
        {
            bool next = delta < 0;
            if ((next && countItemsStorage > page + pageCount - 1)
                || (!next && page > 0))
            {
                ButtonBackNextClick(next);
                isRender = true;
            }
        }

        private void ButtonBackNextClick(bool next)
        {
            page += next ? pageCount : -pageCount;
            UpdateButtonStorage(page);
        }

        private void PageUpdate()
        {
            labelPage.SetText((page / pageCount + 1).ToString() + " / " + (countItemsStorage / pageCount + 1).ToString());
            if (page <= 0) buttonBack.Enabled = false;
            else buttonBack.Enabled = true;
            if (countItemsStorage > page + pageCount) buttonNext.Enabled = true;
            else buttonNext.Enabled = false;
        }
    }
}
