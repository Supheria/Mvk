﻿using System;

namespace MvkServer.Glm
{
    public static partial class glm
    {
        private static float[] sinTable = new float[65536];

        public static void Initialized()
        {
            for (int i = 0; i < 65536; i++)
            {
                sinTable[i] = (float)Math.Sin((float)i * Math.PI * 2.0f / 65536.0f);
            }
        }

        /// <summary>
        /// Четверть Пи, аналог 10гр
        /// </summary>
        public readonly static float pi10 = 0.174532925f;
        /// <summary>
        /// Четверть Пи, аналог 15гр
        /// </summary>
        public readonly static float pi15 = 0.2617993878f;
        /// <summary>
        /// Четверть Пи, аналог 20гр
        /// </summary>
        public readonly static float pi20 = 0.349065850399f;
        /// <summary>
        /// Четверть Пи, аналог 45гр
        /// </summary>
        public readonly static float pi45 = 0.7853981625f;
        /// <summary>
        /// Половина Пи, аналог 90гр
        /// </summary>
        public readonly static float pi90 = 1.570796325f;
        /// <summary>
        /// Половина Пи, аналог 135гр
        /// </summary>
        public readonly static float pi135 = 2.356194487f;
        /// <summary>
        /// Пи, аналог 180гр
        /// </summary>
        public readonly static float pi = 3.14159265f;
        /// <summary>
        /// 3/2 пи, аналог 270гр
        /// </summary>
        public readonly static float pi270 = 4.712388985f;
        /// <summary>
        /// Два Пи, аналог 360гр
        /// </summary>
        public readonly static float pi360 = 6.28318531f;

        public static float degrees(float radians)
        {
            return radians * 57.295779513082320876798154814105f;
        }

        public static float radians(float degrees)
        {
            return degrees * 0.01745329251994329576923690768489f;

        }
        public static float cos(float angle)
        {
            angle %= pi360;
            return sinTable[(int)(angle * 10430.378f + 16384.0f) & 65535];
        }

        public static float sin(float angle)
        {
            angle %= pi360;
            return sinTable[(int)(angle * 10430.378f) & 65535];
        }

        public static float tan(float angle)
        {
            float c = cos(angle);
            return c == 0 ? float.PositiveInfinity : sin(angle) / c;
            //return (float)Math.Tan(angle);
        }

        public static float atan2(float y, float x) => (float)Math.Atan2(y, x);

        /// <summary>
        /// Перевести угол в радианах в -180 .. 180
        /// </summary>
        public static float wrapAngleToPi(float angle)
        {
            angle %= pi360;
            if (angle >= pi) angle -= pi360;
            if (angle < -pi) angle += pi360;
            return angle;
        }
    }
}
