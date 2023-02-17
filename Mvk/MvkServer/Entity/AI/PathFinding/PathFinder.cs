using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.AI.PathFinding
{
    public class PathFinder
    {
        /// <summary>
        /// Объект мира
        /// </summary>
        private readonly WorldBase world;
        /// <summary>
        /// Процесс узла перемещения
        /// </summary>
        private NodeProcessor nodeProcessor;
        /// <summary>
        /// Создаваемый путь
        /// </summary>
        private Path path = new Path();
        /// <summary>
        /// Выбор точек пути для добавления к пути
        /// </summary>
        private readonly PathPoint[] pathOptions = new PathPoint[32];

        /// <summary>
        /// Остановка при соприкосновении коллизии
        /// true - соприкосновении коллизии, false - центр
        /// </summary>
        private bool stopOnOverlap;
        /// <summary>
        /// Приемочный радиус
        /// </summary>
        private float acceptanceRadius = 0;

        public PathFinder(WorldBase world, NodeProcessor nodeProcessor)
        {
            this.world = world;
            this.nodeProcessor = nodeProcessor;
        }

        #region Опции

        /// <summary>
        /// Задать опции
        /// </summary>
        /// <param name="stopOnOverlap">Задать остановку при соприкосновении коллизии, true - соприкосновении коллизии, false - центр</param>
        /// <param name="acceptanceRadius">Задать расстояние до точки, которое будет считаться выполненым, 0 - расчёта нет</param>
        public void SetOptions(bool stopOnOverlap, float acceptanceRadius)
        {
            this.stopOnOverlap = stopOnOverlap;
            this.acceptanceRadius = acceptanceRadius;
        }
        /// <summary>
        /// Остановка при соприкосновении коллизии
        /// true - соприкосновении коллизии, false - центр
        /// </summary>
        public bool GetStopOnOverlap() => stopOnOverlap;
        /// <summary>
        /// Получить расстояние до точки, которое будет считаться выполненым,
        /// 0 - расчёта нет
        /// </summary>
        public float GetAcceptanceRadius() => acceptanceRadius;

        #endregion

        /// <summary>
        /// Создает путь от одного объекта к другому на минимальном расстоянии
        /// </summary>
        public PathEntity CreateEntityPathTo(WorldBase world, EntityLiving entity, EntityLiving entityGoal, float distance)
        {
            float y = entityGoal.Position.y;
            if (entity.IsFlying && nodeProcessor is SwimOrFlyNodeProcessor)
            {
                y += entityGoal.GetEyeHeight();
            }
            return CreateEntityPathTo(world, entity, entityGoal.Position.x, y, entityGoal.Position.z, entityGoal.Width, distance);
        }
            
        /// <summary>
        /// Создает путь от объекта к указанному местоположению на минимальном расстоянии
        /// </summary>
        public PathEntity CreateEntityPathTo(WorldBase world, EntityLiving entity, BlockPos blockPos, float distance) 
            => CreateEntityPathTo(world, entity, blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + .5f, 0, distance);

        /// <summary>
        /// Создает путь от объекта к указанному местоположению на минимальном расстоянии
        /// </summary>
        /// <param name="entityGoalWidth">Ширина сущности к которой идём</param>
        private PathEntity CreateEntityPathTo(WorldBase world, EntityLiving entity, float x, float y, float z, float entityGoalWidth, float distance)
        {
            path.ClearPath();
            nodeProcessor.InitProcessor(world, entity);
            PathPoint pointBegin = nodeProcessor.GetPathPointTo(entity);
            PathPoint pointEnd = nodeProcessor.GetPathPointToCoords(entity, x, y, z);
            if (pointEnd != null)
            {
                PathEntity points = InitEntityPath(entity, pointBegin, pointEnd, x, y, z, entityGoalWidth, distance);
                nodeProcessor.PostProcess();
                return points;
            }
            return null;
        }

        /// <summary>
        /// Инициализировать путь объекта
        /// </summary>
        /// <param name="entityGoalWidth">Ширина сущности к которой идём</param>
        private PathEntity InitEntityPath(EntityLiving entity, PathPoint pointBegin, PathPoint pointEnd, float x, float y, float z, float entityGoalWidth, float distance)
        {
            pointBegin.totalPathDistance = 0f;
            pointBegin.distanceToNext = pointBegin.DistanceToSquared(pointEnd);
            pointBegin.distanceToTarget = pointBegin.distanceToNext;
            path.ClearPath();
            path.AddPoint(pointBegin);
            PathPoint point = pointBegin;
            int count = 0;
            bool isAcceptanceRadius = stopOnOverlap ? entityGoalWidth != 0 : false;
            float acceptanceRadius = isAcceptanceRadius ? entityGoalWidth + entity.Width + .5f : .5f;
            if (this.acceptanceRadius > 0)
            {
                isAcceptanceRadius = true;
                acceptanceRadius += this.acceptanceRadius;
            }
            ///acceptanceRadius">Приемочный радиус
            while (!path.IsPathEmpty())
            {
                PathPoint point2 = path.Dequeue();

                if (point2.Equals(pointEnd) || (isAcceptanceRadius && point2.DistanceTo(x, y, z) < acceptanceRadius))
                {
                    //world.Log.Log("Pathfind.CountEnd {0}", count);
                    return CreateEntityPath(pointBegin, point2, true);
                }

                if (point2.DistanceToSquared(pointEnd) < point.DistanceToSquared(pointEnd))
                {
                    point = point2;
                }

                point2.visited = true;
                int countSide = nodeProcessor.FindPathOptions(pathOptions, entity, point2, pointEnd, distance);

                for (int i = 0; i < countSide; ++i)
                {
                    PathPoint point3 = pathOptions[i];
                    float distance2 = point2.totalPathDistance + point2.DistanceToSquared(point3);

                    if (distance2 < distance * 2f && (!point3.IsAssigned() || distance2 < point3.totalPathDistance))
                    {
                        point3.previous = point2;
                        point3.totalPathDistance = distance2;
                        point3.distanceToNext = point3.DistanceToSquared(pointEnd);

                        if (point3.IsAssigned())
                        {
                            path.ChangeDistance(point3, point3.totalPathDistance + point3.distanceToNext);
                        }
                        else
                        {
                            point3.distanceToTarget = point3.totalPathDistance + point3.distanceToNext;
                            path.AddPoint(point3);
                        }
                    }
                }
                if (++count >= PathNavigate.PATH_MAXIMUN_LENGTH) break;
            }
            //world.Log.Log("Pathfind.Count {0}", count);
            return point == pointBegin ? null : CreateEntityPath(pointBegin, point, point.Equals(pointEnd));
        }

        /// <summary>
        /// Возвращает новый путь сущности для данной начальной и конечной точки
        /// </summary>
        /// <param name="pointBegin">начальная точка</param>
        /// <param name="pointEnd">конечная точка</param>
        /// <param name="isDestinationSame">Является ли пункт назначения одинаковым</param>
        private PathEntity CreateEntityPath(PathPoint pointBegin, PathPoint pointEnd, bool isDestinationSame)
        {
            // Отладка, последняя точка куда идти
            //PathEntity.DebugEnd(world, pointEnd);

            int step = 1;
            PathPoint point;
            
            for (point = pointEnd; point.previous != null; point = point.previous)
            {
                ++step;
            }

            if (step < 2) return null;

            PathPoint[] points = new PathPoint[step - 1];
            point = pointEnd;
            step -= 2;

            for (points[step] = pointEnd; point.previous != null && step > 0; points[step] = point)
            {
                point = point.previous;
                --step;
            }

            return new PathEntity(points, isDestinationSame);
        }
    }
}
