using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Block.List;
using MvkServer.World.Chunk;
using System.Collections.Generic;


namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера блока
    /// </summary>
    public class BlockRender
    {
        /// <summary>
        /// Пометка, разрушается ли блок и его стадия
        /// -1 не разрушается, 0-9 разрушается
        /// </summary>
        public int DamagedBlocksValue { get; set; } = -1;

        /// <summary>
        /// Построение стороны блока
        /// </summary>
        public BlockSide blockUV = new BlockSide();

        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        protected readonly ChunkRender chunk;
        private ChunkRender chunkCheck;
        /// <summary>
        /// Объект блока кэш
        /// </summary>
        public BlockBase block;
        public BlockState blockState;
        public int met;
        
        /// <summary>
        /// Объект блока для проверки
        /// </summary>
        private BlockBase blockCheck;
        private int metCheck;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkX;
        /// <summary>
        /// Позиция блока в чанке 0..255
        /// </summary>
        public int posChunkY;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkY0;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkZ;
        /// <summary>
        /// кэш коробка
        /// </summary>
        private Box cBox;
        /// <summary>
        /// кэш сторона блока
        /// </summary>
        private Face cFace;
        /// <summary>
        /// кэш Направление
        /// </summary>
        private int cSideInt;

        /// <summary>
        /// Тень на углах
        /// </summary>
        private readonly bool ambientOcclusion = false;

        private readonly int[] resultSide = new int[] { -1, -1, -1, -1, -1, -1 };

        private int xcn, zcn;
        private int xc, yc, zc;
        private int xb, yb, zb;
        private int index;
        private float u1, v2;

        private int id;
        private ChunkStorage storage;
        private bool isDraw;
        private int stateLight;
        private int stateLightHis;
        private EnumMaterial material;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRender(ChunkRender chunkRender)
        {
            ambientOcclusion = Setting.SmoothLighting;
            chunk = chunkRender;
        }

        /// <summary>
        /// Получть Сетку блока с возможностью двух сторон
        /// </summary>
        /// <returns>сетка</returns>
        public void RenderMesh()
        {
            xc = chunk.Position.x;
            zc = chunk.Position.y;

            int rs;
            if (posChunkY + 1 >= ChunkBase.COUNT_HEIGHT_BLOCK) rs = resultSide[0] = 0x0F;
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
                if (block.Liquid)
                {
                    RenderMeshBlockLiquid();
                }
                else
                {
                    DamagedBlocksValue = chunk.GetDestroyBlocksValue(posChunkX, posChunkY, posChunkZ);
                    RenderMeshBlock();
                }
            }
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// </summary>
        private int GetBlockSideState(int x, int y, int z, bool isNearbyChunk, bool isUp = false)
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

            isDraw = id == 0 || blockCheck.AllSideForcibly || (isUp && block.Liquid);
            if (isDraw && (blockCheck.Material == block.Material && !blockCheck.BlocksNotSame(met))) isDraw = false;
            
            int k = (block.Material == EnumMaterial.Water && (blockCheck.Material == EnumMaterial.Glass || blockCheck.Material == EnumMaterial.Oil)) ? 1024 : 0;
            // Яркость берётся из данных блока
            return isDraw ? (storage.lightBlock[i] << 4 | storage.lightSky[i] & 0xF) + k : -1;
        }

        private void RenderMeshBlock()
        {
            int idB = 0;
            int idF = 0;
            Box[] boxes = block.GetBoxes(met, xc, zc, posChunkX, posChunkZ);
            int countB = boxes.Length;
            int countF = 0;
            while (idB < countB)
            {
                cBox = boxes[idB];
                countF = cBox.Faces.Length;
                idF = 0;
                while (idF < countF)
                {
                    cFace = cBox.Faces[idF];
                    if (cFace.side == -1)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            cSideInt = i;
                            RenderMeshSide();
                        }
                    }
                    else
                    {
                        cSideInt = cFace.side;
                        RenderMeshSide();
                    }
                    idF++;
                }
                idB++;
            }
        }

        /// <summary>
        /// Получть Сетку стороны блока с проверкой соседнего блока и разрушения его
        /// </summary>
        private void RenderMeshSide()
        {
            if (block.BlocksNotSame(met))
            {
                stateLight = stateLightHis == -1 ? resultSide[cSideInt] : stateLightHis;
            }
            else
            {
                stateLight = resultSide[cSideInt];
            }
            if (stateLight != -1)
            {
                stateLight = stateLight & 0xFF;
                RenderMeshFace((byte)stateLight);

                if (DamagedBlocksValue != -1)
                {
                    Face face = cFace;
                    cFace = new Face((Pole)cSideInt, 4032 + DamagedBlocksValue, true, cFace.color);
                    RenderMeshFace((byte)stateLight);
                    cFace = face;
                }
            }
        }

        /// <summary>
        /// Генерация сетки стороны коробки
        /// </summary>
        private void RenderMeshFace(byte light)
        {
            u1 = cFace.u1;
            v2 = cFace.v2;

            ColorsLights colorLight = GenColors(light);

            blockUV.colorsr = colorLight.colorr;
            blockUV.colorsg = colorLight.colorg;
            blockUV.colorsb = colorLight.colorb;
            blockUV.lights = colorLight.light;
            blockUV.v1x = cBox.From.x + posChunkX;
            blockUV.v1y = cBox.From.y + posChunkY0;
            blockUV.v1z = cBox.From.z + posChunkZ;
            blockUV.v2x = cBox.To.x + posChunkX;
            blockUV.v2y = cBox.To.y + posChunkY0;
            blockUV.v2z = cBox.To.z + posChunkZ;
            blockUV.u1x = u1 + cBox.UVFrom.x;
            blockUV.u1y = v2 + cBox.UVTo.y;
            blockUV.u2x = u1 + cBox.UVTo.x;
            blockUV.u2y = v2 + cBox.UVFrom.y;
            blockUV.animationFrame = cFace.animationFrame;
            blockUV.animationPause = cFace.animationPause;
            blockUV.yawUV = cBox.RotateYawUV;

            if (cBox.Translate.x != 0 || cBox.Translate.y != 0 || cBox.Translate.z != 0)
            {
                blockUV.translateX = cBox.Translate.x;
                blockUV.translateY = cBox.Translate.y;
                blockUV.translateZ = cBox.Translate.z;
                blockUV.isTranslate = true;
            }
            else
            {
                blockUV.isTranslate = false;
            }
            if (cBox.RotateYaw != 0 || cBox.RotatePitch != 0)
            {
                blockUV.posCenterX = posChunkX + .5f;
                blockUV.posCenterY = posChunkY0 + .5f;
                blockUV.posCenterZ = posChunkZ + .5f;
                blockUV.yaw = cBox.RotateYaw;
                blockUV.pitch = cBox.RotatePitch;
                blockUV.isRotate = true;
            }
            else
            {
                blockUV.isRotate = false;
            }
            blockUV.SideRotate(cSideInt);
        }

        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        private ColorsLights GenColors(byte light)
        {
            vec3 color = cFace.isColor ? GetBiomeColor(chunk, posChunkX, posChunkZ) : new vec3(1f);
            float lightPole = block.NoSideDimming ? 0f : 1f - LightPole();

            if (ambientOcclusion && (block.АmbientOcclusion || block.BiomeColor || block.Liquid))
            {
                AmbientOcclusionLights ambient = GetAmbientOcclusionLights();
                lightPole *= .5f;
                if (block.Liquid)
                {
                    return new ColorsLights(
                    ambient.GetColorNotAO(0, color, lightPole), ambient.GetColorNotAO(1, color, lightPole),
                    ambient.GetColorNotAO(2, color, lightPole), ambient.GetColorNotAO(3, color, lightPole),
                    ambient.GetLight(0, light), ambient.GetLight(1, light),
                    ambient.GetLight(2, light), ambient.GetLight(3, light)
                    );
                }
                return new ColorsLights(
                    ambient.GetColor(0, color, lightPole), ambient.GetColor(1, color, lightPole),
                    ambient.GetColor(2, color, lightPole), ambient.GetColor(3, color, lightPole),
                    ambient.GetLight(0, light), ambient.GetLight(1, light), 
                    ambient.GetLight(2, light), ambient.GetLight(3, light));
            }

            color.x -= lightPole; if (color.x < 0) color.x = 0;
            color.y -= lightPole; if (color.y < 0) color.y = 0;
            color.z -= lightPole; if (color.z < 0) color.z = 0;
            return new ColorsLights(color, light);
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        /// <returns></returns>
        private vec3 GetBiomeColor(ChunkBase chunk, int bx, int bz /* тип блока, трава, вода, листва */)
        {
            // подготовка для теста плавности цвета
            if (cFace.isColor)
            {
                if (block.EBlock == EnumBlock.Turf || block.EBlock == EnumBlock.TallGrass)
                {
                    return BlockColorBiome.Grass(chunk.biome[bx << 4 | bz]);
                }
                if (block.Material == EnumMaterial.Leaves)
                {
                    return BlockColorBiome.Leaves(chunk.biome[bx << 4 | bz]);
                }
                if (block.Material == EnumMaterial.Water)
                {
                    return BlockColorBiome.Water(chunk.biome[bx << 4 | bz]);
                }
                return cFace.color;
            }
            return new vec3(1f);
        }

        /// <summary>
        /// Получить все 4 вершины AmbientOcclusion и яркости от блока и неба
        /// </summary>
        private AmbientOcclusionLights GetAmbientOcclusionLights()
        {
            AmbientOcclusionLight a, b, c, d, e, f, g, h;
            switch (cSideInt)
            {
                case 0:
                    a = GetAmbientOcclusionLight(1, 1, 0);
                    b = GetAmbientOcclusionLight(0, 1, 1);
                    c = GetAmbientOcclusionLight(-1, 1, 0);
                    d = GetAmbientOcclusionLight(0, 1, -1);
                    e = GetAmbientOcclusionLight(-1, 1, -1);
                    f = GetAmbientOcclusionLight(-1, 1, 1);
                    g = GetAmbientOcclusionLight(1, 1, 1);
                    h = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 1:
                    c = GetAmbientOcclusionLight(1, -1, 0);
                    b = GetAmbientOcclusionLight(0, -1, 1);
                    a = GetAmbientOcclusionLight(-1, -1, 0);
                    d = GetAmbientOcclusionLight(0, -1, -1);
                    h = GetAmbientOcclusionLight(-1, -1, -1);
                    g = GetAmbientOcclusionLight(-1, -1, 1);
                    f = GetAmbientOcclusionLight(1, -1, 1);
                    e = GetAmbientOcclusionLight(1, -1, -1);
                    break;
                case 2:
                    b = GetAmbientOcclusionLight(1, 1, 0);
                    a = GetAmbientOcclusionLight(1, 0, 1);
                    d = GetAmbientOcclusionLight(1, -1, 0);
                    c = GetAmbientOcclusionLight(1, 0, -1);
                    e = GetAmbientOcclusionLight(1, -1, -1);
                    h = GetAmbientOcclusionLight(1, -1, 1);
                    g = GetAmbientOcclusionLight(1, 1, 1);
                    f = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 3:
                    b = GetAmbientOcclusionLight(-1, 1, 0);
                    c = GetAmbientOcclusionLight(-1, 0, 1);
                    d = GetAmbientOcclusionLight(-1, -1, 0);
                    a = GetAmbientOcclusionLight(-1, 0, -1);
                    h = GetAmbientOcclusionLight(-1, -1, -1);
                    e = GetAmbientOcclusionLight(-1, -1, 1);
                    f = GetAmbientOcclusionLight(-1, 1, 1);
                    g = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 4:
                    b = GetAmbientOcclusionLight(0, 1, -1);
                    a = GetAmbientOcclusionLight(1, 0, -1);
                    d = GetAmbientOcclusionLight(0, -1, -1);
                    c = GetAmbientOcclusionLight(-1, 0, -1);
                    e = GetAmbientOcclusionLight(-1, -1, -1);
                    h = GetAmbientOcclusionLight(1, -1, -1);
                    g = GetAmbientOcclusionLight(1, 1, -1);
                    f = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 5:
                    b = GetAmbientOcclusionLight(0, 1, 1);
                    c = GetAmbientOcclusionLight(1, 0, 1);
                    d = GetAmbientOcclusionLight(0, -1, 1);
                    a = GetAmbientOcclusionLight(-1, 0, 1);
                    h = GetAmbientOcclusionLight(-1, -1, 1);
                    e = GetAmbientOcclusionLight(1, -1, 1);
                    f = GetAmbientOcclusionLight(1, 1, 1);
                    g = GetAmbientOcclusionLight(-1, 1, 1);
                    break;
                default:
                    a = b = c = d = e = f = g = h = new AmbientOcclusionLight();
                    break;
            }
            return new AmbientOcclusionLights(new AmbientOcclusionLight[] { a, b, c, d, e, f, g, h });
        }

        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        private float LightPole()
        {
            switch (cSideInt)
            {
                case 0: return 1f;
                case 2: return 0.7f;
                case 3: return 0.7f;
                case 4: return 0.85f;
                case 5: return 0.85f;
            }
            return 0.6f;
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// Получть данные (AmbientOcclusion и яркость) одно стороны для вершины
        /// </summary>
        private AmbientOcclusionLight GetAmbientOcclusionLight(int x, int y, int z)
        {
            int pX = posChunkX + x;
            int pY = posChunkY + y;
            int pZ = posChunkZ + z;
            AmbientOcclusionLight aoLight = new AmbientOcclusionLight();

            xcn = (pX >> 4);
            zcn = (pZ >> 4);
            xc = chunk.Position.x + xcn;
            zc = chunk.Position.y + zcn;
            xb = pX & 15;
            zb = pZ & 15;
            //aoLight.color = GetBiomeColor(xb, zb);

            // проверка высоты
            if (pY < 0)
            {
                aoLight.lightSky = 0;
                aoLight.color = new vec3(1);
                return aoLight;
            }
            if (pY >= ChunkBase.COUNT_HEIGHT_BLOCK)
            {
                aoLight.lightSky = 15;
                aoLight.color = new vec3(1);
                aoLight.aol = 1;
                return aoLight;
            }
            yc = pY >> 4;
            // Определяем рабочий чанк соседнего блока
            chunkCheck = (xc == chunk.Position.x && zc == chunk.Position.y) ? chunk : chunk.Chunk(xcn, zcn);
            if (chunkCheck == null || !chunkCheck.IsChunkPresent)
            {
                aoLight.lightSky = 15;
                aoLight.color = new vec3(1);
                aoLight.aol = 1;
                return aoLight;
            }
            aoLight.color = GetBiomeColor(chunkCheck, xb, zb);
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
            aoLight.aol = blockCheck.IsNotTransparent() || blockCheck.Liquid ? 0 : 1;

            isDraw = id == 0 || (blockCheck.AllSideForcibly && blockCheck.UseNeighborBrightness);
            if (isDraw && (blockCheck.Material == block.Material && !blockCheck.BlocksNotSame(met))) isDraw = false;

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
        /// Структура для вершин цвета и освещения
        /// </summary>
        private struct ColorsLights
        {
            public byte[] colorr;
            public byte[] colorg;
            public byte[] colorb;
            public byte[] light;

            public ColorsLights(vec3 color, byte light)
            {
                byte c = (byte)(color.x * 255);
                colorr = new byte[] { c, c, c, c };
                c = (byte)(color.y * 255);
                colorg = new byte[] { c, c, c, c };
                c = (byte)(color.z * 255);
                colorb = new byte[] { c, c, c, c };
                this.light = new byte[] { light, light, light, light };
            }
            public ColorsLights(vec3 color1, vec3 color2, vec3 color3, vec3 color4,
                byte light1, byte light2, byte light3, byte light4)
            {
                colorr = new byte[] { (byte)(color1.x * 255), (byte)(color2.x * 255), (byte)(color3.x * 255), (byte)(color4.x * 255) };
                colorg = new byte[] { (byte)(color1.y * 255), (byte)(color2.y * 255), (byte)(color3.y * 255), (byte)(color4.y * 255) };
                colorb = new byte[] { (byte)(color1.z * 255), (byte)(color2.z * 255), (byte)(color3.z * 255), (byte)(color4.z * 255) };
                light = new byte[] { light1, light2, light3, light4 };
            }
        }
        /// <summary>
        /// Структура для 4-ёх вершин цвета и освещения
        /// </summary>
        private struct AmbientOcclusionLights
        {
            private readonly int[] lightBlock;
            private readonly int[] lightSky;
            private readonly vec3[] colors;
            private readonly int[] aols;
            private readonly int[] aocs;

            public AmbientOcclusionLights(AmbientOcclusionLight[] aos)
            {
                // a, b, c, d, e, f, g, h
                // 0, 1, 2, 3, 4, 5, 6, 7

                lightBlock = new int[] {
                    aos[2].lightBlock + aos[3].lightBlock + aos[4].lightBlock,
                    aos[1].lightBlock + aos[2].lightBlock + aos[5].lightBlock,
                    aos[0].lightBlock + aos[1].lightBlock + aos[6].lightBlock,
                    aos[0].lightBlock + aos[3].lightBlock + aos[7].lightBlock
                };
                lightSky = new int[] {
                    aos[2].lightSky + aos[3].lightSky + aos[4].lightSky,
                    aos[1].lightSky + aos[2].lightSky + aos[5].lightSky,
                    aos[0].lightSky + aos[1].lightSky + aos[6].lightSky,
                    aos[0].lightSky + aos[3].lightSky + aos[7].lightSky
                };
                colors = new vec3[]
                {
                    aos[2].color + aos[3].color + aos[4].color,
                    aos[1].color + aos[2].color + aos[5].color,
                    aos[0].color + aos[1].color + aos[6].color,
                    aos[0].color + aos[3].color + aos[7].color
                };
                aols = new int[]
                {
                    aos[2].aol + aos[3].aol + aos[4].aol,
                    aos[1].aol + aos[2].aol + aos[5].aol,
                    aos[0].aol + aos[1].aol + aos[6].aol,
                    aos[0].aol + aos[3].aol + aos[7].aol
                };
                aocs = new int[]
                {
                    aos[2].aoc + aos[3].aoc + aos[4].aoc,
                    aos[1].aoc + aos[2].aoc + aos[5].aoc,
                    aos[0].aoc + aos[1].aoc + aos[6].aoc,
                    aos[0].aoc + aos[3].aoc + aos[7].aoc
                };
            }
            
            public byte GetLight(int index, byte light)
            {
                byte lb = (byte)((light & 0xF0) >> 4);
                byte ls = (byte)(light & 0xF);
                int count = 1 + aols[index];
                lb = (byte)((lightBlock[index] + lb) / count);
                ls = (byte)((lightSky[index] + ls) / count);

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
        private struct AmbientOcclusionLight
        {
            public byte lightBlock;
            public byte lightSky;
            public vec3 color;
            public int aol;
            public int aoc;
        }

        #region Liquid

        /// <summary>
        /// Рендер сетки жидкого блока
        /// </summary>
        private void RenderMeshBlockLiquid()
        {
            material = blockState.GetBlock().Material;
            int y = posChunkY + 1;
            // Получаем данные вверхнего уровня блока, в облости 3*3
            int up00 = GetLiquidBlock(posChunkX - 1, y, posChunkZ - 1, true);
            int up10 = GetLiquidBlock(posChunkX, y, posChunkZ - 1, true);
            int up20 = GetLiquidBlock(posChunkX + 1, y, posChunkZ - 1, true);
            int up01 = GetLiquidBlock(posChunkX - 1, y, posChunkZ, true);
            int up11 = GetLiquidBlock(posChunkX, y, posChunkZ, false);
            int up21 = GetLiquidBlock(posChunkX + 1, y, posChunkZ, true);
            int up02 = GetLiquidBlock(posChunkX - 1, y, posChunkZ + 1, true);
            int up12 = GetLiquidBlock(posChunkX, y, posChunkZ + 1, true);
            int up22 = GetLiquidBlock(posChunkX + 1, y, posChunkZ + 1, true);

            // Получаем данные этого уровня блока, в облости 3*3
            int l00 = GetLiquidBlock(posChunkX - 1, posChunkY, posChunkZ - 1, true);
            int l10 = GetLiquidBlock(posChunkX, posChunkY, posChunkZ - 1, true);
            int l20 = GetLiquidBlock(posChunkX + 1, posChunkY, posChunkZ - 1, true);
            int l01 = GetLiquidBlock(posChunkX - 1, posChunkY, posChunkZ, true);
            int l11 = GetLiquidBlock(posChunkX, posChunkY, posChunkZ, false);
            int l21 = GetLiquidBlock(posChunkX + 1, posChunkY, posChunkZ, true);
            int l02 = GetLiquidBlock(posChunkX - 1, posChunkY, posChunkZ + 1, true);
            int l12 = GetLiquidBlock(posChunkX, posChunkY, posChunkZ + 1, true);
            int l22 = GetLiquidBlock(posChunkX + 1, posChunkY, posChunkZ + 1, true);

            // Получить высоту вершины 0..1
            float h00 = HeightVertexLiquid(up00, up10, up01, up11, new int[] { l00, l10, l01, l11 });
            float h10 = HeightVertexLiquid(up10, up20, up11, up21, new int[] { l10, l20, l11, l21 });
            float h01 = HeightVertexLiquid(up01, up11, up02, up12, new int[] { l01, l11, l02, l12 });
            float h11 = HeightVertexLiquid(up11, up21, up12, up22, new int[] { l11, l21, l12, l22 });

            blockUV.isRotate = false;
            blockUV.isTranslate = false;
            blockUV.yawUV = 0;
            blockUV.bufferCache.Clear();

            float u1, u2, u3, u4, v1, v2, v3, v4;

            if (resultSide[0] != -1) // Up
            {
                float angleFlow = BlockAbLiquid.GetAngleFlow(l11, l01, l10, l12, l21);

                if (angleFlow < -999)
                {
                    // Без вращения
                    cFace = block.GetFace(0);
                    u1 = cFace.u1;
                    v1 = cFace.v2;
                    u2 = cFace.u1;
                    v2 = cFace.v2 + .015625f;
                    u3 = cFace.u1 + .015625f;
                    v3 = cFace.v2 + .015625f;
                    u4 = cFace.u1 + .015625f;
                    v4 = cFace.v2;
                }
                else
                {
                    // Вращаем
                    cFace = block.GetFace(1);
                    float fs = glm.sin(angleFlow) * .25f;
                    float fc = glm.cos(angleFlow) * .25f;
                    u1 = cFace.u1 + (-fc - fs) * .03125f;
                    v1 = cFace.v2 + .015625f + (-fc + fs) * .03125f;
                    u2 = cFace.u1 + (-fc + fs) * .03125f;
                    v2 = cFace.v2 + .015625f + (fc + fs) * .03125f;
                    u3 = cFace.u1 + (fc + fs) * .03125f;
                    v3 = cFace.v2 + .015625f + (fc - fs) * .03125f;
                    u4 = cFace.u1 + (fc - fs) * .03125f;
                    v4 = cFace.v2 + .015625f + (-fc - fs) * .03125f;
                }

                cSideInt = 0;

                ColorsLights colorLight = GenColors(GetLightMix(resultSide[0], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;
                blockUV.animationFrame = cFace.animationFrame;
                blockUV.animationPause = cFace.animationPause;
                // Вверх
                blockUV.BufferSideOutsideCache(
                    posChunkX, posChunkY0 + h00, posChunkZ,
                    posChunkX, posChunkY0 + h01, posChunkZ + 1,
                    posChunkX + 1, posChunkY0 + h11, posChunkZ + 1,
                    posChunkX + 1, posChunkY0 + h10, posChunkZ,
                    u1, v1, u2, v2, u3, v3, u4, v4
                );

                // Условие, если тикучая вода или стоячая но над стоячей вверхний блок 
                // не может быть стоячей водной и спложным блоком
                if (l11 != 15 || (l11 == 15 && CheckUp(new int[] { up00, up10, up20, up01, up11, up21, up02, up12, up22 })))
                {
                    // из нутри
                    blockUV.BufferSideInside(
                        posChunkX, posChunkY0 + h00, posChunkZ,
                        posChunkX, posChunkY0 + h01, posChunkZ + 1,
                        posChunkX + 1, posChunkY0 + h11, posChunkZ + 1,
                        posChunkX + 1, posChunkY0 + h10, posChunkZ,
                        u1, v1, u2, v2, u3, v3, u4, v4
                    );
                }
            }
            if (resultSide[1] != -1) // Down
            {
                cFace = block.GetFace(0);
                cSideInt = 1;
                ColorsLights colorLight = GenColors(GetLightMix(resultSide[1], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;
                blockUV.animationFrame = cFace.animationFrame;
                blockUV.animationPause = cFace.animationPause;
                
                u1 = cFace.u1;
                v1 = cFace.v2;
                u2 = cFace.u1 + .015625f;
                v2 = cFace.v2 + .015625f;

                blockUV.BufferSideOutsideCache(
                    posChunkX + 1, posChunkY0, posChunkZ,
                    posChunkX + 1, posChunkY0, posChunkZ + 1,
                    posChunkX, posChunkY0, posChunkZ + 1,
                    posChunkX, posChunkY0, posChunkZ,
                    u2, v1, u2, v2, u1, v2, u1, v1
                );
            }

            cFace = block.GetFace(1);
            blockUV.animationFrame = cFace.animationFrame;
            blockUV.animationPause = cFace.animationPause;

            if (resultSide[2] != -1) // East
            {
                cSideInt = 2;
                ColorsLights colorLight = GenColors(GetLightMix(resultSide[2], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = cFace.u1;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = cFace.v2 + .015625f;
                v2 = v1 - .015625f * h10;
                v3 = v1 - .015625f * h11;

                blockUV.BufferSideTwo(
                    posChunkX + 1, posChunkY0, posChunkZ,
                    posChunkX + 1, posChunkY0 + h10, posChunkZ,
                    posChunkX + 1, posChunkY0 + h11, posChunkZ + 1,
                    posChunkX + 1, posChunkY0, posChunkZ + 1,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[2] > 1000
                );
            }

            if (resultSide[3] != -1) // West
            {
                cSideInt = 3;
                ColorsLights colorLight = GenColors(GetLightMix(resultSide[3], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = cFace.u1;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = cFace.v2 + .015625f;
                v3 = v1 - .015625f * h00;
                v2 = v1 - .015625f * h01;

                blockUV.BufferSideTwo(
                    posChunkX, posChunkY0, posChunkZ + 1,
                    posChunkX, posChunkY0 + h01, posChunkZ + 1,
                    posChunkX, posChunkY0 + h00, posChunkZ,
                    posChunkX, posChunkY0, posChunkZ,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[3] > 1000
                );
            }

            if (resultSide[4] != -1) // North
            {
                cSideInt = 4;
                ColorsLights colorLight = GenColors(GetLightMix(resultSide[4], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = cFace.u1;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = cFace.v2 + .015625f;
                v2 = v1 - .015625f * h00;
                v3 = v1 - .015625f * h10;

                blockUV.BufferSideTwo(
                    posChunkX, posChunkY0, posChunkZ,
                    posChunkX, posChunkY0 + h00, posChunkZ,
                    posChunkX + 1, posChunkY0 + h10, posChunkZ,
                    posChunkX + 1, posChunkY0, posChunkZ,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[4] > 1000
                );
            }

            if (resultSide[5] != -1) // South
            {
                cSideInt = 5;
                ColorsLights colorLight = GenColors(GetLightMix(resultSide[5], stateLightHis));
                blockUV.colorsr = colorLight.colorr;
                blockUV.colorsg = colorLight.colorg;
                blockUV.colorsb = colorLight.colorb;
                blockUV.lights = colorLight.light;

                u3 = u4 = cFace.u1;
                u1 = u2 = u3 + .015625f;
                v1 = v4 = cFace.v2 + .015625f;
                v3 = v1 - .015625f * h01;
                v2 = v1 - .015625f * h11;

                blockUV.BufferSideTwo(
                    posChunkX + 1, posChunkY0, posChunkZ + 1,
                    posChunkX + 1, posChunkY0 + h11, posChunkZ + 1,
                    posChunkX, posChunkY0 + h01, posChunkZ + 1,
                    posChunkX, posChunkY0, posChunkZ + 1,
                    u1, v1, u2, v2, u3, v3, u4, v4,
                    resultSide[5] > 1000
                );
            }

            blockUV.AddBufferCache();
        }

        /// <summary>
        /// Смекшировать яркость, в зависимости от требований самой яркой
        /// </summary>
        private byte GetLightMix(int stateLight, int stateLightHis)
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

        #endregion
    }
}
