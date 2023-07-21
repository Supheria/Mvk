using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Часть блока крафтого стола
    /// </summary>
    public abstract class BlockAbCraftingTable : BlockBase
    {
        /***
         * Met
         * 0 bit - Level (высота, 0 нижний ряд, 1 верхний ряд)
         * 1-2 bit - Box (квадрат 2*2, 0 х1z1, 1 x1z2, 2 x2z1, 3 x2z2)
         * 
         * level = met & 1 != 0;
         * box
         *  x = met & 2 != 0; 
         *  z = met & 3 != 0; 
         *  
         * met = z << 2 | x << 1 | level;
         */

        /// <summary>
        /// Номер окна крафта
        /// </summary>
        protected int window;

        public BlockAbCraftingTable()
        {
            Combustibility = true;
            IgniteOddsSunbathing = 10;
            BurnOdds = 20;
            Material = Materials.GetMaterialCache(EnumMaterial.WoodTable);
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
            Particle = 138;
            Resistance = 5.0f;
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 80;

        /// <summary>
        /// Стороны целого блока для рендера 0 - 3 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met & 1];

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        protected override int QuantityDropped(Rand random) => 8;

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            if (!worldIn.IsRemote && entityPlayer is EntityPlayerServer entityPlayerServer)
            {
                // Открыть окно крафта 2
                entityPlayerServer.SendPacket(new PacketS31WindowProperty(window));
            }
            return true;
        }

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        public override void OnBreakBlock(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            // Ламаем всю дверь
            int met = state.met;
            int level = met & 1;
            int boxX = (met & 2) != 0 ? 1 : 0;
            int boxZ = (met & 4) != 0 ? 1 : 0;
            int x, y, z;
            for (x = 0; x < 2; x++)
            {
                for (z = 0; z < 2; z++)
                {
                    for (y = 0; y < 2; y++)
                    {
                        worldIn.SetBlockToAir(blockPos.Offset(x - boxX, y - level, z - boxZ), 12);
                    }
                }
            }
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads(int textureUp, int textureDown, int textureSideUp, int textureSideDown)
        {
            quads = new QuadSide[][]
            {
                new QuadSide[] {
                    new QuadSide(0).SetTexture(textureDown).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(textureSideDown).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(textureSideDown).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(textureSideDown).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(textureSideDown).SetSide(Pole.South)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(textureUp).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(textureSideUp).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(textureSideUp).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(textureSideUp).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(textureSideUp).SetSide(Pole.South)
                }
            };
        }
        
    }
}
