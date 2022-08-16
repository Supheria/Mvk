using MvkServer.Util;
using System;
using System.Diagnostics;
using System.Threading;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект тактов и кадров в секунду
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// Запущен ли поток
        /// </summary>
        public bool IsRuning { get; private set; } = false;
        /// <summary>
        /// Желаемое количество кадров в секунду
        /// </summary>
        public int WishFrame { get; private set; } = 60;
        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// </summary>
        public float Interpolation { get; private set; } = 0;
        /// <summary>
        /// Клиент
        /// </summary>
        public Client ClientMain { get; private set; }

        /// <summary>
        /// Желаемое количество тактов в секунду
        /// </summary>
        private readonly int wishTick = 20;
        /// <summary>
        /// Интервал в тиках для кадра
        /// </summary>
        private long intervalFrame;
        /// <summary>
        /// Интервал в тиках для такта
        /// </summary>
        private readonly long intervalTick;
        /// <summary>
        /// Время для сна без нагрузки для кадра
        /// </summary>
        private long sleepFrame;
        /// <summary>
        /// Время для сна без нагрузки для такта
        /// </summary>
        private readonly long sleepTick;
        /// <summary>
        /// Максимальный fps
        /// </summary>
        private bool isMax = false;

        public Ticker(Client client)
        {
            ClientMain = client;
            SetWishFrame(WishFrame);
            intervalTick = Stopwatch.Frequency / wishTick;
            sleepTick = intervalTick / MvkStatic.TimerFrequency;
        }

        /// <summary>
        /// Задать желаемый фпс
        /// </summary>
        public void SetWishFrame(int frame)
        {
            if (frame > 250)
            {
                isMax = true;
            }
            else
            {
                isMax = false;
                WishFrame = frame;
                intervalFrame = Stopwatch.Frequency / WishFrame;
                sleepFrame = intervalFrame / MvkStatic.TimerFrequency;
            }
        }

        /// <summary>
        /// Запуск
        /// </summary>
        public void Start()
        {
            Thread myThread = new Thread(RunThreadTick)
            {
                Priority = ThreadPriority.Highest
            };
            IsRuning = true;
            myThread.Start();
        }

        /// <summary>
        /// Останавливаем
        /// </summary>
        public void Stoping() => IsRuning = false;

        /// <summary>
        /// Метод запуска для отдельного потока, такты
        /// </summary>
        private void RunThreadTick()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long lastTimeFrame = stopwatch.ElapsedTicks;
            long lastTimeTick = lastTimeFrame;

            long currentTimeBegin;
            int sleepFrame = 0;
            int sleepTick = 0;
            int sleep;

            long nextTick = lastTimeTick + intervalTick;
            long nextFrame = lastTimeTick + intervalFrame;

            while (IsRuning)
            {
                // Проверяем такт
                currentTimeBegin = stopwatch.ElapsedTicks;
                if (ClientMain.World != null && !ClientMain.IsGamePaused)
                {
                    if (currentTimeBegin >= nextTick)
                    {
                        lastTimeTick = currentTimeBegin;
                        nextTick += intervalTick;
                        OnTick();
                        Interpolation = 0;
                    }
                    else
                    {
                        Interpolation = (currentTimeBegin - lastTimeTick) / (float)MvkStatic.TimerFrequencyTps;
                        if (Interpolation > 1f) Interpolation = 1f;
                        if (Interpolation < 0) Interpolation = 0f;
                    }
                }
                else if (ClientMain.World != null && ClientMain.IsGamePaused)
                {
                    if (currentTimeBegin >= nextTick)
                    {
                        lastTimeTick = currentTimeBegin;
                        nextTick += intervalTick;
                        Interpolation = 0;
                    }
                }

                // Проверяем кадр
                if (isMax)
                {
                    OnFrame();
                }
                else
                {
                    currentTimeBegin = stopwatch.ElapsedTicks;
                    if (currentTimeBegin >= nextFrame)
                    {
                        lastTimeFrame = currentTimeBegin;
                        nextFrame += intervalFrame;
                        OnFrame();
                    }

                    // Нужно ли засыпание
                    currentTimeBegin = stopwatch.ElapsedTicks;
                    sleepTick = nextTick < currentTimeBegin ? 0 : (int)this.sleepTick - Mth.Floor((nextTick - currentTimeBegin) / MvkStatic.TimerFrequency);
                    sleepFrame = nextFrame < currentTimeBegin ? 0 : (int)this.sleepFrame - Mth.Floor((nextFrame - currentTimeBegin) / MvkStatic.TimerFrequency);

                    // Находим на именьшое засыпание и засыпаем
                    sleep = Mth.Min(sleepFrame, sleepTick);
                    if (sleep > 0) Thread.Sleep(sleep);
                }
            }
            OnCloseded();
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event EventHandler Tick;
        private void OnTick() => Tick?.Invoke(this, new EventArgs());
        /// <summary>
        /// Событие кадра
        /// </summary>
        public event EventHandler Frame;
        private void OnFrame() => Frame?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        private void OnCloseded() => Closeded?.Invoke(this, new EventArgs());
    }
}
