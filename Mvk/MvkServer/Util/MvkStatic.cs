using MvkServer.Glm;
using System.Collections.Generic;
using System.Diagnostics;

namespace MvkServer.Util
{
    /// <summary>
    /// Статические переменные
    /// Многие формируются при открытии проекта
    /// </summary>
    public class MvkStatic
    {
        /**
         *      (North)
         *        0;-1
         *         N
         * (West)  |   (East)
         *   W ----+---- E 
         * -1;0    |    1;0
         *         S
         *        0;1
         *      (South)
         **/


        /// <summary>
        /// Область в один блок без центра, 4 блоков
        /// </summary>
        public static vec2i[] AreaOne4 { get; private set; }
        /// <summary>
        /// Область в один блок без центра, 8 блоков
        /// </summary>
        public static vec2i[] AreaOne8 { get; private set; }
        /// <summary>
        /// Область в один блок c центром, 9 блоков
        /// </summary>
        public static vec2i[] AreaOne9 { get; private set; }

        /// <summary>
        /// 3д область в один блок без центра, 6 блоков
        /// </summary>
        public static vec3i[] ArraOne3d6 { get; private set; }

        /// <summary>
        /// Параметр [0]=0 .. [16]=0.015625f
        /// </summary>
        public static float[] Uv { get; protected set; } = new float[17];
        /// <summary>
        /// Параметр [1]=0.0625f .. [16]=1.0f
        /// </summary>
        public static float[] Xy { get; protected set; } = new float[17];

        /// <summary>
        /// Получает частоту таймера в виде количества тактов в милисекунду
        /// </summary>
        public static long TimerFrequency { get; protected set; }
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в один TPS
        /// </summary>
        public static long TimerFrequencyTps { get; protected set; }
        /// <summary>
        /// Массив круговой до 37 (32+5) обзора
        /// </summary>
        public static vec2i[] DistSqrt37 { get; private set; }
        /// <summary>
        /// Массив круговой 2d до 37 (32+5) обзора
        /// </summary>
        public static vec2i[][] DistSqrtTwo2d { get; private set; }
        /// <summary>
        /// Массив круговой 3d до UPDATE_ALPHE_CHUNK (4)
        /// </summary>
        public static vec3i[][] DistSqrtTwo3d { get; private set; }
        /// <summary>
        /// Длинна массива при нужном обзоре DistSqrt37
        /// </summary>
        public static int[] DistSqrt37Light { get; private set; } = new int[40];
        /// <summary>
        /// Яркость для текстур
        /// </summary>
        public static float[] LightBrightnessTable { get; private set; } = new float[16];
        /// <summary>
        /// Яркость фазы лун
        /// </summary>
        public static float[] LightMoonPhase { get; private set; }

        /// <summary>
        /// Инициализация, запускаем при старте
        /// </summary>
        public static void Initialized()
        {
            for (int i = 0; i <= 16; i++)
            {
                Uv[i] = (float)i * 0.0009765625f;
                Xy[i] = (float)i * 0.0625f;
            }
            AreaOne4 = new vec2i[] { 
                new vec2i(0, 1), new vec2i(1, 0), new vec2i(0, -1), new vec2i(-1, 0)
            };
            AreaOne8 = new vec2i[] {
                new vec2i(0, 1), new vec2i(1, 1), new vec2i(1, 0), new vec2i(1, -1),
                new vec2i(0, -1), new vec2i(-1, -1), new vec2i(-1, 0), new vec2i(-1, 1)
            };
            AreaOne9  = new vec2i[] { new vec2i(0, 0),
                new vec2i(0, 1), new vec2i(1, 1), new vec2i(1, 0), new vec2i(1, -1),
                new vec2i(0, -1), new vec2i(-1, -1), new vec2i(-1, 0), new vec2i(-1, 1)
            };
            ArraOne3d6 = new vec3i[]
            {
                new vec3i(0, 1, 0), new vec3i(0, -1, 0), new vec3i(1, 0, 0),
                new vec3i(-1, 0, 0), new vec3i(0, 0, -1), new vec3i(0, 0, 1)
            };
            TimerFrequency = Stopwatch.Frequency / 1000;
            TimerFrequencyTps = Stopwatch.Frequency / 20;

            DistSqrt37 = GetSqrt(40);
            DistSqrtTwo2d = new vec2i[40][];
            for (int i = 0; i < 40; i++)
            {
                DistSqrtTwo2d[i] = GetSqrt(i);
                DistSqrt37Light[i] = GetSqrtCount(i);
            }

            int count = MvkGlobal.UPDATE_ALPHE_CHUNK + 1;
            DistSqrtTwo3d = new vec3i[count][];
            for (int i = 0; i < count; i++)
            {
                DistSqrtTwo3d[i] = GetSqrt3d(i);
            }
            

            float fm = 0f;
            for (int i = 0; i < 16; i++)
            {
                float f = 1f - i / 15f;
                LightBrightnessTable[i] = (1f - f) / (f * 3f + 1f) * (1f - fm) + fm;
            }
            LightMoonPhase = new float[] { .8f, .48f, .32f, .16f, 0, .16f, .32f, .48f };
        }

