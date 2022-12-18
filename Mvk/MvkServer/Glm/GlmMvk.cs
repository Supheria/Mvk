namespace MvkServer.Glm
{
    public static partial class glm
    {
        /// <summary>
        /// Возвращает новый вектор X со значением value, равным второму параметру, 
        /// вдоль линии между этим вектором и переданным вектором или vec(0), если это невозможно.
        /// </summary>
        public static vec3 GetIntermediateWithXValue(vec3 vec1, vec3 vec2, float valueX)
        {
            float vx = vec2.x - vec1.x;
            if ((vx * vx) < 1E-7f) return new vec3(0);
            float vy = vec2.y - vec1.y;
            float vz = vec2.z - vec1.z;
            float k = (valueX - vec1.x) / vx;
            return k >= 0f && k <= 1f ? new vec3(vec1.x + vx * k, vec1.y + vy * k, vec1.z + vz * k) : new vec3(0);
        }

        /// <summary>
        /// Возвращает новый вектор Y со значением value, равным второму параметру, 
        /// вдоль линии между этим вектором и переданным вектором или vec(0), если это невозможно.
        /// </summary>
        public static vec3 GetIntermediateWithYValue(vec3 vec1, vec3 vec2, float valueY)
        {
            float vy = vec2.y - vec1.y;
            if ((vy * vy) < 1E-7f) return new vec3(0);
            float vx = vec2.x - vec1.x;
            float vz = vec2.z - vec1.z;
            float k = (valueY - vec1.y) / vy;
            return k >= 0f && k <= 1f ? new vec3(vec1.x + vx * k, vec1.y + vy * k, vec1.z + vz * k) : new vec3(0);
        }

        /// <summary>
        /// Возвращает новый вектор Z со значением value, равным второму параметру, 
        /// вдоль линии между этим вектором и переданным вектором или vec(0), если это невозможно.
        /// </summary>
        public static vec3 GetIntermediateWithZValue(vec3 vec1, vec3 vec2, float valueZ)
        {
            float vz = vec2.z - vec1.z;
            if ((vz * vz) < 1E-7f) return new vec3(0);
            float vx = vec2.x - vec1.x;
            float vy = vec2.y - vec1.y;
            float k = (valueZ - vec1.z) / vz;
            return k >= 0f && k <= 1f ? new vec3(vec1.x + vx * k, vec1.y + vy * k, vec1.z + vz * k) : new vec3(0);
        }

        /// <summary>
        /// Квадрат евклидова расстояния между этим и заданным вектором.
        /// </summary>
        public static float SquareDistanceTo(vec3 vec1, vec3 vec2)
        {
            float vx = vec2.x - vec1.x;
            float vy = vec2.y - vec1.y;
            float vz = vec2.z - vec1.z;
            return vx * vx + vy * vy + vz * vz;
        }
        /// <summary>
        /// Квадрат евклидова расстояния между этим и заданным вектором.
        /// </summary>
        public static float SquareDistanceTo(float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
        {
            float vx = v2x - v1x;
            float vy = v2y - v1y;
            float vz = v2z - v1z;
            return vx * vx + vy * vy + vz * vz;
        }
    }
}
