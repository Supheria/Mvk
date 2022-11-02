using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World.Block;
using System;
using System.Collections.Generic;

namespace MvkServer.World
{
    /// <summary>
    /// Объект взрыва
    /// Максимальный взрыв до 16 блоков в радиусе
    /// </summary>
    public class Explosion
    {
        /// <summary>
        /// Словарь игроков с перемещением попавшие в зрыве
        /// </summary>
        public Dictionary<EntityPlayer, vec3> MotionPlayer { get; private set; } = new Dictionary<EntityPlayer, vec3>();

        private WorldBase world;
        /// <summary>
        /// Позиция взрыва
        /// </summary>
        private vec3 pos;
        /// <summary>
        /// Сила, для мощности ломания блоков
        /// </summary>
        private float strength;
        /// <summary>
        /// Дистанция радиус разлёта
        /// </summary>
        private float distance;
        /// <summary>
        /// Максимальный блок для расчёта, радиус
        /// </summary>
        private int maxDistance;
        /// <summary>
        /// Диаметр
        /// </summary>
        private int diameter;
        /// <summary>
        /// Список блоков для уничтожения
        /// </summary>
        private ArrayMvk<BlockPos> listBlockDestruction = new ArrayMvk<BlockPos>(32768);
        /// <summary>
        /// Массив кэша устойчивости блоков
        /// </summary>
        private readonly float[,,] arrayResistanceCache = new float[33, 33, 33];
        /// <summary>
        /// Звуковые семплы взрыва
        /// </summary>
        private readonly AssetsSample[] samples = new AssetsSample[] { AssetsSample.Explode1, AssetsSample.Explode2, AssetsSample.Explode3, AssetsSample.Explode4 };

        public Explosion(WorldBase world)
        {
            this.world = world;
            /**
             * DoExplosionOld
             * 
             * strength = 3 
             * Explosion 0,0728 + 0,4930 = 0,5659 count:222, max:4, c0:386, c1:3965, c2:4, c3:230
             * 
             * strength = 4
             * Explosion 0,2896 + 1,2319 = 1,5218 count:520, max:6, c0:866, c1:11570, c2:1, c3:544
             * 
             * strength = 5
             * Explosion 0,3636 + 4,6666 = 5,0299 count:1029, max:7, c0:1178, c1:19915, c2:5, c3:1068
             * 
             * strength = 7
             * Explosion 1,6119 + 14,6743 = 16,2862 count:2521, max:10, c0:2402, c1:54947, c2:32, c3:2621
             * Explosion 0,8188 + 11,9650 = 12,7841 count:2680, max:10, c0:2402, c1:56411, c2:34, c3:2768
             * Explosion 1,0811 + 10,4236 = 11,5044 count:2511, max:10, c0:2402, c1:55122, c2:28, c3:2634
             * 
             * strength = 10
             * Explosion 4,0068 + 46,3440 = 50,3511 count:6875, max:13, c0:4058, c1:124496, c2:402, c3:6684
             * Explosion 4,7573 + 48,4748 = 53,2327 count:7103, max:13, c0:4058, c1:130307, c2:398, c3:6966
             * Explosion 6,4191 + 40,0964 = 46,5164 count:6925, max:13, c0:4058, c1:123576, c2:466, c3:6658
             * 
             * strength = 16
             * Explosion 8,2885 + 96,2179 = 104,5067 count:27242, max:21, c0:10586, c1:492018, c2:2247, c3:25494
             * Explosion 31,4751 + 98,7061 = 130,1818 count:26379, max:21, c0:10586, c1:487369, c2:1876, c3:24983
             * Explosion 6,9448 + 79,5431 = 86,4882 count:27440, max:21, c0:10586, c1:494131, c2:2196, c3:25700
             * Explosion 21,6902 + 104,5568 = 126,2475 count:26787, max:21, c0:10586, c1:484879, c2:2149, c3:25096
             * 
             */
            /**
             * DoExplosion
             * 
             * distance = 5 strength = 5
             * Explosion 0,1661 + 3,4441 = 3,6105 count:472, max:7, c0:1178, c1:6257, c2:5449, c3:808 c4:472
             * Explosion 0,1724 + 3,8094 = 3,9815 count:470, max:7, c0:1178, c1:6201, c2:5425, c3:776 c4:470
             * Explosion 0,1622 + 3,2632 = 3,4253 count:466, max:7, c0:1178, c1:6200, c2:5413, c3:787 c4:466
             * 
             * distance = 7 strength = 3 
             * Explosion 0,4626 + 9,8222 = 10,2845 count:921, max:10, c0:2402, c1:16764, c2:14900, c3:1864 c4:918
             * Explosion 0,3989 + 11,6652 = 12,0640 count:953, max:10, c0:2402, c1:16839, c2:14959, c3:1880 c4:951
             * Explosion 0,4282 + 15,4179 = 15,8461 count:1073, max:10, c0:2402, c1:17128, c2:15121, c3:2007 c4:1073
             * Explosion 0,4213 + 13,7977 = 14,2191 count:960, max:10, c0:2402, c1:16991, c2:15067, c3:1924 c4:958
             * Explosion 0,3977 + 10,9858 = 11,3832 count:875, max:10, c0:2402, c1:16788, c2:14917, c3:1871 c4:875
             * 
             * distance = 7 strength = 7
             * Explosion 0,7963 + 17,9551 = 18,7514 count:1029, max:10, c0:2402, c1:16990, c2:15046, c3:1944 c4:1029
             * Explosion 1,0834 + 25,4387 = 26,5223 count:1090, max:10, c0:2402, c1:17169, c2:15158, c3:2011 c4:1090
             * 
             * distance = 10 strength = 10 
             * Explosion 1,0276 + 47,5838 = 48,6117 count:2651, max:13, c0:4058, c1:39937, c2:34649, c3:5288 c4:2651
             * Explosion 1,1937 + 48,5696 = 49,7636 count:2458, max:13, c0:4058, c1:39701, c2:34502, c3:5199 c4:2452
             * Explosion 1,3229 + 52,1414 = 53,4646 count:2669, max:13, c0:4058, c1:40116, c2:34718, c3:5398 c4:2669
             * Explosion 1,5687 + 47,8037 = 49,3724 count:2381, max:13, c0:4058, c1:39324, c2:34228, c3:5096 c4:2378
             * 
             * 
             */
        }

