using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь навигации по земле
    /// </summary>
    public class PathNavigateGround : PathNavigate
    {
        /// <summary>
        /// Ограничитель солнца, если true - то сущность будет избегать прямые блоки от солнца
        /// </summary>
        public bool restrictSun = false;

        private WalkNodeProcessor nodeProcessor;
        
        /// <summary>
        /// Путь навигации по земле
        /// </summary>
        public PathNavigateGround(EntityLiving entity, WorldBase worldIn) : base(entity, worldIn) { }

        /// <summary>
        /// Сущность может плавать
        /// </summary>
        public void CanSwim(bool canSwim) => nodeProcessor.canSwim = canSwim;
        /// <summary>
        /// Следует избегать воды
        /// </summary>
        public void SetAvoidWater(bool avoidWater) => nodeProcessor.avoidsWater = avoidWater;
        /// <summary>
        /// Следует избегать лавы и огня
        /// </summary>
        public void SetAvoidLavaOrFire(bool avoidLavaOrFire) => nodeProcessor.avoidsLavaOrFire = avoidLavaOrFire;

        /// <summary>
        /// Получить объект PathFinder
        /// </summary>
        protected override PathFinder GetPathFinder()
        {
            nodeProcessor = new WalkNodeProcessor();
            return new PathFinder(world, nodeProcessor);
        }

        /// <summary>
        /// Если на земле или в плавании и может плавать
        /// </summary>
        protected override bool CanNavigate() 
            => theEntity.OnGround || nodeProcessor.canSwim && IsInLiquid()/* || theEntity.IsRiding() && theEntity instanceof EntityZombie && this.theEntity.ridingEntity instanceof EntityChicken*/;

        protected override vec3 GetEntityPosition() 
            => new vec3(theEntity.Position.x, GetPositionHeight(), theEntity.Position.z);

        /// <summary>
        /// Определить высоту позиции сущности
        /// </summary>
        private int GetPositionHeight()
        {
            if (theEntity.IsInWater() && nodeProcessor.canSwim)
            {
                int y = (int)theEntity.Position.y;
                BlockBase block = world.GetBlockState(new BlockPos(Mth.Floor(theEntity.Position.x), y, Mth.Floor(theEntity.Position.z))).GetBlock();
                int count = 0;

                do
                {
                    if (block.Material != EnumMaterial.Water)
                    {
                        return y;
                    }

                    ++y;
                    block = world.GetBlockState(new BlockPos(Mth.Floor(theEntity.Position.x), y, Mth.Floor(theEntity.Position.z))).GetBlock();
                    ++count;
                }
                while (count <= 16);

                return (int)theEntity.Position.y;
            }
            else
            {
                return (int)(theEntity.Position.y + .5f);
            }
        }

        /// <summary>
        /// Обрезает данные пути от конца до первого блока, покрытого солнцем
        /// </summary>
        protected override void RemoveSunnyPath()
        {
            base.RemoveSunnyPath();
            if (restrictSun)
            {
                if (world.IsAgainstSky(new BlockPos(Mth.Floor(theEntity.Position.x),
                    (int)(theEntity.Position.y + .5f), Mth.Floor(theEntity.Position.z))))
                {
                    return;
                }

                for (int i = 0; i < currentPath.GetCurrentPathLength(); ++i)
                {
                    PathPoint var2 = currentPath.GetPathPointFromIndex(i);

                    if (world.IsAgainstSky(new BlockPos(var2.xCoord, var2.yCoord, var2.zCoord)))
                    {
                        currentPath.SetCurrentPathLength(i - 1);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает true, если объект указанного размера может безопасно пройти по прямой линии между двумя точками
        /// </summary>
        protected override bool IsDirectPathBetweenPoints(vec3 pos1, vec3 pos2, int sizeX, int sizeY, int sizeZ)
        {
            int x = Mth.Floor(pos1.x);
            int z = Mth.Floor(pos1.z);
            float vx = pos2.x - pos1.x;
            float vz = pos2.z - pos1.z;
            float sq = vx * vx + vz * vz;

            if (sq < .000001f) return false;

            float k = 1f / Mth.Sqrt(sq);
            vx *= k;
            vz *= k;
            sizeX += 2;
            sizeZ += 2;

            if (!CheckMove(x, (int)pos1.y, z, sizeX, sizeY, sizeZ, pos1, vx, vz))
            {
                return false;
            }
            
            sizeX -= 2;
            sizeZ -= 2;
            float kx = 1f / Mth.Abs(vx);
            float kz = 1f / Mth.Abs(vz);
            float vx2 = x - pos1.x;
            float vz2 = z - pos1.z;

            if (vx >= 0f) ++vx2;
            if (vz >= 0f) ++vz2;

            vx2 /= vx;
            vz2 /= vz;
            int nx = vx < 0 ? -1 : 1;
            int nz = vz < 0 ? -1 : 1;
            int x2 = Mth.Floor(pos2.x);
            int z2 = Mth.Floor(pos2.z);
            int v2x = x2 - x;
            int v2z = z2 - z;

            do
            {
                if (v2x * nx <= 0 && v2z * nz <= 0) return true;

                if (vx2 < vz2)
                {
                    vx2 += kx;
                    x += nx;
                    v2x = x2 - x;
                }
                else
                {
                    vz2 += kz;
                    z += nz;
                    v2z = z2 - z;
                }
            }
            while (CheckMove(x, (int)pos1.y, z, sizeX, sizeY, sizeZ, pos1, vx, vz));

            return false;
        }

        /// <summary>
        /// Можем ли мы пройти в этой части и не упасть
        /// </summary>
        private bool CheckMove(int posX, int posY, int posZ, int sizeX, int sizeY, int sizeZ, vec3 pos, float vecX, float vecZ)
        {
            int x0 = posX - sizeX / 2;
            int z0 = posZ - sizeZ / 2;
            float x2, z2;
            BlockPos blockPos;
            BlockState blockState;

            BlockPos[] blocks = BlockPos.GetAllInBox(new vec3i(x0, posY, z0), new vec3i(x0 + sizeX - 1, posY + sizeY - 1, z0 + sizeZ - 1));
            for (int i = 0; i < blocks.Length; i++)
            {
                blockPos = blocks[i];
                x2 = blockPos.X + .5f - pos.x;
                z2 = blockPos.Z + .5f - pos.z;

                if (x2 * vecX + z2 * vecZ >= 0f)
                {
                    blockState = world.GetBlockState(blockPos);
                    if (!blockState.GetBlock().IsPassableOnIt(blockState.met))
                    {
                        return false;
                    }
                }
            }

            // Определить что под ногами
            for (int x = x0; x < x0 + sizeX; ++x)
            {
                for (int z = z0; z < z0 + sizeZ; ++z)
                {
                    x2 = x + .5f - pos.x;
                    z2 = z + .5f - pos.z;
                    if (x2 * vecX + z2 * vecZ >= 0.0f)
                    {
                        EnumMaterial material = world.GetBlockState(new BlockPos(x, posY - 1, z)).GetBlock().Material;

                        if (material == EnumMaterial.Air
                            || (material == EnumMaterial.Water && !theEntity.IsInWater())
                            || material == EnumMaterial.Lava || material == EnumMaterial.Oil)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
