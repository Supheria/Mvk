using System;

namespace MvkServer.Entity.AI.PathFinding
{
    public class Path
    {
        /// <summary>
        /// Содержит точки на этом пути
        /// </summary>
        private PathPoint[] pathPoints = new PathPoint[1024];

        /// <summary>
        /// Количество точек на этом пути
        /// </summary>
        private int count;

        /// <summary>
        /// Добавляет точку на путь
        /// </summary>
        public PathPoint AddPoint(PathPoint point)
        {
            if (point.index >= 0)
            {
                throw new Exception("Индекс тут должен быть = -1");
            }
            else
            {
                if (count == pathPoints.Length)
                {
                    PathPoint[] points = new PathPoint[count << 1];
                    Array.Copy(pathPoints, 0, points, 0, count);
                    pathPoints = points;
                }

                pathPoints[count] = point;
                point.index = count;
                SortBack(count++);
                return point;
            }
        }

        /// <summary>
        /// Очищает путь
        /// </summary>
        public void ClearPath() => count = 0;

        /// <summary>
        /// Возвращает и удаляет первую точку пути
        /// </summary>
        public PathPoint Dequeue()
        {
            PathPoint points = pathPoints[0];
            pathPoints[0] = pathPoints[--count];
            pathPoints[count] = null;
            if (count > 0) SortForward(0);
            points.index = -1;
            return points;
        }

        /// <summary>
        /// Изменяет расстояние указанной точки до цели
        /// </summary>
        /// <param name="point">Точка пути</param>
        /// <param name="distance">новое расстояние</param>
        public void ChangeDistance(PathPoint point, float distance)
        {
            float distanceToTarget = point.distanceToTarget;
            point.distanceToTarget = distance;
            if (distance < distanceToTarget) SortBack(point.index);
            else SortForward(point.index);
        }

        /// <summary>
        /// Сортирует точку с конца
        /// </summary>
        private void SortBack(int index)
        {
            PathPoint point = pathPoints[index];
            int indexNew;
            for (float distance = point.distanceToTarget; index > 0; index = indexNew)
            {
                indexNew = index - 1 >> 1;
                PathPoint point2 = pathPoints[indexNew];

                if (distance >= point2.distanceToTarget) break;

                pathPoints[index] = point2;
                point2.index = index;
            }
            pathPoints[index] = point;
            point.index = index;
        }

        /// <summary>
        /// Сортирует точку с начала
        /// </summary>
        private void SortForward(int index)
        {
            PathPoint point = pathPoints[index];
            float distance = point.distanceToTarget;

            int index2, index3;
            float distance2, distance3;
            PathPoint point2, point3;

            while (true)
            {
                index2 = 1 + (index << 1);
                index3 = index2 + 1;

                if (index2 >= count) break;

                point2 = pathPoints[index2];
                distance2 = point2.distanceToTarget;

                if (index3 >= count)
                {
                    point3 = null;
                    distance3 = float.PositiveInfinity;
                }
                else
                {
                    point3 = pathPoints[index3];
                    distance3 = point3.distanceToTarget;
                }

                if (distance2 < distance3)
                {
                    if (distance2 >= distance) break;
                    pathPoints[index] = point2;
                    point2.index = index;
                    index = index2;
                }
                else
                {
                    if (distance3 >= distance) break;
                    pathPoints[index] = point3;
                    point3.index = index;
                    index = index3;
                }
            }

            pathPoints[index] = point;
            point.index = index;
        }

        /// <summary>
        /// Возвращает true, если этот путь не содержит точек
        /// </summary>
        public bool IsPathEmpty() => count == 0;
    }
}
