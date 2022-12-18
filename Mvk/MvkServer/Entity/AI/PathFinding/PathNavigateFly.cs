using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь навигации летать
    /// </summary>
    public class PathNavigateFly : PathNavigateSwimmer
    {
        /// <summary>
        /// Путь навигации летать
        /// </summary>
        public PathNavigateFly(EntityLiving entity, WorldBase worldIn) : base(entity, worldIn) { }

        /// <summary>
        /// Получить объект PathFinder
        /// </summary>
        protected override PathFinder GetPathFinder() => new PathFinder(world, new SwimOrFlyNodeProcessor(EnumMaterial.Air));

        /// <summary>
        /// Если на земле или в плавании и может плавать
        /// </summary>
        protected override bool CanNavigate() => true;

        /// <summary>
        /// Возвращает true, если объект указанного размера может безопасно пройти по прямой линии между двумя точками
        /// </summary>
        protected override bool IsDirectPathBetweenPoints(vec3 pos1, vec3 pos2, int sizeX, int sizeY, int sizeZ)
        {
            pos2.y += theEntity.Height * .5f;
            vec3 vec = pos2 - pos1;
            MovingObjectPosition moving = world.RayCastBlock(pos1, vec.normalize(), glm.distance(vec), true);
            return moving == null || moving.IsEntity();
        }
    }
}
