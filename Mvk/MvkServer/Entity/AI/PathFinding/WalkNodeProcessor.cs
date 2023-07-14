using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.AI.PathFinding
{
    /// <summary>
    /// Процессор узла ходьбы
    /// </summary>
    public class WalkNodeProcessor : NodeProcessor
    {
        /// <summary>
        /// Может плавать
        /// </summary>
        public bool canSwim = false;
        /// <summary>
        /// Избегает воды 
        /// </summary>
        public bool avoidsWater = true;
        /// <summary>
        /// Следует избегать воды
        /// </summary>
        private bool shouldAvoidWater = true;
        /// <summary>
        /// Избегает лавы и огня
        /// </summary>
        public bool avoidsLavaOrFire = true;

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void InitProcessor(WorldBase world, EntityLiving entity)
        {
            base.InitProcessor(world, entity);
            shouldAvoidWater = avoidsWater;
        }

        /// <summary>
        /// Этот метод вызывается после обработки всех узлов и создания PathEntity
        /// </summary>
        public override void PostProcess()
        {
            base.PostProcess();
            avoidsWater = shouldAvoidWater;
        }

        /// <summary>
        /// Возвращает точку пути куда надо придти
        /// </summary>
        public override PathPoint GetPathPointToCoords(EntityLiving entity, float x, float y, float z) 
            => pathPointEnd = OpenPoint(Mth.Floor(x), Mth.Floor(y), Mth.Floor(z));

        /// <summary>
        /// Возвращает точку пути от куда стартуем
        /// </summary>
        public override PathPoint GetPathPointTo(EntityLiving entity)
        {
            int y;

            if (canSwim && entity.IsInWater())
            {
                // если в воде и избегает воды, выплываем вверх, и помечаем, что не избегаем воды
                y = (int)entity.BoundingBox.Min.y;

                for (BlockBase block = world.GetBlockState(new BlockPos(Mth.Floor(entity.Position.x), y, Mth.Floor(entity.Position.z))).GetBlock(); 
                    block.Material.EMaterial == EnumMaterial.Water;
                    block = world.GetBlockState(new BlockPos(Mth.Floor(entity.Position.x), y, Mth.Floor(entity.Position.z))).GetBlock())
                {
                    ++y;
                }

                avoidsWater = false;
            }
            else
            {
                y = Mth.Floor(entity.BoundingBox.Min.y + .5f);
            }

            return OpenPoint(Mth.Floor(entity.Position.x), y, Mth.Floor(entity.Position.z));
        }

        /// <summary>
        /// Найти параметры пути
        /// </summary>
        public override int FindPathOptions(PathPoint[] points, EntityLiving entity, PathPoint pointBegin, PathPoint pointEnd, float distance)
        {
            int index = 0;
            byte up = 0;

            // Проверяем состояние, можем ли мы залесть на один блок при необходимости
            if (!CheckBlocksCollision(pointBegin.xCoord, pointBegin.yCoord + 1, pointBegin.zCoord, true))
            {
                up = 1;
            }

            // Проверяем возможность перемещения в любую сторону
            PathPoint pointSouth = GetSafePoint(entity, pointBegin.xCoord, pointBegin.yCoord, pointBegin.zCoord + 1, up);
            PathPoint pointWest = GetSafePoint(entity, pointBegin.xCoord - 1, pointBegin.yCoord, pointBegin.zCoord, up);
            PathPoint pointEast = GetSafePoint(entity, pointBegin.xCoord + 1, pointBegin.yCoord, pointBegin.zCoord, up);
            PathPoint pointNorth = GetSafePoint(entity, pointBegin.xCoord, pointBegin.yCoord, pointBegin.zCoord - 1, up);

            PathPoint pointSouthWest = GetSafePoint(entity, pointBegin.xCoord - 1, pointBegin.yCoord, pointBegin.zCoord + 1, up);
            PathPoint pointSouthEast = GetSafePoint(entity, pointBegin.xCoord + 1, pointBegin.yCoord, pointBegin.zCoord + 1, up);
            PathPoint pointNorthWest = GetSafePoint(entity, pointBegin.xCoord - 1, pointBegin.yCoord, pointBegin.zCoord - 1, up);
            PathPoint pointNorthEast = GetSafePoint(entity, pointBegin.xCoord + 1, pointBegin.yCoord, pointBegin.zCoord - 1, up);

            if (pointSouth != null && !pointSouth.visited && pointSouth.DistanceTo(pointEnd) < distance) points[index++] = pointSouth;
            if (pointWest != null && !pointWest.visited && pointWest.DistanceTo(pointEnd) < distance) points[index++] = pointWest;
            if (pointEast != null && !pointEast.visited && pointEast.DistanceTo(pointEnd) < distance) points[index++] = pointEast;
            if (pointNorth != null && !pointNorth.visited && pointNorth.DistanceTo(pointEnd) < distance) points[index++] = pointNorth;

            if (pointSouthWest != null && !pointSouthWest.visited && pointSouthWest.DistanceTo(pointEnd) < distance) points[index++] = pointSouthWest;
            if (pointSouthEast != null && !pointSouthEast.visited && pointSouthEast.DistanceTo(pointEnd) < distance) points[index++] = pointSouthEast;
            if (pointNorthWest != null && !pointNorthWest.visited && pointNorthWest.DistanceTo(pointEnd) < distance) points[index++] = pointNorthWest;
            if (pointNorthEast != null && !pointNorthEast.visited && pointNorthEast.DistanceTo(pointEnd) < distance) points[index++] = pointNorthEast;

            return index;
        }

        /// <summary>
        /// Возвращает точку, в которую сущность может безопасно переместиться
        /// </summary>
        private PathPoint GetSafePoint(EntityLiving entity, int x, int y, int z, int up)
        {
            PathPoint point = null;
            // Проверяем тикущее состояние
            if (!CheckBlocksCollision(x, y, z, true))
            {
                point = OpenPoint(x, y, z);
            }

            // Проверяем возможность запрыгнуть если есть возможность up == 1
            if (point == null && up == 1 && !CheckBlocksCollision(x, y + up, z, false))
            {
                point = OpenPoint(x, y + up, z);
                y += up;
            }

            if (point != null)
            {
                // Всё-таки можно находится, надо проверить, что под нагами
                int height = 0;
                int i;

                for (i = 0; y > 0; point = OpenPoint(x, y, z))
                {
                    // Проверяем что под ногами
                    i = CheckRowBlocks(x, y - 1, z);

                    if ((avoidsWater && i == -1) || (avoidsLavaOrFire && i == -3) || i == -2)
                    {
                        // Если вода, если избегаем воду или любая опастность (лава, огонь, нефть)
                        return null;
                    }

                    if (i != 1)
                    {
                        // Нет столкновения
                        break;
                    }

                    if (height++ >= entity.GetMaxFallHeight())
                    {
                        return null;
                    }

                    --y;

                    if (y <= 0)
                    {
                        // если пытаемся падать в пропость
                        return null;
                    }
                }
            }

            return point;
        }

        /// <summary>
        /// Проверьте блоки тела, для коллизии, возвращаем:
        /// true - столкновение, false - можно перемещаться
        /// </summary>
        /// <param name="isCollision">true - чек по колизии, false - чек по IsPassable</param>
        private bool CheckBlocksCollision(int posX, int posY, int posZ, bool isCollision)
        {
            BlockState blockState;
            BlockBase block;
            MaterialBase material;
            EnumMaterial eMaterial;

            for (int x = posX; x < posX + sizeXZ; ++x)
            {
                for (int y = posY; y < posY + sizeY; ++y)
                {
                    for (int z = posZ; z < posZ + sizeXZ; ++z)
                    {
                        blockState = world.GetBlockState(new BlockPos(x, y, z));
                        if (!blockState.IsAir()) // Не воздух
                        {
                            block = blockState.GetBlock();
                            material = block.Material;
                            eMaterial = material.EMaterial;

                            if ((isCollision && block.IsCollidable) || (!isCollision && !block.IsPassable(blockState.met))) 
                            {
                                // столкновении с любым сплошным блоком и дверь если надо
                                return true;
                            }
                            if (eMaterial == EnumMaterial.Water && avoidsWater)
                            {
                                // столкновении с водой (если избегает воды)
                                return true;
                            }
                            if (avoidsLavaOrFire && material.Ignites)
                            {
                                // столкновении с лавой или огнём
                                return true;
                            }
                            if (eMaterial == EnumMaterial.Oil)
                            {
                                // столкновении с нефтью
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Проверьте блоки ряда снизу, чтоб можно было упасть, возвращаем:
        /// 1 -  нет столкновений для перемещения по блокам,
        /// 0 -  столкновении с любым сплошным блоком, можно ходить, !приоритет 
        /// -1 - столкновении с водой, 
        /// -2 - столкновении с нефтью,
        /// -3 - столкновении с лавой или огнём
        /// </summary>
        private int CheckRowBlocks(int posX, int posY, int posZ)
        {
            BlockState blockState;
            BlockBase block;
            MaterialBase material;
            EnumMaterial eMaterial;
            // имеется блок воды
            bool isWater = false;
            // имеется блок нефти
            bool isOil = false;
            // имеется блок лавы или огня
            bool isLavaOrFile = false;

            for (int x = posX; x < posX + sizeXZ; ++x)
            {
                for (int z = posZ; z < posZ + sizeXZ; ++z)
                {
                    blockState = world.GetBlockState(new BlockPos(x, posY, z));
                    if (!blockState.IsAir()) // Не воздух
                    {
                        block = blockState.GetBlock();
                        material = block.Material;
                        eMaterial = material.EMaterial;

                        if (eMaterial == EnumMaterial.Water)
                        {
                            // столкновении с водой
                            isWater = true;
                        }
                        else if (eMaterial == EnumMaterial.Oil)
                        {
                            // столкновении с нефтью
                            isOil = true;
                        }
                        else if (material.Ignites)
                        {
                            // столкновении с лавой или огнём
                            isLavaOrFile = true;
                        }
                        else if (block.IsPassableOnIt(blockState.met))
                        {
                            // Можно ходить
                            return 0;
                        }
                    }
                }
            }

            if (isLavaOrFile) return -3;
            if (isOil) return -2;
            if (isWater) return -1;
            return 1;
        }
    }
}
