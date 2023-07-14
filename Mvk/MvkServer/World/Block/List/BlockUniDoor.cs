using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект двери
    /// </summary>
    public class BlockUniDoor : BlockBase
    {
        /***
         * Met
         * 0 bit - Open/Close 
         * 1 bit - Left/Right
         * 2-3 bit - Pole
         * 4-5 bit - Level (высота, 0 нижний ряд, 3 верхний ряд)
         * 6-7 bit - Box (квадрат 2*2, 0 х1z1, 1 x1z2, 2 x2z1, 3 x2z2)
         * 
         * Open = met & 1 != 0;
         * Left = met & 2 != 0;
         * pole = met >> 2 & 3;
         *  East = pole == 0
         *  West = pole == 1
         *  North = pole == 2
         *  South = pole == 3
         * level = met >> 4 & 3;
         * box
         *  x = met & 64 != 0; 
         *  z = met & 128 != 0; 
         *  
         * met = z << 7 | x << 6 | level << 4 | pole << 2 | left << 1 | open;
         */

        /// <summary>
        /// Номера текстуры торца
        /// 0 1
        /// 2 3 
        /// 4 5
        /// 6 7
        /// </summary>
        private readonly int[] numberTexture;

        private readonly EnumItem itemDoor;

        /// <summary>
        /// Конверт данных в байт
        /// </summary>
        public static ushort ConvMetData(bool open, bool left, int pole, int level, int x, int z)
            => (ushort)(z << 7 | x << 6 | level << 4 | pole << 2 | (left ? 1 : 0) << 1 | (open ? 1 : 0));

        public BlockUniDoor(int numberTexture, EnumItem itemDoor)
        {
            this.itemDoor = itemDoor;
            this.numberTexture = new int[]
            {
                numberTexture, numberTexture + 1,
                numberTexture + 64, numberTexture + 65,
                numberTexture + 128, numberTexture + 129,
                numberTexture + 192, numberTexture + 193,
            };
            IsAddMet = true;
            SetUnique();
            Combustibility = true;
            IgniteOddsSunbathing = 10;
            BurnOdds = 20;
            Material = Materials.GetMaterialCache(EnumMaterial.Door);
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
            Particle = numberTexture;
            Resistance = 5.0f;
            InitQuads();
        }

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public override bool IsLiquidDestruction() => true;

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 20;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
            => Items.GetItemCache(itemDoor);

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => (met & 1) != 0; // open

        /// <summary>
        /// Является ли блок проходимым на нём, т.е. можно ли ходить по нему
        /// </summary>
        public override bool IsPassableOnIt(int met) => false;

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            bool open = (met & 1) != 0;
            Pole pole = (Pole)(2 + (met >> 2 & 3));

            if (open)
            {
                bool left = (met & 2) != 0;
                if (pole == Pole.North) pole = left ? Pole.East : Pole.West;
                else if (pole == Pole.South) pole = left ? Pole.West : Pole.East;
                else if (pole == Pole.West) pole = left ? Pole.North : Pole.South;
                else if (pole == Pole.East) pole = left ? Pole.South : Pole.North;
            }

            int z = (met & 128) != 0 ? 1 : 0;
            if (pole == Pole.North && z == 0) return new AxisAlignedBB[] 
                { new AxisAlignedBB(pos.X, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + .1875f) };
            if (pole == Pole.South && z == 1) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X, pos.Y, pos.Z + .8125f, pos.X + 1, pos.Y + 1, pos.Z + 1) };
            int x = (met & 64) != 0 ? 1 : 0;
            if (pole == Pole.West && x == 0) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X, pos.Y, pos.Z, pos.X + .1875f, pos.Y + 1, pos.Z + 1) };
            if (pole == Pole.East && x == 1) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X + .8125f, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + 1) };

            return new AxisAlignedBB[0];
        }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            int met = state.met;
            bool open = (met & 1) == 0;
            bool left = (met & 2) != 0;
            int pole = met >> 2 & 3;
            int level = met >> 4 & 3;
            int boxX = (met & 64) != 0 ? 1 : 0;
            int boxZ = (met & 128) != 0 ? 1 : 0;
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        worldIn.SetBlockStateMet(blockPos.Offset(x - boxX, y - level, z - boxZ),
                            ConvMetData(open, left, pole, y, x, z));
                    }
                }
            }
            if (!worldIn.IsRemote)
            {
                worldIn.PlaySound(open ? AssetsSample.DoorOpen : AssetsSample.DoorClose, blockPos.ToVec3() + .5f, 1f, 1f);
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
            int level = met >> 4 & 3;
            int boxX = (met & 64) != 0 ? 1 : 0;
            int boxZ = (met & 128) != 0 ? 1 : 0;
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        worldIn.SetBlockToAir(blockPos.Offset(x - boxX, y - level, z - boxZ), 12);
                    }
                }
            }
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            int met = neighborState.met;
            bool open = (met & 1) != 0;
            bool left = (met & 2) != 0;
            Pole pole = (Pole)(2 + (met >> 2 & 3));
            int level = met >> 4 & 3;
            int x = (met & 64) != 0 ? 1 : 0;
            int z = (met & 128) != 0 ? 1 : 0;

            if (!IsCanDoorStay(worldIn, blockPos.Offset(-x, -level, -z), open, left, pole))
            {
                OnBreakBlock(worldIn, blockPos, neighborState);
                DropBlockAsItem(worldIn, blockPos, neighborState);
                if (!worldIn.IsRemote) 
                {
                    PlaySoundBreak(worldIn, blockPos);
                }
            }
        }

        /// <summary>
        /// Может ли тут стоять дверь
        /// </summary>
        public bool IsCanDoorStay(WorldBase worldIn, BlockPos blockPos, bool open, bool left, Pole pole)
        {
            int x1, x2, z1, z2;
            x1 = x2 = z1 = z2 = 0;
            if (pole == Pole.North)
            {
                x1 = x2 = left ? 2 : -1;
                z1 = 0;
                z2 = -1;
            }
            else if (pole == Pole.South)
            {
                x1 = x2 = left ? -1 : 2;
                z1 = 1;
                z2 = 2;
            }
            else if (pole == Pole.West)
            {
                z1 = z2 = left ? -1 : 2;
                x1 = 0;
                x2 = -1;
            }
            else if (pole == Pole.East)
            {
                z1 = z2 = left ? 2 : -1;
                x1 = 1;
                x2 = 2;
            }
            else
            {
                return false;
            }

            return ((worldIn.GetBlockState(blockPos.Offset(x1, 0, z1)).GetBlock().FullBlock
                        || worldIn.GetBlockState(blockPos.Offset(x2, 0, z2)).GetBlock().FullBlock))
                        && ((worldIn.GetBlockState(blockPos.Offset(x1, 3, z1)).GetBlock().FullBlock
                        || worldIn.GetBlockState(blockPos.Offset(x2, 3, z2)).GetBlock().FullBlock));
        }

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb)
        {
            bool open = (met & 1) != 0;
            bool left = (met & 2) != 0;
            Pole pole = (Pole)(2 + (met >> 2 & 3));
            int level = met >> 4 & 3;
            int x = (met & 64) != 0 ? 1 : 0;
            int z = (met & 128) != 0 ? 1 : 0;

            if (open)
            {
                if (pole == Pole.North) pole = left ? Pole.East : Pole.West;
                else if (pole == Pole.South) pole = left ? Pole.West : Pole.East;
                else if (pole == Pole.West) pole = left ? Pole.North : Pole.South;
                else if (pole == Pole.East) pole = left ? Pole.South : Pole.North;
                left = !left;
            }

            if (pole == Pole.North && z == 0) return quads[level + (left ? (x == 0 ? 12 : 8) : (x == 0 ? 0 : 4))];
            if (pole == Pole.South && z == 1) return quads[32 + level + (left ? (x == 0 ? 8 : 12) : (x == 0 ? 4 : 0))];
            if (pole == Pole.West && x == 0) return quads[16 + level + (left ? (z == 0 ? 8 : 12) : (z == 0 ? 4 : 0))];
            if (pole == Pole.East && x == 1) return quads[48 + level + (left ? (z == 0 ? 12 : 8) : (z == 0 ? 0 : 4))];

            return quads[64];
        }

        #region InitQuads

        private QuadSide[] Bs(bool up, bool upOrDown, int it, float angle, bool butt, bool rev, bool loop)
        {
            int n0 = rev ? 16 : 0;
            int n16 = rev ? 0 : 16;

            QuadSide[] sides = new QuadSide[upOrDown ? 4 : 3];
            sides[0] = Quad(0, 16, 0, 3, n16, n0, 0, 16, Pole.North, numberTexture[it]);
            sides[0].SetRotate(angle);
            sides[1] = Quad(0, 16, 0, 3, n0, n16, 0, 16, Pole.South, numberTexture[it]);
            sides[1].SetRotate(angle);
            sides[2] = Quad(0, 16, 0, 3, loop ? 0 : 15, loop ? 1 : 16, 0, 16, butt ? Pole.West : Pole.East, numberTexture[it]);
            sides[2].SetRotate(angle);
            if (upOrDown)
            {
                sides[3] = up ? Quad(0, 16, 0, 3, n16, n0, 0, 1, Pole.Up, numberTexture[it])
                    : Quad(0, 16, 0, 3, n0, n16, 15, 16, Pole.Down, numberTexture[it]);
                sides[3].SetRotate(angle);
            }
            return sides;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads()
        {
            int i, it;
            float[] angle;
            quads = new QuadSide[65][];
            quads[64] = new QuadSide[0];
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };

            bool up, upOrDown;
            int j = 0;
            for (j = 0; j < 4; j++)
            {
                for (i = 0; i < 4; i++)
                {
                    up = i == 3;
                    upOrDown = up || i == 0;
                    it = 6 - i * 2;
                    // Петля
                    quads[j * 16 + i] = Bs(up, upOrDown, it, angle[j], true, false, true);
                    // Петля реверс
                    quads[j * 16 + 8 + i] = Bs(up, upOrDown, it, angle[j], false, true, true);

                    it = 7 - i * 2;
                    // Ручка
                    quads[j * 16 + 4 + i] = Bs(up, upOrDown, it, angle[j], false, false, false);
                    // Ручка реверс
                    quads[j * 16 + 12 + i] = Bs(up, upOrDown, it, angle[j], true, true, false);
                }
            }
        }

        #endregion
    }
}
