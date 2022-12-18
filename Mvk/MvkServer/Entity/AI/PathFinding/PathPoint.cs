using MvkServer.Util;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Точки пути
    /// </summary>
    public class PathPoint
    {
        public readonly int xCoord;
        public readonly int yCoord;
        public readonly int zCoord;

        /// <summary>
        /// Индекс этой точки в назначенном ей пути
        /// </summary>
        public int index = -1;
        /// <summary>
        /// Расстояние до цели
        /// </summary>
        public float distanceToTarget;
        /// <summary>
        /// Расстояние по пути до этой точки
        /// </summary>
        public float totalPathDistance;
        /// <summary>
        /// Линейное расстояние до следующей точки
        /// </summary>
        public float distanceToNext;
        /// <summary>
        /// Предшествующая этому точка на заданном пути
        /// </summary>
        public PathPoint previous;
        /// <summary>
        /// Истинно, если навигатор уже посетил эту точку
        /// </summary>
        public bool visited;

        /// <summary>
        /// Хэш координат, используемый для идентификации этой точки
        /// </summary>
        private readonly int hash;

        public PathPoint(int x, int y, int z)
        {
            xCoord = x;
            yCoord = y;
            zCoord = z;
            hash = MakeHash(x, y, z);
        }

        /// <summary>
        /// Возвращает линейное расстояние до другой точки пути
        /// </summary>
        public float DistanceTo(PathPoint point)
        {
            float x = point.xCoord - xCoord;
            float y = point.yCoord - yCoord;
            float z = point.zCoord - zCoord;
            return Mth.Sqrt(x * x + y * y + z * z);
        }
        /// <summary>
        /// Возвращает линейное расстояние до другой точки
        /// </summary>
        public float DistanceTo(float x, float y, float z)
        {
            float vx = x - xCoord - .5f;
            float vy = y - yCoord - .5f;
            float vz = z - zCoord - .5f;
            return Mth.Sqrt(vx * vx + vy * vy + vz * vz);
        }

        /// <summary>
        /// Возвращает квадрат расстояния до другой точки пути
        /// </summary>
        public float DistanceToSquared(PathPoint point)
        {
            float x = point.xCoord - xCoord;
            float y = point.yCoord - yCoord;
            float z = point.zCoord - zCoord;
            return x * x + y * y + z * z;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(PathPoint))
            {
                var point = (PathPoint)obj;
                if (hash == point.hash && xCoord == point.xCoord && yCoord == point.yCoord && zCoord == point.zCoord)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает true, если эта точка уже была назначена пути
        /// </summary>
        public bool IsAssigned() => index >= 0;

        public override int GetHashCode() => hash;

        public static int MakeHash(int x, int y, int z) 
            => y & 255 | (x & 32767) << 8 | (z & 32767) << 24 | (x < 0 ? int.MinValue : 0) | (z < 0 ? 32768 : 0);

        public override string ToString() => xCoord + "," + yCoord + "," + zCoord;
    }
}
