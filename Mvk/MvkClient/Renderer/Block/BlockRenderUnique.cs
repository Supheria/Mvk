using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера уникальных блоков, без проверки соседних блоков
    /// </summary>
    public class BlockRenderUnique : BlockRenderFull
    {
        private EnumMaterial material;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRenderUnique() : base() { }

        /// <summary>
        /// Получть Сетку блока с возможностью двух сторон
        /// </summary>
        /// <returns>сетка</returns>
        public override void RenderMesh()
        {
            material = block.Material;
            xc = chunk.Position.x;
            zc = chunk.Position.y;
            yc = posChunkY >> 4;
            damagedBlocksValue = chunk.GetDestroyBlocksValue(posChunkX, posChunkY, posChunkZ);
            stateLightHis = blockState.lightBlock << 4 | blockState.lightSky & 0xF;
            if (block.UseNeighborBrightness)
            {
                // Если нужна проверка яркости соседних блоков, то проверяем
                stateLight = posChunkY + 1 > ChunkBase.COUNT_HEIGHT_BLOCK ? 0x0F : GetBlockSideState(posChunkX, posChunkY + 1, posChunkZ, false, true);
                CheckNeighborBrightness();
                stateLight = posChunkY - 1 < 0 ? 0x0F : GetBlockSideState(posChunkX, posChunkY - 1, posChunkZ, false);
                CheckNeighborBrightness();
                yc = posChunkY >> 4;
                stateLight = GetBlockSideState(posChunkX + 1, posChunkY, posChunkZ, true);
                CheckNeighborBrightness();
                stateLight = GetBlockSideState(posChunkX - 1, posChunkY, posChunkZ, true);
                CheckNeighborBrightness();
                stateLight = GetBlockSideState(posChunkX, posChunkY, posChunkZ - 1, true);
                CheckNeighborBrightness();
                stateLight = GetBlockSideState(posChunkX, posChunkY, posChunkZ + 1, true);
                CheckNeighborBrightness();
            }
            RenderMeshBlockUnique();
        }

        private void CheckNeighborBrightness()
        {
            if (stateLight != stateLightHis && stateLight != -1)
            {
                stateLight = stateLight & 0xFF;
                s1 = stateLight >> 4;
                s2 = stateLight & 0x0F;
                s3 = stateLightHis >> 4;
                s4 = stateLightHis & 0x0F;

                s5 = s1 > s3 ? s1 : s3;
                s6 = s2 > s4 ? s2 : s4;

                stateLightHis = (s5 << 4 | s6 & 0xF);
            }
        }

        /// <summary>
        /// Рендер сеток сторон блока которые требуются для прорисовки
        /// </summary>
        private void RenderMeshBlockUnique()
        {
            rectangularSides = block.GetQuads(met, xc, zc, posChunkX, posChunkZ);
            count = rectangularSides.Length;
            for (i = 0; i < count; i++)
            {
                rectangularSide = rectangularSides[i];
                
                lightPole = rectangularSide.lightPole;
                GenColors((byte)stateLightHis);

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
                blockUV.BuildingWind(rectangularSide.wind);

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
}
