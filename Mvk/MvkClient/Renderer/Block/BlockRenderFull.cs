using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера блока
    /// </summary>
    public class BlockRenderFull : BlockRenderBase
    {
        /// <summary>
        /// Построение стороны блока
        /// </summary>
        public BlockSide blockUV = new BlockSide();

        protected ChunkRender chunkCheck;

        /// <summary>
        /// Объект блока для проверки
        /// </summary>
        protected BlockBase blockCheck;
        protected int metCheck;

        /// <summary>
        /// Тень на углах
        /// </summary>
        protected bool ambientOcclusion = false;

        protected readonly int[] resultSide = new int[] { -1, -1, -1, -1, -1, -1 };

        protected int i, count, index, id, s1, s2, s3, s4, s5, s6, ntdbv, rs;
        protected int xc, yc, zc, xb, yb, zb, xcn, zcn, pX, pY, pZ;

        protected ChunkStorage storage;
        protected bool isDraw;
        protected int stateLight, stateLightHis;
        protected QuadSide[] rectangularSides;
        protected QuadSide rectangularSide;

        protected AmbientOcclusionLights ambient;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRenderFull() : base()
        {
            ambient = new AmbientOcclusionLights();
            ambient.Init();
        }

        public override void InitChunk(ChunkRender chunkRender)
        {
            chunk = chunkRender;
            ambientOcclusion = Setting.SmoothLighting;
        }

        /// <summary>
        /// Получть cетку сплошного блока
        /// </summary>
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
                damagedBlocksValue = chunk.GetDestroyBlocksValue(posChunkX, posChunkY, posChunkZ);
                RenderMeshBlock();
            }
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// </summary>
        protected int GetBlockSideState(int x, int y, int z, bool isNearbyChunk, bool isUp = false)
        {
            if (isNearbyChunk)
            {
                xcn = x >> 4;
                zcn = z >> 4;
                xc = chunk.Position.x;
                zc = chunk.Position.y;
                // Определяем рабочий чанк соседнего блока
                if (xcn == 0 && zcn == 0)
                {
                    storage = chunk.StorageArrays[yc];
                }
                else
                {
                    xc += xcn;
                    zc += zcn;
                    chunkCheck = chunk.Chunk(xcn, zcn);
                    if (chunkCheck == null || !chunkCheck.IsChunkPresent)
                    {
                        return 0x0F; // Только яркость неба макс
                    }
                    storage = chunkCheck.StorageArrays[yc];
                }
            }
            else
            {
                storage = chunk.StorageArrays[y >> 4];
            }

            xb = x & 15;
            yb = y & 15;
            zb = z & 15;
            i = yb << 8 | zb << 4 | xb;

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

            if (id == 0)
            {
                return storage.lightBlock[i] << 4 | storage.lightSky[i] & 0xF;
            }

            blockCheck = Blocks.blocksInt[id];

            if (blockCheck.AllSideForcibly || (isUp && block.Liquid))
            {
                if (!(!blockCheck.BlocksNotSame && blockCheck.Material == block.Material))
                {
                    EnumMaterial material = blockCheck.Material.EMaterial;
                    return (storage.lightBlock[i] << 4 | storage.lightSky[i] & 0xF)
                        + ((block.Material.EMaterial == EnumMaterial.Water && (material == EnumMaterial.Glass || material == EnumMaterial.Oil)) ? 1024 : 0);
                }
            }
            return -1;
        }

        /// <summary>
        /// Рендер сеток сторон блока которые требуются для прорисовки
        /// </summary>
        private void RenderMeshBlock()
        {
            rectangularSides = block.GetQuads(met, xc, zc, posChunkX, posChunkZ);
            count = rectangularSides.Length;
            for(i = 0; i < count; i++)
            {
                rectangularSide = rectangularSides[i];
                side = rectangularSide.side;
                stateLight = resultSide[side];
                if (stateLight != -1)
                {
                    // Смекшировать яркость, в зависимости от требований самой яркой
                    if (stateLight != stateLightHis
                        && block.UseNeighborBrightness && stateLightHis != -1)
                    {
                        stateLight = stateLight & 0xFF;
                        s1 = stateLight >> 4;
                        s2 = stateLight & 0x0F;
                        s3 = stateLightHis >> 4;
                        s4 = stateLightHis & 0x0F;

                        s5 = s1 > s3 ? s1 : s3;
                        s6 = s2 > s4 ? s2 : s4;

                        stateLight = (s5 << 4 | s6 & 0xF);
                    }

                    lightPole = rectangularSide.lightPole;
                    GenColors((byte)(stateLight & 0xFF));

                    blockUV.colorsr = colorLight.colorr;
                    blockUV.colorsg = colorLight.colorg;
                    blockUV.colorsb = colorLight.colorb;
                    blockUV.lights = colorLight.light;
                    blockUV.posCenterX = posChunkX;
                    blockUV.posCenterY = posChunkY;
                    blockUV.posCenterZ = posChunkZ;
                    blockUV.animationFrame = rectangularSide.animationFrame;
                    blockUV.animationPause = rectangularSide.animationPause;
                    blockUV.vertex = rectangularSide.vertex;
                    blockUV.Building();

                    if (damagedBlocksValue != -1)
                    {
                        int i1 = i + 1;
                        if ((i1 < count && rectangularSides[i1].side != side) || i1 == count)
                        {
                            // Разрушение блока
                            blockUV.colorsr = colorsFF;
                            blockUV.colorsg = colorsFF;
                            blockUV.colorsb = colorsFF;
                            blockUV.BuildingDamaged(3968 + damagedBlocksValue * 2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        protected override void GenColors(byte light)
        {
            color = GetBiomeColor(chunk, posChunkX, posChunkZ);
            if (ambientOcclusion && (block.АmbientOcclusion || block.BiomeColor || block.Liquid))
            {
                GetAmbientOcclusionLights();
                lightPole *= .5f;
                if (block.Liquid)
                {
                    colorLight.InitColorsLights(
                        ambient.GetColorNotAO(0, color, lightPole), ambient.GetColorNotAO(1, color, lightPole),
                        ambient.GetColorNotAO(2, color, lightPole), ambient.GetColorNotAO(3, color, lightPole),
                        ambient.GetLight(0, light), ambient.GetLight(1, light),
                        ambient.GetLight(2, light), ambient.GetLight(3, light)
                    );
                }
                else
                {
                    colorLight.InitColorsLights(
                        ambient.GetColor(0, color, lightPole), ambient.GetColor(1, color, lightPole),
                        ambient.GetColor(2, color, lightPole), ambient.GetColor(3, color, lightPole),
                        ambient.GetLight(0, light), ambient.GetLight(1, light),
                        ambient.GetLight(2, light), ambient.GetLight(3, light));
                }
            }
            else
            {
                color.x -= lightPole; if (color.x < 0) color.x = 0;
                color.y -= lightPole; if (color.y < 0) color.y = 0;
                color.z -= lightPole; if (color.z < 0) color.z = 0;

                colorLight.colorr[0] = colorLight.colorr[1] = colorLight.colorr[2] = colorLight.colorr[3] = (byte)(color.x * 255);
                colorLight.colorg[0] = colorLight.colorg[1] = colorLight.colorg[2] = colorLight.colorg[3] = (byte)(color.y * 255);
                colorLight.colorb[0] = colorLight.colorb[1] = colorLight.colorb[2] = colorLight.colorb[3] = (byte)(color.z * 255);
                colorLight.light[0] = colorLight.light[1] = colorLight.light[2] = colorLight.light[3] = light;
            }
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        /// <returns></returns>
        protected override vec3 GetBiomeColor(ChunkBase chunk, int bx, int bz)
        {
            // подготовка для теста плавности цвета
            if (rectangularSide.IsBiomeColor())
            {
                if (rectangularSide.IsBiomeColorGrass())
                {
                    return BlockColorBiome.Grass(chunk.biome[bx << 4 | bz]);
                }
                if (rectangularSide.IsYourColor())
                {
                    return block.GetColorGuiOrPartFX();
                }
            }
            return colorWhite;
        }

        /// <summary>
        /// Получить все 4 вершины AmbientOcclusion и яркости от блока и неба
        /// </summary>
        private void GetAmbientOcclusionLights()
        {
            switch (side)
            {
                case 0:
                    ambient.aos[0] = GetAmbientOcclusionLight(1, 1, 0);
                    ambient.aos[1] = GetAmbientOcclusionLight(0, 1, 1);
                    ambient.aos[2] = GetAmbientOcclusionLight(-1, 1, 0);
                    ambient.aos[3] = GetAmbientOcclusionLight(0, 1, -1);
                    ambient.aos[4] = GetAmbientOcclusionLight(-1, 1, -1);
                    ambient.aos[5] = GetAmbientOcclusionLight(-1, 1, 1);
                    ambient.aos[6] = GetAmbientOcclusionLight(1, 1, 1);
                    ambient.aos[7] = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 1:
                    ambient.aos[2] = GetAmbientOcclusionLight(1, -1, 0);
                    ambient.aos[1] = GetAmbientOcclusionLight(0, -1, 1);
                    ambient.aos[0] = GetAmbientOcclusionLight(-1, -1, 0);
                    ambient.aos[3] = GetAmbientOcclusionLight(0, -1, -1);
                    ambient.aos[7] = GetAmbientOcclusionLight(-1, -1, -1);
                    ambient.aos[6] = GetAmbientOcclusionLight(-1, -1, 1);
                    ambient.aos[5] = GetAmbientOcclusionLight(1, -1, 1);
                    ambient.aos[4] = GetAmbientOcclusionLight(1, -1, -1);
                    break;
                case 2:
                    ambient.aos[1] = GetAmbientOcclusionLight(1, 1, 0);
                    ambient.aos[0] = GetAmbientOcclusionLight(1, 0, 1);
                    ambient.aos[3] = GetAmbientOcclusionLight(1, -1, 0);
                    ambient.aos[2] = GetAmbientOcclusionLight(1, 0, -1);
                    ambient.aos[4] = GetAmbientOcclusionLight(1, -1, -1);
                    ambient.aos[7] = GetAmbientOcclusionLight(1, -1, 1);
                    ambient.aos[6] = GetAmbientOcclusionLight(1, 1, 1);
                    ambient.aos[5] = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 3:
                    ambient.aos[1] = GetAmbientOcclusionLight(-1, 1, 0);
                    ambient.aos[2] = GetAmbientOcclusionLight(-1, 0, 1);
                    ambient.aos[3] = GetAmbientOcclusionLight(-1, -1, 0);
                    ambient.aos[0] = GetAmbientOcclusionLight(-1, 0, -1);
                    ambient.aos[7] = GetAmbientOcclusionLight(-1, -1, -1);
                    ambient.aos[4] = GetAmbientOcclusionLight(-1, -1, 1);
                    ambient.aos[5] = GetAmbientOcclusionLight(-1, 1, 1);
                    ambient.aos[6] = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 4:
                    ambient.aos[1] = GetAmbientOcclusionLight(0, 1, -1);
                    ambient.aos[0] = GetAmbientOcclusionLight(1, 0, -1);
                    ambient.aos[3] = GetAmbientOcclusionLight(0, -1, -1);
                    ambient.aos[2] = GetAmbientOcclusionLight(-1, 0, -1);
                    ambient.aos[4] = GetAmbientOcclusionLight(-1, -1, -1);
                    ambient.aos[7] = GetAmbientOcclusionLight(1, -1, -1);
                    ambient.aos[6] = GetAmbientOcclusionLight(1, 1, -1);
                    ambient.aos[5] = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 5:
                    ambient.aos[1] = GetAmbientOcclusionLight(0, 1, 1);
                    ambient.aos[2] = GetAmbientOcclusionLight(1, 0, 1);
                    ambient.aos[3] = GetAmbientOcclusionLight(0, -1, 1);
                    ambient.aos[0] = GetAmbientOcclusionLight(-1, 0, 1);
                    ambient.aos[7] = GetAmbientOcclusionLight(-1, -1, 1);
                    ambient.aos[4] = GetAmbientOcclusionLight(1, -1, 1);
                    ambient.aos[5] = GetAmbientOcclusionLight(1, 1, 1);
                    ambient.aos[6] = GetAmbientOcclusionLight(-1, 1, 1);
                    break;
            }
            
            ambient.InitAmbientOcclusionLights();
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// Получть данные (AmbientOcclusion и яркость) одно стороны для вершины
        /// </summary>
        private AmbientOcclusionLight GetAmbientOcclusionLight(int x, int y, int z)
        {
            pX = posChunkX + x;
            pY = posChunkY + y;
            pZ = posChunkZ + z;
            AmbientOcclusionLight aoLight = new AmbientOcclusionLight();

            xcn = (pX >> 4);
            zcn = (pZ >> 4);
            xc = chunk.Position.x + xcn;
            zc = chunk.Position.y + zcn;
            xb = pX & 15;
            zb = pZ & 15;

            // проверка высоты
            if (pY < 0)
            {
                aoLight.lightSky = 0;
                aoLight.color = colorWhite;
                return aoLight;
            }
            // Определяем рабочий чанк соседнего блока
            chunkCheck = (xc == chunk.Position.x && zc == chunk.Position.y) ? chunk : chunk.Chunk(xcn, zcn);
            if (chunkCheck == null || !chunkCheck.IsChunkPresent)
            {
                aoLight.lightSky = 15;
                aoLight.color = color;
                aoLight.aol = 1;
                return aoLight;
            }
            aoLight.color = GetBiomeColor(chunkCheck, xb, zb);
            if (pY >= ChunkBase.COUNT_HEIGHT_BLOCK)
            {
                aoLight.lightSky = 15;
                aoLight.aol = 1;
                return aoLight;
            }
            yc = pY >> 4;
            storage = chunkCheck.StorageArrays[yc];

            yb = pY & 15;
            index = yb << 8 | zb << 4 | xb;

            if (storage.countBlock == 0)
            {
                // Только яркость неба
                aoLight.lightSky = storage.lightSky[index];
                aoLight.lightBlock = storage.lightBlock[index];
                aoLight.aol = 1;
                return aoLight;
            }

            id = storage.data[index];
            metCheck = id >> 12;
            id = id & 0xFFF;
            blockCheck = Blocks.blocksInt[id];
            aoLight.aoc = blockCheck.АmbientOcclusion ? 1 : 0;
            aoLight.aol = blockCheck.IsNotTransparent() || (blockCheck.Liquid && block.Liquid) ? 0 : 1;

            isDraw = id == 0 || blockCheck.AllSideForcibly;
            if (isDraw && (blockCheck.Material == block.Material && !blockCheck.BlocksNotSame)) isDraw = false;

            if (isDraw)
            {
                // Яркость берётся из данных блока
                aoLight.lightBlock = storage.lightBlock[index];
                aoLight.lightSky = storage.lightSky[index];
            }

            if (aoLight.aol == 0)
            {
                aoLight.lightBlock = 0;
                aoLight.lightSky = 0;
            }
            return aoLight;
        }

        
        /// <summary>
        /// Структура для 4-ёх вершин цвета и освещения
        /// </summary>
        protected struct AmbientOcclusionLights
        {
            private int[] lightBlock;
            private int[] lightSky;
            private vec3[] colors;
            private int[] aols;
            private int[] aocs;
            public AmbientOcclusionLight[] aos;

            private int lb, ls, count;

            public void Init()
            {
                lightBlock = new int[4];
                lightSky = new int[4];
                colors = new vec3[4];
                aols = new int[4];
                aocs = new int[4];
                aos = new AmbientOcclusionLight[8];
            }

            public void InitAmbientOcclusionLights()
            {
                //a, b, c, d, e, f, g, h
                //0, 1, 2, 3, 4, 5, 6, 7

                lightBlock[0] = aos[2].lightBlock + aos[3].lightBlock + aos[4].lightBlock;
                lightBlock[1] = aos[1].lightBlock + aos[2].lightBlock + aos[5].lightBlock;
                lightBlock[2] = aos[0].lightBlock + aos[1].lightBlock + aos[6].lightBlock;
                lightBlock[3] = aos[0].lightBlock + aos[3].lightBlock + aos[7].lightBlock;

                lightSky[0] = aos[2].lightSky + aos[3].lightSky + aos[4].lightSky;
                lightSky[1] = aos[1].lightSky + aos[2].lightSky + aos[5].lightSky;
                lightSky[2] = aos[0].lightSky + aos[1].lightSky + aos[6].lightSky;
                lightSky[3] = aos[0].lightSky + aos[3].lightSky + aos[7].lightSky;

                colors[0] = aos[2].color + aos[3].color + aos[4].color;
                colors[1] = aos[1].color + aos[2].color + aos[5].color;
                colors[2] = aos[0].color + aos[1].color + aos[6].color;
                colors[3] = aos[0].color + aos[3].color + aos[7].color;

                aols[0] = aos[2].aol + aos[3].aol + aos[4].aol;
                aols[1] = aos[1].aol + aos[2].aol + aos[5].aol;
                aols[2] = aos[0].aol + aos[1].aol + aos[6].aol;
                aols[3] = aos[0].aol + aos[3].aol + aos[7].aol;

                aocs[0] = aos[2].aoc + aos[3].aoc + aos[4].aoc;
                aocs[1] = aos[1].aoc + aos[2].aoc + aos[5].aoc;
                aocs[2] = aos[0].aoc + aos[1].aoc + aos[6].aoc;
                aocs[3] = aos[0].aoc + aos[3].aoc + aos[7].aoc;
            }

            public byte GetLight(int index, byte light)
            {
                lb = (light & 0xF0) >> 4;
                ls = light & 0xF;
                count = 1 + aols[index];
                lb = (lightBlock[index] + lb) / count;
                ls = (lightSky[index] + ls) / count;
                return (byte)(lb << 4 | ls);
            }

            public vec3 GetColor(int index, vec3 color, float lightPole)
            {
                vec3 c = (colors[index] + color) / 4f * (1f - aocs[index] * .2f);
                c.x -= lightPole; if (c.x < 0) c.x = 0;
                c.y -= lightPole; if (c.y < 0) c.y = 0;
                c.z -= lightPole; if (c.z < 0) c.z = 0;
                return c;
            }
            public vec3 GetColorNotAO(int index, vec3 color, float lightPole)
            {
                vec3 c = (colors[index] + color) / 4f;
                c.x -= lightPole; if (c.x < 0) c.x = 0;
                c.y -= lightPole; if (c.y < 0) c.y = 0;
                c.z -= lightPole; if (c.z < 0) c.z = 0;
                return c;
            }
        }
        /// <summary>
        /// Структура данных (AmbientOcclusion и яркости от блока и неба) одно стороны для вершины
        /// </summary>
        protected struct AmbientOcclusionLight
        {
            public byte lightBlock;
            public byte lightSky;
            public vec3 color;
            public int aol;
            public int aoc;
        }
    }
}
