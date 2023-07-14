using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект стекляных панелей
    /// </summary>
    public class BlockUniGlassPane : BlockBase
    {
        /***
         * Met
         * 4 bit - pole
         * 
         * South = pole & 1 != 0
         * East = pole & 2 != 0
         * North = pole & 4 != 0
         * West = pole & 8 != 0
         */

        /// <summary>
        /// Номер текстуры торца
        /// </summary>
        private readonly int numberTextureButt;
        /// <summary>
        /// Номер текстуры стороны
        /// </summary>
        private readonly int numberTextureSide;

        public BlockUniGlassPane(int numberTextureSide, int numberTextureButt, vec3 color, bool translucent = true)
        {
            Translucent = translucent;
            this.numberTextureButt = numberTextureButt;
            this.numberTextureSide = numberTextureSide;
            SetUnique();
            base.color = color;
            Particle = numberTextureSide;
            Resistance = .6f;
            canDropPresent = false;
            Material = Materials.GetMaterialCache(EnumMaterial.GlassPane);
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
            InitQuads();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 10;

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met];
        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override QuadSide[] GetQuadsGui() => quads[10];

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            // 7 = .4375f 9 = .5625f
            switch (met)
            {
                // один
                case 1: return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .4375f, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f) };
                case 2: return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f) };
                case 4: return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + .5625f) };
                case 8: return new AxisAlignedBB[] { new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + .5625f, pos.Y + 1f, pos.Z + .5625f) };
                // два прямых
                case 5: return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f) };
                case 10: return new AxisAlignedBB[] { new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f) };
                // угол
                case 3: return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .4375f, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f),
                    new AxisAlignedBB(pos.X + .5625f, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f)
                };
                case 6:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f),
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + .4375f)
                };
                case 12:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + .5625f),
                    new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + .4375f, pos.Y + 1f, pos.Z + .5625f)
                };
                case 9:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + .5625f, pos.Y + 1f, pos.Z + .5625f),
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .5625f, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f)
                };
                // три стороны
                case 7:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f),
                    new AxisAlignedBB(pos.X + .5625f, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f)
                };
                case 13:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f),
                    new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + .4375f, pos.Y + 1f, pos.Z + .5625f)
                };
                case 14:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f),
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + .4375f)
                };
                case 11:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f),
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .5625f, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f)
                };
                // Все стороны
                case 15:
                    return new AxisAlignedBB[] {
                    new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z, pos.X + .5625f, pos.Y + 1f, pos.Z + 1f),
                    new AxisAlignedBB(pos.X + .5625f, pos.Y, pos.Z + .4375f, pos.X + 1f, pos.Y + 1f, pos.Z + .5625f),
                    new AxisAlignedBB(pos.X, pos.Y, pos.Z + .4375f, pos.X + .4375f, pos.Y + 1f, pos.Z + .5625f)
                };
            }
            // без соединения 0
            return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .4375f, pos.Y, pos.Z + .4375f, pos.X + .5625f, pos.Y + 1f, pos.Z + .5625f) };
        }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            return state.NewMet(MetUpdate(worldIn, blockPos));
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            ushort met = MetUpdate(worldIn, blockPos);
            if (met != neighborState.met) worldIn.SetBlockStateMet(blockPos, met);
        }

        private ushort MetUpdate(WorldBase worldIn, BlockPos blockPos)
        {
            ushort pole = 0;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetSouth())) pole |= 1;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetEast())) pole |= 2;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetNorth())) pole |= 4;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetWest())) pole |= 8;
            return pole;
        }

        /// <summary>
        /// Блок имеет твердую непрозрачную поверхность и стекло
        /// </summary>
        private bool DoesBlockHaveSolidTopSurface(WorldBase worldIn, BlockPos blockPos)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return (block.IsNotTransparent() && block.FullBlock) || block.Material.Glass;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads()
        {
            int i, j, index;
            int[] ar;
            float[] angle;
            quads = new QuadSide[16][];
            // Нет сторон
            quads[0] = new QuadSide[6];
            quads[0][0] = QuadColor(7, 9, 7, 9, 7, 9, 7, 9, Pole.Up, numberTextureButt);
            quads[0][1] = QuadColor(7, 9, 7, 9, 7, 9, 7, 9, Pole.Down, numberTextureButt);
            quads[0][2] = QuadColor(7, 9, 7, 9, 7, 9, 0, 16, Pole.North, numberTextureSide);
            quads[0][3] = QuadColor(7, 9, 7, 9, 7, 9, 0, 16, Pole.South, numberTextureSide);
            quads[0][4] = QuadColor(7, 9, 7, 9, 7, 9, 0, 16, Pole.East, numberTextureSide);
            quads[0][5] = QuadColor(7, 9, 7, 9, 7, 9, 0, 16, Pole.West, numberTextureSide);

            // одна сторона
            ar = new int[] { 1, 2, 4, 8 };
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };
            for (i = 0; i < 4; i++)
            {
                index = ar[i];
                quads[index] = new QuadSide[6];
                quads[index][0] = QuadColor(7, 9, 7, 16, 7, 9, 0, 9, Pole.Up, numberTextureButt);
                quads[index][1] = QuadColor(7, 9, 7, 16, 7, 9, 0, 9, Pole.Down, numberTextureButt);
                quads[index][2] = QuadColor(7, 9, 7, 16, 7, 9, 0, 16, Pole.North, numberTextureButt);
                quads[index][3] = QuadColor(7, 9, 7, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                quads[index][4] = QuadColor(7, 9, 7, 16, 0, 9, 0, 16, Pole.East, numberTextureSide);
                quads[index][5] = QuadColor(7, 9, 7, 16, 7, 16, 0, 16, Pole.West, numberTextureSide);
                for (j = 0; j < 6; j++) quads[index][j].SetRotate(angle[i]);
            }

            // угол 2 стороны
            ar = new int[] { 3, 6, 12, 9 };
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };

            for (i = 0; i < 4; i++)
            {
                index = ar[i];
                quads[index] = new QuadSide[10];
                quads[index][0] = QuadColor(7, 9, 7, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                quads[index][0].SetRotate(glm.pi90 + angle[i]);
                quads[index][1] = QuadColor(7, 9, 9, 16, 7, 9, 0, 9, Pole.Up, numberTextureButt);
                quads[index][1].SetRotate(glm.pi90 + angle[i]);
                quads[index][2] = QuadColor(7, 9, 9, 16, 7, 9, 0, 9, Pole.Down, numberTextureButt);
                quads[index][2].SetRotate(glm.pi90 + angle[i]);

                quads[index][3] = QuadColor(7, 9, 7, 16, 7, 9, 0, 9, Pole.Up, numberTextureButt);
                quads[index][4] = QuadColor(7, 9, 7, 16, 7, 9, 0, 9, Pole.Down, numberTextureButt);
                quads[index][5] = QuadColor(7, 9, 7, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                quads[index][6] = QuadColor(7, 9, 7, 16, 7, 16, 0, 16, Pole.West, numberTextureSide);
                quads[index][7] = QuadColor(7, 16, 7, 16, 0, 9, 0, 16, Pole.North, numberTextureSide);
                quads[index][8] = QuadColor(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
                quads[index][9] = QuadColor(9, 16, 7, 9, 9, 16, 0, 16, Pole.South, numberTextureSide);
                for (j = 3; j < 10; j++) quads[index][j].SetRotate(angle[i]);
            }
            // прямые 2 стороны
            ar = new int[] { 5, 10 };
            angle = new float[] { 0, glm.pi90 };
            for (i = 0; i < 2; i++)
            {
                index = ar[i];
                quads[index] = new QuadSide[6];
                quads[index][0] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.Up, numberTextureButt);
                quads[index][1] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.Down, numberTextureButt);
                quads[index][2] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.North, numberTextureButt);
                quads[index][3] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                quads[index][4] = QuadColor(7, 9, 0, 16, 0, 16, 0, 16, Pole.East, numberTextureSide);
                quads[index][5] = QuadColor(7, 9, 0, 16, 0, 16, 0, 16, Pole.West, numberTextureSide);
                for (j = 0; j < 6; j++) quads[index][j].SetRotate(angle[i]);
            }
            // три стороны
            ar = new int[] { 7, 14, 13, 11 };
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };
            for (i = 0; i < 4; i++)
            {
                index = ar[i];
                quads[index] = new QuadSide[11];
                quads[index][0] = QuadColor(7, 9, 9, 16, 7, 9, 0, 7, Pole.Up, numberTextureButt);
                quads[index][1] = QuadColor(7, 9, 9, 16, 7, 9, 0, 7, Pole.Down, numberTextureButt);
                quads[index][2] = QuadColor(7, 9, 9, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                quads[index][3] = QuadColor(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
                quads[index][4] = QuadColor(7, 9, 9, 16, 9, 16, 0, 16, Pole.West, numberTextureSide);
                for (j = 0; j < 5; j++) quads[index][j].SetRotate(glm.pi90 + angle[i]);

                quads[index][5] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.Up, numberTextureButt);
                quads[index][6] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.Down, numberTextureButt);
                quads[index][7] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.North, numberTextureButt);
                quads[index][8] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                quads[index][9] = QuadColor(7, 9, 0, 16, 0, 16, 0, 16, Pole.East, numberTextureSide);
                quads[index][10] = QuadColor(7, 9, 0, 16, 0, 16, 0, 16, Pole.West, numberTextureSide);
                for (j = 5; j < 11; j++) quads[index][j].SetRotate(angle[i]);
            }
            // Все стороны
            quads[15] = new QuadSide[16];
            quads[15][0] = QuadColor(7, 9, 9, 16, 7, 9, 0, 7, Pole.Up, numberTextureButt);
            quads[15][1] = QuadColor(7, 9, 9, 16, 7, 9, 0, 7, Pole.Down, numberTextureButt);
            quads[15][2] = QuadColor(7, 9, 9, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
            quads[15][3] = QuadColor(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
            quads[15][4] = QuadColor(7, 9, 9, 16, 9, 16, 0, 16, Pole.West, numberTextureSide);
            for (j = 0; j < 5; j++) quads[15][j].SetRotate(glm.pi90);

            quads[15][5] = QuadColor(7, 9, 9, 16, 7, 9, 0, 7, Pole.Up, numberTextureButt);
            quads[15][6] = QuadColor(7, 9, 9, 16, 7, 9, 0, 7, Pole.Down, numberTextureButt);
            quads[15][7] = QuadColor(7, 9, 9, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
            quads[15][8] = QuadColor(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
            quads[15][9] = QuadColor(7, 9, 9, 16, 9, 16, 0, 16, Pole.West, numberTextureSide);
            for (j = 5; j < 10; j++) quads[15][j].SetRotate(glm.pi270);

            quads[15][10] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.Up, numberTextureButt);
            quads[15][11] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.Down, numberTextureButt);
            quads[15][12] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.North, numberTextureButt);
            quads[15][13] = QuadColor(7, 9, 0, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
            quads[15][14] = QuadColor(7, 9, 0, 16, 0, 16, 0, 16, Pole.East, numberTextureSide);
            quads[15][15] = QuadColor(7, 9, 0, 16, 0, 16, 0, 16, Pole.West, numberTextureSide);
        }
    }
}
