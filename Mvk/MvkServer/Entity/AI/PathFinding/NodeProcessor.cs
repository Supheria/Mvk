using MvkServer.Util;
using MvkServer.World;
using System.Collections.Generic;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Процесс узла перемещения
    /// </summary>
    public abstract class NodeProcessor
    {
        protected WorldBase world;
        protected Dictionary<int, PathPoint> map = new Dictionary<int, PathPoint>();
        protected int sizeXZ;
        protected int sizeY;
        protected PathPoint pathPointEnd;

        /// <summary>
        /// Инициализация
        /// </summary>
        public virtual void InitProcessor(WorldBase world, EntityLiving entity)
        {
            this.world = world;
            map.Clear();
            sizeXZ = Mth.Floor(entity.Width * 2 + 1f);
            sizeY = Mth.Floor(entity.Height + 1);
        }

        /// <summary>
        /// Этот метод вызывается после обработки всех узлов и создания PathEntity
        /// </summary>
        public virtual void PostProcess() { }

        /// <summary>
        /// Возвращает сопоставленную точку или создает и добавляет ее  
        /// </summary>
        protected PathPoint OpenPoint(int x, int y, int z)
        {
            int hash = PathPoint.MakeHash(x, y, z);

            if (!map.ContainsKey(hash))
            {
                PathPoint point = new PathPoint(x, y, z);
                map.Add(hash, point);
                return point;
            }
            else if (pathPointEnd != null && pathPointEnd.GetHashCode() == hash)
            {
                return pathPointEnd;
            }

            return null;
        }

        /// <summary>
        /// Возвращает точку пути от куда стартуем
        /// </summary>
        public abstract PathPoint GetPathPointTo(EntityLiving entity);

        /// <summary>
        /// Возвращает точку пути куда надо придти
        /// </summary>
        public abstract PathPoint GetPathPointToCoords(EntityLiving entity, float x, float y, float z);

        /// <summary>
        /// Найти параметры пути
        /// </summary>
        public abstract int FindPathOptions(PathPoint[] points, EntityLiving entity, PathPoint pointBegin, PathPoint pointEnd, float distance);
    }
}
