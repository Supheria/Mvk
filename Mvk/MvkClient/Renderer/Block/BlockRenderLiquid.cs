using MvkServer.Glm;
using MvkServer.World.Block;
using MvkServer.World.Block.List;
using MvkServer.World.Chunk;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера жидкого блока
    /// </summary>
    public class BlockRenderLiquid : BlockRenderFull
    {
        /// <summary>
        /// Структура активной стороны жидкого блока
        /// </summary>
        private SideLiquid sideLiquid;
        /// <summary>
        /// Материал тикущего блока
        /// </summary>
        private EnumMaterial material; 

        private int y;
        private float u1, u2, u3, u4, v1, v2, v3, v4;
        private float h00, h10, h01, h11;
        private int up00, up10, up20, up01, up11, up21, up02, up12, up22;
        private int l00, l10, l20, l01, l11, l21, l02, l12, l22;
        private byte h1, h2, h3, h4;

        private float angleFlow;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRenderLiquid() : base() { }

        /// <summary>
        /// Получть Сетку блока с возможностью двух сторон
        /// </summary>
        /// <returns>сетка</returns>
        public override void RenderMesh()
        {
            xc = chunk.Position.x;
            zc = chunk.Position.y;

            if (posChunkY + 1 > ChunkBase.COUNT_HEIGHT_BLOCK) rs = resultSide[0] = 0x0F;
            else rs = resultSide[0] = GetBlockSideState(posChunkX, posChunkY + 1, posChunkZ, false, true);
            if (posChunkY - 1 < 0) rs += resultSide[1] = 0x0F;
            else rs += resultSide[1] = GetBlockSideState(posChunkX, posChunkY - 1, posChunkZ, false);

            yc = posChunkY >> 4;
            rs += resultSide[2] = GetBlockSideState(posChunkX + 1, posChunkY, posChunkZ, true);
            rs += resultSide[3] = GetBlockSideState(posChunkX - 1, posChunkY, posChunkZ, true);
            rs += resultSide[4] = GetBlockSideState(posChunkX, posChunkY, posChunkZ - 1, true);
            rs += resultSide[5] = GetBlockSideState(posChunkX, posChunkY, posChunkZ + 1, true);

            stateLightHis = -1;
            if (rs != -6)
            {
                if (block.UseNeighborBrightness) stateLightHis = blockState.lightBlock << 4 | blockState.lightSky & 0xF;
                RenderMeshBlockLiquid();
            }
        }


        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        /// <returns></returns>
        protected override vec3 GetBiomeColor(ChunkBase chunk, int bx, int bz) 
            => sideLiquid.IsBiomeColorWater() ? BlockColorBiome.Water(chunk.biome[bx << 4 | bz]) : colorWhite;

        /// <summary>
        /// Рендер сетки жидкого блока
        /// </summary>
        private void RenderMeshBlockLiquid()
        {
            // TODO::2023-06-16 Моря сильно грузят рендер можно попробовать оптимизировать!
            material = blockState.GetBlock().Material;
            y = posChunkY + 1;
            // Получаем данные вверхнего уровня блока, в облости 3*3
            up00 = GetLiquidBlock(posChunkX - 1, y, posChunkZ - 1, true);
            up10 = GetLiquidBlock(posChunkX, y, posChunkZ - 1, true);
            up20 = GetLiquidBlock(posChunkX + 1, y, posChunkZ - 1, true);
            up01 = GetLiquidBlock(posChunkX - 1, y, posChunkZ, true);
            up11 = GetLiquidBlock(posChunkX, y, posChunkZ, false);
            up21 = GetLiquidBlock(posChunkX + 1, y, posChunkZ, true);
            up02 = GetLiquidBlock(posChunkX - 1, y, posChunkZ + 1, true);
            up12 = GetLiquidBlock(posChunkX, y, posChunkZ + 1, true);
            up22 = GetLiquidBlock(posChunkX + 1, y, posChunkZ + 1, true);

            // Получаем данные этого уровня блока, в облости 3*3
            l00 = GetLiquidBlock(posChunkX - 1, posChunkY, posChunkZ - 1, true);
            l10 = GetLiquidBlock(posChunkX, posChunkY, posChunkZ - 1, true);
            l20 = GetLiquidBlock(posChunkX + 1, posChunkY, posChunkZ - 1, true);
            l01 = GetLiquidBlock(posChunkX - 1, posChunkY, posChunkZ, true);
            l11 = GetLiquidBlock(posChunkX, posChunkY, posChunkZ, false);
            l21 = GetLiquidBlock(posChunkX + 1, posChunkY, posChunkZ, true);
            l02 = GetLiquidBlock(posChunkX - 1, posChunkY, posChunkZ + 1, true);
            l12 = GetLiquidBlock(posChunkX, posChunkY, posChunkZ + 1, true);
            l22 = GetLiquidBlock(posChunkX + 1, posChunkY, posChunkZ + 1, true);

            // Получить высоту вершины 0..1
            h00 = HeightVertexLiquid(up00, up10, up01, up11, new int[] { l00, l10, l01, l11 });
            h10 = HeightVertexLiquid(up10, up20, up11, up21, new int[] { l10, l20, l11, l21 });
            h01 = HeightVertexLiquid(up01, up11, up02, up12, new int[] { l01, l11, l02, l12 });
            h11 = HeightVertexLiquid(up11, up21, up12, up22, new int[] { l11, l21, l12, l22 });

            blockUV.bufferCache.Clear();

            if (resultSide[0] != -1) // Up
            {
                angleFlow = BlockAbLiquid.GetAngleFlow(l11, l01, l10, l12, l21);
                sideLiquid = block.GetSideLiquid(0);

                if (angleFlow < -999)
                {
                    // Без вращения
                    blockUV.animationFrame = sideLiquid.animationFrame;
                    blockUV.animationPause = sideLiquid.animationPause;
                    u1 = sideLiquid.u;
                    v1 = sideLiquid.v;
                    u2 = sideLiquid.u;
                    v2 = sideLiquid.v + .015625f;
                    u3 = sideLiquid.u + .015625f;
                    v3 = sideLiquid.v + .015625f;
                    u4 = sideLiquid.u + .015625f;
                    v4 = sideLiquid.v;
                }
                else
                {
                    // Вращаем
                    SideLiquid sideLiquidCache = block.GetSideLiquid(2);
                    blockUV.animationFrame = sideLiquidCache.animationFrame;
                    blockUV.animationPause = sideLiquidCache.animationPause;

                    float fs = glm.sin(angleFlow) * .25f;
                    float fc = glm.cos(angleFlow) * .25f;
                    u1 = sideLiquidCache.u + (-fc - fs) * .03125f;
                    v1 = sideLiquidCache.v + .015625f + (-fc + fs) * .03125f;
                    u2 = sideLiquidCache.u + (-fc + fs) * .03125f;
                    v2 = sideLiquidCache.v + .015625f + (fc + fs) * .03125f;
                    u3 = sideLiquidCache.u + (fc + fs) * .03125f;
                    v3 = sideLiquidCache.v + .015625f + (fc - fs) * .03125f;
                    u4 = sideLiquidCache.u + (fc - fs) * .03125f;
                    v4 = sideLiquidCache.v + .015625f + (-fc - fs) * .03125f;
                }
                lightPole = sideLiquid.lightPole;
                GenColors(GetLightMix(resultSide[0], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                if (h00 == .9f) if (posChunkZ % 2 == 0) if (posChunkX % 2 == 0) h1 = 1; else h1 = 2; else h1 = 2; else h1 = 0;
                if (h01 == .9f) if (posChunkZ % 2 != 0) if (posChunkX % 2 == 0) h2 = 1; else h2 = 2; else h2 = 2; else h2 = 0;
                if (h11 == .9f) if (posChunkZ % 2 != 0) if (posChunkX % 2 != 0) h3 = 1; else h3 = 2; else h3 = 2; else h3 = 0;
                if (h10 == .9f) if (posChunkZ % 2 == 0) if (posChunkX % 2 != 0) h4 = 1; else h4 = 2; else h4 = 2; else h4 = 0;

                // Вверх
                blockUV.BufferSideOutsideCache(
                    posChunkX, posChunkY + h00, posChunkZ,
                    posChunkX, posChunkY + h01, posChunkZ + 1,
                    posChunkX + 1, posChunkY + h11, posChunkZ + 1,
                    posChunkX + 1, posChunkY + h10, posChunkZ,
                    u1, v1, u2, v2, u3, v3, u4, v4, h1, h2, h3, h4
                );

                // Условие, если тикучая вода или стоячая но над стоячей вверхний блок 
                // не может быть стоячей водной и спложным блоком
                if (l11 != 15 || (l11 == 15 && CheckUp(new int[] { up00, up10, up20, up01, up11, up21, up02, up12, up22 })))
                {
                    // из нутри
                    blockUV.BufferSideInside(
                        posChunkX, posChunkY + h00, posChunkZ,
                        posChunkX, posChunkY + h01, posChunkZ + 1,
                        posChunkX + 1, posChunkY + h11, posChunkZ + 1,
                        posChunkX + 1, posChunkY + h10, posChunkZ,
                        u1, v1, u2, v2, u3, v3, u4, v4, h1, h2, h3, h4
                    );
                }
            }
            if (resultSide[1] != -1) // Down
            {
                sideLiquid = block.GetSideLiquid(1);
                lightPole = sideLiquid.lightPole;
                GenColors(GetLightMix(resultSide[1], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;
                blockUV.animationFrame = sideLiquid.animationFrame;
                blockUV.animationPause = sideLiquid.animationPause;
                
                u1 = sideLiquid.u;
                v1 = sideLiquid.v;
                u2 = sideLiquid.u + .015625f;
                v2 = sideLiquid.v + .015625f;

                blockUV.BufferSideOutsideCache(
                    posChunkX + 1, posChunkY, posChunkZ,
                    posChunkX + 1, posChunkY, posChunkZ + 1,
                    posChunkX, posChunkY, posChunkZ + 1,
                    posChunkX, posChunkY, posChunkZ,
                    u2, v1, u2, v2, u1, v2, u1, v1
                );
            }

            if (resultSide[2] != -1) // East
            {
                sideLiquid = block.GetSideLiquid(2);
                lightPole = sideLiquid.lightPole;
                GenColors(GetLightMix(resultSide[2], stateLightHis));
                blockUV.animationFrame = sideLiquid.animationFrame;
                blockUV.animationPause = sideLiquid.animationPause;
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = sideLiquid.u;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = sideLiquid.v + .015625f;
                v2 = v1 - .015625f * h10;
                v3 = v1 - .015625f * h11;

                blockUV.BufferSideTwo(
                    posChunkX + 1, posChunkY, posChunkZ,
                    posChunkX + 1, posChunkY + h10, posChunkZ,
                    posChunkX + 1, posChunkY + h11, posChunkZ + 1,
                    posChunkX + 1, posChunkY, posChunkZ + 1,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[2] > 1000
                );
            }

            if (resultSide[3] != -1) // West
            {
                sideLiquid = block.GetSideLiquid(3);
                lightPole = sideLiquid.lightPole;
                GenColors(GetLightMix(resultSide[3], stateLightHis));
                blockUV.animationFrame = sideLiquid.animationFrame;
                blockUV.animationPause = sideLiquid.animationPause;
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = sideLiquid.u;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = sideLiquid.v + .015625f;
                v3 = v1 - .015625f * h00;
                v2 = v1 - .015625f * h01;
                
                blockUV.BufferSideTwo(
                    posChunkX, posChunkY, posChunkZ + 1,
                    posChunkX, posChunkY + h01, posChunkZ + 1,
                    posChunkX, posChunkY + h00, posChunkZ,
                    posChunkX, posChunkY, posChunkZ,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[3] > 1000
                );
            }

            if (resultSide[4] != -1) // North
            {
                sideLiquid = block.GetSideLiquid(4);
                lightPole = sideLiquid.lightPole;
                GenColors(GetLightMix(resultSide[4], stateLightHis));
                blockUV.animationFrame = sideLiquid.animationFrame;
                blockUV.animationPause = sideLiquid.animationPause;
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = sideLiquid.u;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = sideLiquid.v + .015625f;
                v2 = v1 - .015625f * h00;
                v3 = v1 - .015625f * h10;

                blockUV.BufferSideTwo(
                    posChunkX, posChunkY, posChunkZ,
                    posChunkX, posChunkY + h00, posChunkZ,
                    posChunkX + 1, posChunkY + h10, posChunkZ,
                    posChunkX + 1, posChunkY, posChunkZ,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[4] > 1000
                );
            }

            if (resultSide[5] != -1) // South
            {
                sideLiquid = block.GetSideLiquid(5);
                lightPole = sideLiquid.lightPole;
                GenColors(GetLightMix(resultSide[5], stateLightHis));
                blockUV.animationFrame = sideLiquid.animationFrame;
                blockUV.animationPause = sideLiquid.animationPause;
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = sideLiquid.u;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = sideLiquid.v + .015625f;
                v3 = v1 - .015625f * h01;
                v2 = v1 - .015625f * h11;

                blockUV.BufferSideTwo(
                    posChunkX + 1, posChunkY, posChunkZ + 1,
                    posChunkX + 1, posChunkY + h11, posChunkZ + 1,
                    posChunkX, posChunkY + h01, posChunkZ + 1,
                    posChunkX, posChunkY, posChunkZ + 1,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[5] > 1000
                );
            }

            blockUV.AddBufferCache();
        }
        /// <summary>
        /// Смекшировать яркость, в зависимости от требований самой яркой, может быть -1
        /// </summary>
        //private int GetLightMixNull(int stateLight, int stateLightHis)
        //{
        //    if (stateLight != -1 && stateLight != stateLightHis)
        //    {
        //        if (block.UseNeighborBrightness)
        //        {
        //            if (stateLightHis == -1) return stateLight;

        //            stateLight = stateLight & 0xFF;
        //            int s1 = stateLight >> 4;
        //            int s2 = stateLight & 0x0F;
        //            int s3 = stateLightHis >> 4;
        //            int s4 = stateLightHis & 0x0F;

        //            int s5 = s1 > s3 ? s1 : s3;
        //            int s6 = s2 > s4 ? s2 : s4;

        //            return (s5 << 4 | s6 & 0xF);
        //        }
        //    }
        //    return stateLight;
        //}
        /// <summary>
        /// Смекшировать яркость, в зависимости от требований самой яркой
        /// </summary>
        private byte GetLightMix(int stateLight, int stateLightHis)
        {
            if (stateLight != stateLightHis)
            {
                stateLight = stateLight & 0xFF;
                if (block.UseNeighborBrightness)
                {
                    int s1 = stateLight >> 4;
                    int s2 = stateLight & 0x0F;
                    int s3 = stateLightHis >> 4;
                    int s4 = stateLightHis & 0x0F;

                    int s5 = s1 > s3 ? s1 : s3;
                    int s6 = s2 > s4 ? s2 : s4;

                    return (byte)(s5 << 4 | s6 & 0xF);
                }
            }
            return (byte)stateLight;
        }

        /// <summary>
        /// Проверка вверхнего блока на не целый блок
        /// </summary>
        private bool CheckUp(int[] vs)
        {
            for (int i = 0; i < vs.Length; i++)
            {
                if (vs[i] > -2) return true;
            }
            return false;
        }

        /// <summary>
        /// Получить высоту вершины жидкости 0..1
        /// </summary>
        private float HeightVertexLiquid(int up00, int up10, int up01, int up11, int[] vs)
        {
            if (up00 > 0 || up10 > 0 || up01 > 0 || up11 > 0)
            {
                return 1f;
            }
            float count = 0;
            float value = 0;
            int metLevel;
            for (int i = 0; i < 4; i++)
            {
                metLevel = vs[i];
                if (metLevel > 0)
                {
                    if (metLevel == 15)
                    {
                        // value чем больше тут цифра, тем ниже уровень 
                        // (.5 почти на уровне, 2.0 около двух пикселей от верха)
                        value++; 
                        count += 10;
                    }
                    else
                    {
                        value += (15 - metLevel) / 16f;
                        count++;
                    }
                }
                else if (metLevel == -1)
                {
                    value++;
                    count++;
                }
            }
            return 1f - value / count;
        }

        /// <summary>
        /// Получить значение блока жидкости мет данных.
        /// Где 15 это стоячаяя или целый растекаемы, 1 минимум в воде, 3 в нефте и лаве минимум,
        /// -1 блок не жидкости, -2 сплошной блок не жидкости
        /// </summary>
        private int GetLiquidBlock(int x, int y, int z, bool isNearbyChunk)
        {
            if (y >= ChunkBase.COUNT_HEIGHT_BLOCK) return -1;

            if (isNearbyChunk)
            {
                xcn = x >> 4;
                zcn = z >> 4;
                xc = chunk.Position.x;
                zc = chunk.Position.y;
                // Определяем рабочий чанк соседнего блока
                if (xcn == 0 && zcn == 0)
                {
                    storage = chunk.StorageArrays[y >> 4];
                }
                else
                {
                    xc += xcn;
                    zc += zcn;
                    chunkCheck = chunk.Chunk(xcn, zcn);
                    if (chunkCheck == null || !chunkCheck.IsChunkPresent) return -1;
                    storage = chunkCheck.StorageArrays[y >> 4];
                }
            }
            else
            {
                storage = chunk.StorageArrays[y >> 4];
            }

            xb = x & 15;
            yb = y & 15;
            zb = z & 15;
            int i = yb << 8 | zb << 4 | xb;

            if (storage.countBlock > 0)
            {
                id = storage.data[i];
                metCheck = id >> 12;
                id = id & 0xFFF;
            }
            else
            {
                metCheck = 0;
                id = 0;
            }

            blockCheck = Blocks.blocksInt[id];
            if ((material != EnumMaterial.Lava && (blockCheck.Material == EnumMaterial.Oil || blockCheck.Material == EnumMaterial.Water))
                || (material == EnumMaterial.Lava && blockCheck.Material == EnumMaterial.Lava))
            {
                if (id == 13 || id == 15 || id == 17)
                {
                    // стоячая жидкость
                    return 15;
                }
                return metCheck;
            }
            else if (!blockCheck.IsAir && blockCheck.FullBlock) return -2;
            return -1;
        }
    }
}
