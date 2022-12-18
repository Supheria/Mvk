using System.Diagnostics;

namespace MvkServer.Util
{
    public class Profiler
    {
        /// <summary>
        /// Объект лога
        /// </summary>
        public Logger Log { get; protected set; }

        protected Stopwatch stopwatch = new Stopwatch();
        protected string profilingSection;
        protected bool profilingEnabled = false;

        public Profiler(Logger log)
        {
            Log = log;
            stopwatch.Start();
        }

        public void StartSection(string name)
        {
            profilingSection = name;
            profilingEnabled = true;
            stopwatch.Restart();
        }

        /// <summary>
        /// Закрыть проверку по времени, если параметр 0, будет всегда ответ в мс 0.00
        /// </summary>
        public void EndSection(int stepTime = 100)
        {
            if (profilingEnabled)
            {
                profilingEnabled = false;
                
                if (stepTime == 0)
                {
                    Log.Log("{0} {1:0.00} мс", profilingSection, stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency);
                }
                else
                {
                    long time = stopwatch.ElapsedTicks / MvkStatic.TimerFrequency;
                    if (time > stepTime) // больше 100 мс
                    {
                        Log.Log("Что-то слишком долго! {0} заняло приблизительно {1} мс", profilingSection, time);
                    }
                }
            }
        }

        public void EndStartSection(string name)
        {
            EndSection();
            StartSection(name);
        }
    }
}
