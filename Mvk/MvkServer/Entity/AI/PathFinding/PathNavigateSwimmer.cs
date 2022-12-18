using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь навигации пловец
    /// </summary>
    public class PathNavigateSwimmer : PathNavigate
    {
        /// <summary>
        /// Путь навигации пловец
        /// </summary>
        public PathNavigateSwimmer(EntityLiving entity, WorldBase worldIn) : base(entity, worldIn) { }

        /// <summary>
        /// Получить объект PathFinder
        /// </summary>
        protected override PathFinder GetPathFinder() => new PathFinder(world, new SwimOrFlyNodeProcessor(EnumMaterial.Water));

        /// <summary>
        /// Если на земле или в плавании и может плавать
        /// </summary>
        protected override bool CanNavigate() => IsInLiquid();

        protected override vec3 GetEntityPosition() 
            => new vec3(theEntity.Position.x, theEntity.Position.y + theEntity.Height * .5f, theEntity.Position.z);

        /// <summary>
        /// Следовать пути
        /// </summary>
        protected override void PathFollow()
        {
            vec3 pos = GetEntityPosition();
            float squareDistance = theEntity.Width * theEntity.Width * 4;

            if (glm.SquareDistanceTo(pos, currentPath.GetVectorFromIndex(theEntity, currentPath.GetCurrentPathIndex())) < squareDistance)
            {
                currentPath.IncrementPathIndex();
            }

            for (int i = Mth.Min(currentPath.GetCurrentPathIndex() + 6, currentPath.GetCurrentPathLength() - 1); 
                i > currentPath.GetCurrentPathIndex(); --i)
            {
                vec3 vec = currentPath.GetVectorFromIndex(theEntity, i);
                if (glm.SquareDistanceTo(vec, pos) <= 36f && IsDirectPathBetweenPoints(pos, vec, 0, 0, 0))
                {
                    currentPath.SetCurrentPathIndex(i);
                    break;
                }
            }

            CheckForStuck(pos);
        }

        /// <summary>
        /// Возвращает true, если объект указанного размера может безопасно пройти по прямой линии между двумя точками
        /// </summary>
        protected override bool IsDirectPathBetweenPoints(vec3 pos1, vec3 pos2, int sizeX, int sizeY, int sizeZ)
        {
            pos2.y += theEntity.Height * .5f;
            vec3 vec = pos2 - pos1;
            MovingObjectPosition moving = world.RayCastBlock(pos1, vec.normalize(), glm.distance(vec), true, true);
            return moving == null || moving.IsLiquid || moving.IsEntity();
        }
    }
}