        /// <summary>
        /// Задать параметры взрыва
        /// </summary>
        /// <param name="pos">Позиция где активируется взрыв</param>
        /// <param name="strength">Сила взрыва</param>
        /// <param name="distance">Дистанция взрыва</param>
        public void SetExplosion(vec3 pos, float strength, float distance)
        {
            MotionPlayer.Clear();
            this.pos = pos;
            this.strength = strength;
            this.distance = distance;
            maxDistance = Mth.Ceiling(distance * 1.1f);
            if (maxDistance > 16)
            {
                maxDistance = 16;
                this.distance = 14.5f;
            }
            diameter = maxDistance * 2 + 1;
        }
        
        public void DoExplosion()
        {
            listBlockDestruction.Clear();
            int ox = Mth.Floor(pos.x);
            int oy = Mth.Floor(pos.y);
            int oz = Mth.Floor(pos.z);
            int x, y, z, x0, y0, z0;
            x0 = y0 = z0 = 0;
            float dis, res, fx, fy, fz, dis0, resistance;
            BlockPos blockPos;
            BlockState blockState;

            vec3 vec = new vec3();
            bool repeat;

            // Массив присутствующих блоков в кэше, для оптимизации повторного изъятия данных блока
            bool[,,] bePresentBlock = new bool[diameter, diameter, diameter];

            try
            {
                for (x = -maxDistance; x <= maxDistance; ++x)
                {
                    for (y = -maxDistance; y <= maxDistance; ++y)
                    {
                        for (z = -maxDistance; z <= maxDistance; ++z)
                        {
                            if (x == -maxDistance || x == maxDistance || y == -maxDistance || y == maxDistance || z == -maxDistance || z == maxDistance)
                            {
                                vec.x = x;
                                vec.y = y;
                                vec.z = z;
                                vec = vec.normalize();
                                // дистанция x * 0.9 ... x * 1.1
                                dis = distance * (.9f + world.Rnd.NextFloat() * .2f);
                                // Сила x * 0.7 ... x * 1.3
                                res = strength * (.7f + world.Rnd.NextFloat() * .6f);

                                fx = pos.x;
                                fy = pos.y;
                                fz = pos.z;

                                dis0 = 0;
                                resistance = 0;

                                while (dis0 < dis && res > 0)
                                {
                                    repeat = false;
                                    blockPos = new BlockPos(fx, fy, fz);
                                    x0 = blockPos.X - ox + maxDistance;
                                    y0 = blockPos.Y - oy + maxDistance;
                                    z0 = blockPos.Z - oz + maxDistance;
                                    if (!bePresentBlock[x0, y0, z0])
                                    {
                                        bePresentBlock[x0, y0, z0] = true;
                                        blockState = world.GetBlockState(blockPos);
                                        if (blockState.GetEBlock() != EnumBlock.Air)
                                        {
                                            arrayResistanceCache[x0, y0, z0] = resistance = blockState.GetBlock().Resistance;
                                        }
                                        else
                                        {
                                            arrayResistanceCache[x0, y0, z0] = resistance = -100f;
                                        }
                                    }
                                    else
                                    {
                                        repeat = true;
                                        resistance = arrayResistanceCache[x0, y0, z0];
                                    }

                                    if (resistance >= 0)
                                    {
                                        res -= (resistance + .3f) * .3f;

                                        if (!repeat && res > 0)
                                        {
                                            listBlockDestruction.Add(blockPos);
                                        }
                                    }
                                    fx += vec.x;
                                    fy += vec.y;
                                    fz += vec.z;
                                    dis0++;
                                }
                            }
                        }
                    }
                }

                // Уничтажаем блоки
                if (listBlockDestruction.count > 0)
                {
                    for (int i = 0; i < listBlockDestruction.count; i++)
                    {
                        blockPos = listBlockDestruction[i];
                        blockState = world.GetBlockState(blockPos);
                        BlockBase block = blockState.GetBlock();
                        world.SpawnParticle(EnumParticle.Smoke, 1, blockPos.ToVec3() + .5f, new vec3(1), 0, 40);
                        if (!block.IsAir)
                        {
                            if (block.CanDropFromExplosion)
                            {
                                block.DropBlockAsItemWithChance(world, blockPos, blockState, 1f / strength, 0);
                            }
                            world.SetBlockToAir(blockPos);
                        }
                    }
                }

                // Собираем список сущностей для взаимодействия со взрывом
                MapListEntity listEntity = world.GetEntitiesWithinAABB(Chunk.ChunkBase.EnumEntityClassAABB.All,
                    new AxisAlignedBB(pos - distance, pos + distance), -1);

                EntityBase entity;
                for (int i = 0; i < listEntity.Count; i++)
                {
                    entity = listEntity.GetAt(i);
                    vec3 epos = entity.Position;
                    epos.y += entity.GetEyeHeight();

                    int ex = Mth.Floor(epos.x);
                    int ey = Mth.Floor(epos.y);
                    int ez = Mth.Floor(epos.z);

                    x0 = ex - ox + maxDistance;
                    y0 = ey - oy + maxDistance;
                    z0 = ez - oz + maxDistance;

                    // Данный блок подвергся урону
                    if (x0 >= 0 && y0 >= 0 && z0 >= 0 && x0 < diameter && y0 < diameter && z0 < diameter
                        && bePresentBlock[x0, y0, z0])
                    {
                        // Определяем силу урона

                        // Коэфф до взрыва, 0..1
                        float kof = 1f - glm.distance(pos, epos) / distance;
                        if (kof > 0)
                        {
                            entity.AttackEntityFrom(EnumDamageSource.ExplosionSource, strength * kof * 8f);

                            // Определяем смещение от взрыва 
                            vec3 motion = (epos - pos).normalize() * kof * 4f;
                            entity.SetMotion(entity.Motion + motion);

                            if (entity is EntityPlayer entityPlayer)
                            {
                                MotionPlayer.Add(entityPlayer, motion);
                            }
                        }
                    }
                }

                // Звуковой эффект
                world.PlaySound(samples[world.Rnd.Next(4)], pos, 4f, (1f + (world.Rnd.NextFloat() - world.Rnd.NextFloat()) * .2f) * .7f);
            }
            catch (Exception ex)
            {
                Logger.Crach(ex, "diameter:{3} x0:{0} y0:{1} z0:{2}", x0, y0, z0, diameter);
                throw;
            }
        }
    }
}
