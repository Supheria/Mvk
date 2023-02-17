using MvkAssets;
using MvkClient.Setitings;
using MvkClient.Util;
using MvkServer.Sound;
using System;
using System.Drawing;
using System.IO;

namespace MvkClient
{
    /// <summary>
    /// Объект подготовки загрузки звуковых файлов в буфер, текстур и прочего
    /// </summary>
    public class Loading
    {
        /// <summary>
        /// Количество процессинга
        /// </summary>
        public int Count { get; protected set; } = 70;
        /// <summary>
        /// Основной объект клиента
        /// </summary>
        private Client client;

        public Loading(Client client)
        {
            this.client = client;

            
            // Определяем максимальное количество для счётчика
            Count = 1 // Загрузка опций
                + Enum.GetValues(typeof(AssetsSample)).Length + Enum.GetValues(typeof(AssetsTexture)).Length 
                - 4 // 3 текстуры загружаются до загрузчика (шрифты и логотип)
                + 1; // Финишный такт
        }

        /// <summary>
        /// Запуск загрузчика
        /// </summary>
        public void LoadStart()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                // Опции
                Setting.Load();
                Language.SetLanguage((AssetsLanguage)Setting.Language);
                OnTick(new ObjectKeyEventArgs(ObjectKey.LoadStep));

                // Загрузка семплов
                Array array = Enum.GetValues(typeof(AssetsSample));
                client.Sample.InitializeArray(array.Length);
                foreach (AssetsSample key in array)
                {
                    if (key == AssetsSample.None) continue;
                    client.Sample.InitializeSample(key);
                    OnTick(new ObjectKeyEventArgs(ObjectKey.LoadStep));
                }

                // AtlasBlocks
                Bitmap bitmapAtlasBlocks = File.Exists("AtlasBlocks.png") ? new Bitmap("AtlasBlocks.png") : Assets.AtlasBlocks;
                OnTick(new ObjectKeyEventArgs(ObjectKey.LoadStepTexture, 
                    new BufferedImage(AssetsTexture.AtlasBlocks, bitmapAtlasBlocks, true)));

                // AtlasItems
                Bitmap bitmapAtlasItems = File.Exists("AtlasItems.png") ? new Bitmap("AtlasItems.png") : Assets.AtlasItems;
                OnTick(new ObjectKeyEventArgs(ObjectKey.LoadStepTexture, 
                    new BufferedImage(AssetsTexture.AtlasItems, bitmapAtlasItems, true)));

                int i = 0;
                foreach (AssetsTexture key in Enum.GetValues(typeof(AssetsTexture)))
                {
                    i++;
                    if (i < 7) continue;
                    OnTick(new ObjectKeyEventArgs(ObjectKey.LoadStepTexture, new BufferedImage(key, Assets.GetBitmap(key))));
                }
                //System.Threading.Thread.Sleep(2000); // Тест пауза чтоб увидеть загрузчик
                OnTick(new ObjectKeyEventArgs(ObjectKey.LoadedMain));
            });
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event ObjectKeyEventHandler Tick;
        protected virtual void OnTick(ObjectKeyEventArgs e) => Tick?.Invoke(this, e);
    }
}