        /// <summary>
        /// Сгенерировать массив по длинам используя квадратный корень
        /// </summary>
        /// <param name="overview">Обзор, в одну сторону от ноля</param>
        private static vec2i[] GetSqrt(int overview)
        {
            List<ArrayDistance> r = new List<ArrayDistance>();
            for (int x = -overview; x <= overview; x++)
            {
                for (int y = -overview; y <= overview; y++)
                {
                    r.Add(new ArrayDistance(new vec3i(x, y, 0), Mth.Sqrt(x * x + y * y)));
                }
            }
            r.Sort();
            vec2i[] list = new vec2i[r.Count];
            for (int i = 0; i < r.Count; i++)
            {
                list[i] = r[i].GetPos2d();
            }
            return list;
        }

        /// <summary>
        /// Сгенерировать массив по длинам используя квадратный корень
        /// </summary>
        /// <param name="overview">Обзор, в одну сторону от ноля</param>
        private static int GetSqrtCount(int overview)
        {
            int count = 0;
            ArrayDistance arrayDistance;
            for (int x = -overview; x <= overview; x++)
            {
                for (int y = -overview; y <= overview; y++)
                {
                    arrayDistance = new ArrayDistance(new vec3i(x, y, 0), Mth.Sqrt(x * x + y * y));
                    if (arrayDistance.Distance() - .3f  <= overview)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Сгенерировать массив по длинам используя квадратный корень в объёме
        /// </summary>
        /// <param name="overview">Обзор, в одну сторону от ноля</param>
        private static vec3i[] GetSqrt3d(int overview)
        {
            List<ArrayDistance> r = new List<ArrayDistance>();
            for (int x = -overview; x <= overview; x++)
            {
                for (int y = -overview; y <= overview; y++)
                {
                    for (int z = -overview; z <= overview; z++)
                    {
                        r.Add(new ArrayDistance(new vec3i(x, y, z), Mth.Sqrt(x * x + y * y + z * z)));
                    }
                }
            }
            r.Sort();
            vec3i[] list = new vec3i[r.Count];
            for (int i = 0; i < r.Count; i++)
            {
                list[i] = r[i].Position();
            }
            return list;
        }

        /// <summary>
        /// Получить индекс по вектору в один блок без центра, 8 блоков
        /// </summary>
        public static int GetAreaOne8(int x, int y)
        {
            if (x == 1)
            {
                if (y == 0) return 2;
                if (y == -1) return 3;
                return 1;
            }
            if (x == -1)
            {
                if (y == -1) return 5;
                if (y == 0) return 6;
                return 7;
            }
            if (y == -1) return 4;
            return 0;
        }
        /// <summary>
        /// Получить индекс по вектору в один блок без центра, 9 блоков
        /// </summary>
        public static int GetAreaOne9(int x, int y)
        {
            if (x == 0)
            { 
                if (y == 0) return 0;
                if (y == 1) return 1;
                return 5;
            }
            if (x == 1)
            {
                if (y == 0) return 3;
                if (y == -1) return 4;
                return 2;
            }
            if (y == -1) return 6;
            if (y == 0) return 7;
            return 8;
        }

        
    }
}
