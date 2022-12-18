using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь сущности
    /// </summary>
    public class PathEntity
    {
        /// <summary>
        /// Фактические точки пути
        /// </summary>
        private readonly PathPoint[] points;

        /// <summary>
        /// Является ли пункт назначения одинаковым
        /// </summary>
        private readonly bool isDestinationSame;

        /// <summary>
        /// Индекс массива, на который в данный момент нацелен объект
        /// </summary>
        private int currentPathIndex = 0;

        /// <summary>
        /// Общая длина пути
        /// </summary>
        private int pathLength;

        public PathEntity(PathPoint[] points, bool isDestinationSame)
        {
            this.isDestinationSame = isDestinationSame;
            this.points = points;
            pathLength = points.Length;
        }

        /// <summary>
        /// Направляет этот путь к следующей точке в своем массиве
        /// </summary>
        public void IncrementPathIndex() => ++currentPathIndex;

        /// <summary>
        /// Возвращает true, если этот путь достиг конца
        /// </summary>
        public bool IsFinished() => currentPathIndex >= pathLength;

        /// <summary>
        /// Возвращает последний объект массива
        /// </summary>
        public PathPoint GetFinalPathPoint() => pathLength > 0 ? points[pathLength - 1] : null;

        /// <summary>
        /// Вернуть объект по интексу расположения
        /// </summary>
        public PathPoint GetPathPointFromIndex(int index) => points[index];

        /// <summary>
        /// Вернуть общую длинну
        /// </summary>
        public int GetCurrentPathLength() => pathLength;

        /// <summary>
        /// Задать общую длинну
        /// </summary>
        public void SetCurrentPathLength(int length) => pathLength = length;

        /// <summary>
        /// Вернуть индекс текущего элемента
        /// </summary>
        public int GetCurrentPathIndex() => currentPathIndex;

        /// <summary>
        /// Задать индекс текущего элемента
        /// </summary>
        public void SetCurrentPathIndex(int index) => currentPathIndex = index;

        /// <summary>
        /// Получает вектор объекта, связанный с данным индексом.
        /// </summary>
        public vec3 GetVectorFromIndex(EntityBase entity, int index)
        {
            float w = entity.Width * 2f;
            w = (Mth.Ceiling(w) - w) / 2f + entity.Width;
            return new vec3(points[index].xCoord + w, points[index].yCoord, points[index].zCoord + w);
        }

        /// <summary>
        /// Возвращает текущий целевой узел объекта как vec3
        /// </summary>
        public vec3 GetPosition(EntityBase entity) => GetVectorFromIndex(entity, currentPathIndex);

        /// <summary>
        /// Возвращает true, если EntityPath совпадают. Равенства, не связанные с экземпляром
        /// </summary>
        public bool IsSamePath(PathEntity pathEntity)
        {
            if (pathEntity == null) return false;
            if (pathEntity.points.Length != points.Length) return false;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].xCoord != pathEntity.points[i].xCoord || points[i].yCoord != pathEntity.points[i].yCoord 
                    || points[i].zCoord != pathEntity.points[i].zCoord) return false;
            }
            return true;
        }

        /// <summary>
        /// Является ли пункт назначения одинаковым
        /// </summary>
        public bool IsDestinationSame()
        {
            PathPoint pathPoint = GetFinalPathPoint();
            return pathPoint == null ? false : isDestinationSame;
        }

        /// <summary>
        /// Отладка, визуализация перемещения PathNavigate.SetPath
        /// </summary>
        public void DebugPath(WorldBase world)
        {
            for (int i = 0; i < points.Length; i++)
            {
                world.SpawnParticle(EnumParticle.Flame, 1, new vec3(points[i].xCoord + .5f, points[i].yCoord, points[i].zCoord + .5f), new vec3(0), 0);
            }
        }

        /// <summary>
        /// Отладка, последняя точка куда идти PathFinder.CreateEntityPath
        /// </summary>
        public static void DebugEnd(WorldBase world, PathPoint pointEnd) 
            => world.SpawnParticle(EnumParticle.Flame, 1, new vec3(pointEnd.xCoord + .5f, pointEnd.yCoord, pointEnd.zCoord + .5f), new vec3(0), 0);
    }
}
