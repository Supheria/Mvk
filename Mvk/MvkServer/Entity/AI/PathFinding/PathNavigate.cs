using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Абстрактный класс навигации моба
    /// </summary>
    public abstract class PathNavigate
    {
        /// <summary>
        /// Путь максимальной длинны, да обрезания бесмысленного поиска обхода.
        /// Значение должно быть примерно по формуле Sqrt(PATH_SEARCH_RANGE^2 * 2) * 2
        /// </summary>
        public const int PATH_MAXIMUN_LENGTH = 44;
        /// <summary>
        /// Максимальное расстояние, на котором будет выполняться поиск пути.
        /// Количество блоков (дополнительных) +/- на каждой оси, которые извлекаются в качестве кеша 
        /// для пространства поиска навигатора
        /// </summary>
        private const float PATH_SEARCH_RANGE = 16f;
        /// <summary>
        /// Через какое количество тиков прерывается перемещение, если сущность не перемещалось
        /// </summary>
        private const int COUNT_TICK_NOT_PATH = 50;

        protected EntityLiving theEntity; 
        protected WorldBase world;

        /// <summary>
        /// Отслеживаемый объект
        /// </summary>
        protected PathEntity currentPath;
        /// <summary>
        /// Скорость перемещения
        /// </summary>
        protected float speed;
        /// <summary>
        /// Время в тиках по текущему пути
        /// </summary>
        private int totalTicks;
        /// <summary>
        /// Время последней проверки позиции (для обнаружения успешного движения)
        /// </summary>
        private int ticksAtLastPos;
        /// <summary>
        /// Координаты положения объекта в последний раз, когда выполнялась проверка (часть мониторинга «зависания»)
        /// </summary>
        private vec3 lastPosCheck = new vec3(0);

        protected readonly PathFinder pathFinder;

        public PathNavigate(EntityLiving entity, WorldBase worldIn)
        {
            theEntity = entity;
            world = worldIn;
            pathFinder = GetPathFinder();
        }

        #region Опции

        /// <summary>
        /// Задать расстояние до точки, которое будет считаться выполненым,
        /// 0 - расчёта нет
        /// </summary>
        public void SetAcceptanceRadius(float acceptanceRadius) => pathFinder.SetAcceptanceRadius(acceptanceRadius);
        /// <summary>
        /// Получить расстояние до точки, которое будет считаться выполненым,
        /// 0 - расчёта нет
        /// </summary>
        public float GetAcceptanceRadius() => pathFinder.GetAcceptanceRadius();

        /// <summary>
        /// Задать остановку при соприкосновении коллизии
        /// true - соприкосновении коллизии, false - центр
        /// </summary>
        public void SetStopOnOverlap(bool stopOnOverlap) => pathFinder.SetStopOnOverlap(stopOnOverlap);
        /// <summary>
        /// Остановка при соприкосновении коллизии
        /// true - соприкосновении коллизии, false - центр
        /// </summary>
        public bool GetStopOnOverlap() => pathFinder.GetStopOnOverlap();

        #endregion

        /// <summary>
        /// Получить объект PathFinder
        /// </summary>
        protected abstract PathFinder GetPathFinder();
        /// <summary>
        /// Может ли перемещаться
        /// </summary>
        protected abstract bool CanNavigate();
        /// <summary>
        /// Тикущая позиция сущности
        /// </summary>
        protected abstract vec3 GetEntityPosition();
        /// <summary>
        /// Возвращает true, если объект указанного размера может безопасно пройти по прямой линии между двумя точками
        /// </summary>
        protected abstract bool IsDirectPathBetweenPoints(vec3 pos1, vec3 pos2, int sizeX, int sizeY, int sizeZ);

        /// <summary>
        /// Обрезает данные пути от конца до первого блока, покрытого солнцем
        /// </summary>
        protected virtual void RemoveSunnyPath() { }

        /// <summary>
        /// Возвращает true, если объект находится в воде, лаве или нефте, иначе false
        /// </summary>
        protected bool IsInLiquid() => theEntity.IsInWater() || theEntity.IsInOil() || theEntity.IsInLava();

        /// <summary>
        /// Возвращает путь к заданным координатам
        /// </summary>
        public virtual PathEntity GetPathToXYZ(BlockPos blockPos)
        {
            if (!CanNavigate()) return null;
            return pathFinder.CreateEntityPathTo(world, theEntity, blockPos, PATH_SEARCH_RANGE);
        }

        /// <summary>
        /// Возвращает путь к заданному EntityLiving
        /// </summary>
        public virtual PathEntity GetPathToEntityLiving(EntityLiving entity)
        {
            if (!CanNavigate()) return null;
            return pathFinder.CreateEntityPathTo(world, theEntity, entity, PATH_SEARCH_RANGE);
        }

        /// <summary>
        /// Попробуйте найти и указать путь к XYZ. Возвращает true в случае успеха
        /// </summary>
        /// <param name="allowPartialPath">Разрешить частичный путь</param>
        public bool TryMoveToXYZ(float x, float y, float z, float speed, bool allowPartialPath = true)
        {
            pathFinder.SetStopOnOverlap(false);
            BlockPos blockPos = new BlockPos(x, y, z);
            PathEntity path = GetPathToXYZ(blockPos);
            // Проверка на доступ чтоб сущность могла дойти до конечной точки
            if (!allowPartialPath && path != null && !path.IsDestinationSame()) return false;
            return SetPath(path, speed);
        }

        /// <summary>
        /// Попробуйте найти и указать путь к EntityLiving. Возвращает true в случае успеха
        /// </summary>
        /// <param name="allowPartialPath">Разрешить частичный путь </param>
        public virtual bool TryMoveToEntityLiving(EntityLiving entity, float speed, bool allowPartialPath = true)
        {
            pathFinder.SetStopOnOverlap(false);
            PathEntity path = GetPathToEntityLiving(entity);
            // Проверка на доступ чтоб сущность могла дойти до конечной точки
            if (!allowPartialPath && path != null && !path.IsDestinationSame()) return false;
            return SetPath(path, speed);
        }

        /// <summary>
        /// Задает новый путь. Если он отличается от старого пути. 
        /// Проверяет корректировку пути для избегания солнца и сохраняет начальные координаты.
        /// </summary>
        public bool SetPath(PathEntity path, float speed)
        {
            if (path == null)
            {
                currentPath = null;
                return false;
            }
            if (!path.IsSamePath(currentPath))
            {
                currentPath = path;
            }

            // Отладка, визуализация перемещения
            //path.DebugPath(worldObj);

            RemoveSunnyPath();

            if (currentPath.GetCurrentPathLength() == 0)
            {
                return false;
            }

            this.speed = speed;
            ticksAtLastPos = totalTicks;
            lastPosCheck = GetEntityPosition();
            return true;
        }

        /// <summary>
        /// Если путь нулевой или достигнут конец
        /// </summary>
        public bool NoPath() => currentPath == null || currentPath.IsFinished();

        /// <summary>
        /// Устанавливает для активного PathEntity значение null
        /// </summary>
        public void ClearPathEntity() => currentPath = null;

        /// <summary>
        /// Получает активно используемый путь
        /// </summary>
        public PathEntity GetPath() => currentPath;

        public virtual void OnUpdateNavigation()
        {
            ++totalTicks;
            if (!NoPath())
            {
                vec3 pos;

                if (CanNavigate())
                {
                    PathFollow();
                }
                else if (currentPath != null && currentPath.GetCurrentPathIndex() < currentPath.GetCurrentPathLength())
                {
                    pos = GetEntityPosition();
                    vec3 pos2 = currentPath.GetVectorFromIndex(theEntity, currentPath.GetCurrentPathIndex());

                    if (pos.y > pos2.y && !theEntity.OnGround && Mth.Floor(pos.x) == Mth.Floor(pos2.x) && Mth.Floor(pos.z) == Mth.Floor(pos2.z))
                    {
                        currentPath.SetCurrentPathIndex(currentPath.GetCurrentPathIndex() + 1);
                    }
                }

                if (!NoPath())
                {
                    theEntity.GetMoveHelper().SetMoveTo(currentPath.GetPosition(theEntity), speed);
                }
            }
        }

        /// <summary>
        /// Следовать пути
        /// </summary>
        protected virtual void PathFollow()
        {
            vec3 pos = GetEntityPosition();
            int leght = currentPath.GetCurrentPathLength();
            int i;

            for (i = currentPath.GetCurrentPathIndex(); i < currentPath.GetCurrentPathLength(); ++i)
            {
                if (currentPath.GetPathPointFromIndex(i).yCoord != (int)pos.y)
                {
                    leght = i;
                    break;
                }
            }
            
            float squareDistance = pathFinder.GetStopOnOverlap() 
                ? theEntity.Width * theEntity.Width * 4 : .0625f;

            for (i = currentPath.GetCurrentPathIndex(); i < leght; ++i)
            {
                vec3 vec = currentPath.GetVectorFromIndex(theEntity, i);
                
                if (glm.SquareDistanceTo(pos, vec) < squareDistance)
                {
                    currentPath.SetCurrentPathIndex(i + 1);
                }
            }

            i = Mth.Ceiling(theEntity.Width * 2f);
            int height = (int)theEntity.Height + 1;

            for (int j = leght - 1; j >= currentPath.GetCurrentPathIndex(); --j)
            {
                if (IsDirectPathBetweenPoints(pos, currentPath.GetVectorFromIndex(theEntity, j), i, height, i))
                {
                    currentPath.SetCurrentPathIndex(j);
                    break;
                }
            }

            CheckForStuck(pos);
        }

        /// <summary>
        /// Проверяет, не был ли объект перемещен при последней проверке, и если да, очищает текущий PathEntity
        /// </summary>
        protected void CheckForStuck(vec3 pos)
        {
            if (totalTicks - ticksAtLastPos > COUNT_TICK_NOT_PATH)
            {
                if (glm.SquareDistanceTo(pos, lastPosCheck) < 2.25f)
                {
                    ClearPathEntity();
                }
                ticksAtLastPos = totalTicks;
                lastPosCheck = pos;
            }
        }
    }
}
