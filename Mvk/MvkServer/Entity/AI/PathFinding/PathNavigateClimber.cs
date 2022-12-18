using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь навигации альпиниста (паук)
    /// </summary>
    public class PathNavigateClimber : PathNavigateGround
    {
        private BlockPos posClimber;
        private bool isPosClimber;

        /// <summary>
        /// Путь навигации альпиниста (паук)
        /// </summary>
        public PathNavigateClimber(EntityLiving entity, WorldBase worldIn) : base(entity, worldIn) { }

        /// <summary>
        /// Возвращает путь к заданным координатам
        /// </summary>
        public override PathEntity GetPathToXYZ(BlockPos blockPos)
        {
            posClimber = blockPos;
            isPosClimber = true;
            return base.GetPathToXYZ(blockPos);
        }

        /// <summary>
        /// Возвращает путь к заданному EntityLiving
        /// </summary>
        public override PathEntity GetPathToEntityLiving(EntityLiving entity)
        {
            posClimber = new BlockPos(entity.Position);
            isPosClimber = true;
            return base.GetPathToEntityLiving(entity);
        }

        /// <summary>
        /// Попробуйте найти и указать путь к EntityLiving. Возвращает true в случае успеха
        /// </summary>
        public override bool TryMoveToEntityLiving(EntityLiving entity, float speed, bool allowPartialPath = true)
        {
            pathFinder.SetStopOnOverlap(true);
            PathEntity path = GetPathToEntityLiving(entity);
            if (path != null)
            {
                return SetPath(path, speed);
            }
            posClimber = new BlockPos(entity.Position);
            isPosClimber = true;
            this.speed = speed;
            return true;
        }

        public override void OnUpdateNavigation()
        {
            if (!NoPath())
            {
                base.OnUpdateNavigation();
            }
            else if (isPosClimber)
            {
                float k = theEntity.Width * theEntity.Width * 4f;
                if (glm.SquareDistanceTo(posClimber.ToVec3() + .5f, theEntity.Position) >= k
                    && (theEntity.Position.y <= posClimber.Y
                        || glm.SquareDistanceTo(new vec3(posClimber.X, Mth.Floor(theEntity.Position.y), posClimber.Z), theEntity.Position) >= k))
                {
                    theEntity.GetMoveHelper().SetMoveTo(posClimber.X, posClimber.Y, posClimber.Z, speed);
                }
                else
                {
                    isPosClimber = false;
                }
            }
        }
    }
}
