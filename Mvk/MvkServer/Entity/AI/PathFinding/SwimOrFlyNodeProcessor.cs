using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Процессор плавательного или летательного узла
    /// </summary>
    public class SwimOrFlyNodeProcessor : NodeProcessor
    {
        /// <summary>
        /// Материал где плаваем, вода или воздух, можно и лаву
        /// </summary>
        private readonly EnumMaterial material;

        public SwimOrFlyNodeProcessor(EnumMaterial material) => this.material = material;

        /// <summary>
        /// Возвращает точку пути куда надо придти
        /// </summary>
        public override PathPoint GetPathPointToCoords(EntityLiving entity, float x, float y, float z) 
            => pathPointEnd = OpenPoint(Mth.Floor(x), Mth.Floor(y), Mth.Floor(z));

        /// <summary>
        /// Возвращает точку пути от куда стартуем
        /// </summary>
        public override PathPoint GetPathPointTo(EntityLiving entity) 
            => OpenPoint(Mth.Floor(entity.Position.x), Mth.Floor(entity.BoundingBox.Min.y + .5f), Mth.Floor(entity.Position.z));

        /// <summary>
        /// Найти параметры пути
        /// </summary>
        public override int FindPathOptions(PathPoint[] points, EntityLiving entity, PathPoint pointBegin, PathPoint pointEnd, float distance)
        {
            int index = 0;
            vec3i vec;
            PathPoint point;

            for (int i = 0; i < 6; i++)
            {
                vec = MvkStatic.ArraOne3d6[i];
                point = GetVerticalOffset(pointBegin.xCoord + vec.x, pointBegin.yCoord + vec.y, pointBegin.zCoord + vec.z);
                if (point != null && !point.visited && point.DistanceTo(pointEnd) < distance)
                {
                    points[index++] = point;
                }
            }
            return index;
        }

        private PathPoint GetVerticalOffset(int posX, int posY, int posZ)
        {
            int key = CheckBlocks(posX, posY, posZ);
            return key == -1 ? OpenPoint(posX, posY, posZ) : null;
        }

        /// <summary>
        /// Проверка блока, возвращаем:
        /// 0 - блок не воды и не нефти
        /// -1 - блоки только воды и нефти
        /// </summary>
        private int CheckBlocks(int posX, int posY, int posZ)
        {
            for (int x = posX; x < posX + sizeXZ; ++x)
            {
                for (int y = posY; y < posY + sizeY; ++y)
                {
                    for (int z = posZ; z < posZ + sizeXZ; ++z)
                    {
                        if (world.GetBlockState(new BlockPos(x, y, z)).GetBlock().Material != material) return 0;
                    }
                }
            }
            return -1;
        }
    }
}
