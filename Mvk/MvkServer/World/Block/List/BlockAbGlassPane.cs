using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект стекляных панелей
    /// </summary>
    public abstract class BlockAbGlassPane : BlockBase
    {
        /// <summary>
        /// Номер текстуры торца
        /// </summary>
        private readonly int numberTextureButt;
        /// <summary>
        /// Номер текстуры стороны
        /// </summary>
        private readonly int numberTextureSide;

        public BlockAbGlassPane(int numberTextureSide, int numberTextureButt, vec3 color)
        {
            this.numberTextureButt = numberTextureButt;
            this.numberTextureSide = numberTextureSide;
            FullBlock = false;
            Translucent = true;
            АmbientOcclusion = false;
            Shadow = false;
            Color = color;
            AllSideForcibly = true;
            NoSideDimming = true;
            UseNeighborBrightness = true;
            Particle = numberTextureSide;
            LightOpacity = 2;
            Material = EnumMaterial.GlassPane;
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
            InitBoxs();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 10;

        /// <summary>
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public override bool BlocksNotSame(int met) => true;

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];
        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public override Box[] GetBoxesGui() => boxes[10];

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

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
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            if (base.Put(worldIn, blockPos, state, side, facing))
            {
                MetUpdate(worldIn, blockPos, state);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock) 
            => MetUpdate(worldIn, blockPos, state);

        private void MetUpdate(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            int met = state.Met();
            int metNew = 0;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetSouth())) metNew |= 1;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetEast())) metNew |= 2;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetNorth())) metNew |= 4;
            if (DoesBlockHaveSolidTopSurface(worldIn, blockPos.OffsetWest())) metNew |= 8;
            if (metNew != met)
            {
                worldIn.SetBlockStateMet(blockPos, metNew);
            }
        }

        /// <summary>
        /// Блок имеет твердую непрозрачную поверхность и стекло
        /// </summary>
        private bool DoesBlockHaveSolidTopSurface(WorldBase worldIn, BlockPos blockPos)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return (block.IsNotTransparent() && block.FullBlock) || block.Material == EnumMaterial.Glass || block.Material == EnumMaterial.GlassPane;
        }

        private Box B(int x1, int x2, int z1, int z2, int u1, int u2, int v1, int v2, Pole p1, Pole p2, int t)
        {
            return new Box()
            {
                From = new vec3(MvkStatic.Xy[x1], MvkStatic.Xy[0], MvkStatic.Xy[z1]),
                To = new vec3(MvkStatic.Xy[x2], MvkStatic.Xy[16], MvkStatic.Xy[z2]),
                UVFrom = new vec2(MvkStatic.Uv[u1], MvkStatic.Uv[v1]),
                UVTo = new vec2(MvkStatic.Uv[u2], MvkStatic.Uv[v2]),
                Faces = new Face[] { new Face(p1, t, true, Color), new Face(p2, t, true, Color), }
            };
        }

        private Box B(int x1, int x2, int z1, int z2, int u1, int u2, int v1, int v2, Pole p, int t)
        {
            return new Box()
            {
                From = new vec3(MvkStatic.Xy[x1], MvkStatic.Xy[0], MvkStatic.Xy[z1]),
                To = new vec3(MvkStatic.Xy[x2], MvkStatic.Xy[16], MvkStatic.Xy[z2]),
                UVFrom = new vec2(MvkStatic.Uv[u1], MvkStatic.Uv[v1]),
                UVTo = new vec2(MvkStatic.Uv[u2], MvkStatic.Uv[v2]),
                Faces = new Face[] { new Face(p, t, true, Color) }
            };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            int i, index;
            int[] ar;
            float[] angle;
            boxes = new Box[16][];
            // Нет сторон
            boxes[0] = new Box[3];
            boxes[0][0] = B(7, 9, 7, 9, 7, 9, 7, 9, Pole.Up, Pole.Down, numberTextureButt);
            boxes[0][1] = B(7, 9, 7, 9, 7, 9, 0, 16, Pole.North, Pole.South, numberTextureSide);
            boxes[0][2] = B(7, 9, 7, 9, 7, 9, 0, 16, Pole.East, Pole.West, numberTextureSide);

            // одна сторона
            ar = new int[] { 1, 2, 4, 8 };
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };
            for (i = 0; i < 4; i++)
            {
                index = ar[i];
                boxes[index] = new Box[4];
                boxes[index][0] = B(7, 9, 7, 16, 7, 9, 0, 9, Pole.Up, Pole.Down, numberTextureButt);
                boxes[index][0].RotateYaw = angle[i];
                boxes[index][1] = B(7, 9, 7, 16, 7, 9, 0, 16, Pole.North, Pole.South, numberTextureButt);
                boxes[index][1].RotateYaw = angle[i];
                boxes[index][2] = B(7, 9, 7, 16, 0, 9, 0, 16, Pole.East, numberTextureSide);
                boxes[index][2].RotateYaw = angle[i];
                boxes[index][3] = B(7, 9, 7, 16, 7, 16, 0, 16, Pole.West, numberTextureSide);
                boxes[index][3].RotateYaw = angle[i];
            }
            // угол 2 стороны
            ar = new int[] { 3, 6, 12, 9 };
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };

            for (i = 0; i < 4; i++)
            {
                index = ar[i];
                boxes[index] = new Box[8];
                boxes[index][0] = B(7, 9, 7, 16, 7, 9, 0, 9, Pole.Up, Pole.Down, numberTextureButt);
                boxes[index][0].RotateYaw = angle[i];
                boxes[index][1] = B(7, 9, 9, 16, 7, 9, 0, 9, Pole.Up, Pole.Down, numberTextureButt);
                boxes[index][1].RotateYaw = glm.pi90 + angle[i];
                boxes[index][2] = B(7, 9, 7, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                boxes[index][2].RotateYaw = angle[i];
                boxes[index][3] = B(7, 9, 7, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                boxes[index][3].RotateYaw = glm.pi90 + angle[i];
                boxes[index][4] = B(7, 9, 7, 16, 7, 16, 0, 16, Pole.West, numberTextureSide);
                boxes[index][4].RotateYaw = angle[i];
                boxes[index][5] = B(7, 16, 7, 16, 0, 9, 0, 16, Pole.North, numberTextureSide);
                boxes[index][5].RotateYaw = angle[i];
                boxes[index][6] = B(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
                boxes[index][6].RotateYaw = angle[i];
                boxes[index][7] = B(9, 16, 7, 9, 9, 16, 0, 16, Pole.South, numberTextureSide);
                boxes[index][7].RotateYaw = angle[i];
            }
            // прямые 2 стороны
            ar = new int[] { 5, 10 };
            angle = new float[] { 0, glm.pi90 };
            for (i = 0; i < 2; i++)
            {
                index = ar[i];
                boxes[index] = new Box[3];
                boxes[index][0] = B(7, 9, 0, 16, 7, 9, 0, 16, Pole.Up, Pole.Down, numberTextureButt);
                boxes[index][0].RotateYaw = angle[i];
                boxes[index][1] = B(7, 9, 0, 16, 7, 9, 0, 16, Pole.North, Pole.South, numberTextureButt);
                boxes[index][1].RotateYaw = angle[i];
                boxes[index][2] = B(7, 9, 0, 16, 0, 16, 0, 16, Pole.East, Pole.West, numberTextureSide);
                boxes[index][2].RotateYaw = angle[i];
            }
            // три стороны
            ar = new int[] { 7, 14, 13, 11 };
            angle = new float[] { 0, glm.pi90, glm.pi, glm.pi270 };
            for (i = 0; i < 4; i++)
            {
                index = ar[i];
                boxes[index] = new Box[7];
                boxes[index][0] = B(7, 9, 9, 16, 7, 9, 0, 7, Pole.Up, Pole.Down, numberTextureButt);
                boxes[index][0].RotateYaw = glm.pi90 + angle[i];
                boxes[index][1] = B(7, 9, 9, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
                boxes[index][1].RotateYaw = glm.pi90 + angle[i];
                boxes[index][2] = B(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
                boxes[index][2].RotateYaw = glm.pi90 + angle[i];
                boxes[index][3] = B(7, 9, 9, 16, 9, 16, 0, 16, Pole.West, numberTextureSide);
                boxes[index][3].RotateYaw = glm.pi90 + angle[i];
                boxes[index][4] = B(7, 9, 0, 16, 7, 9, 0, 16, Pole.Up, Pole.Down, numberTextureButt);
                boxes[index][4].RotateYaw = angle[i];
                boxes[index][5] = B(7, 9, 0, 16, 7, 9, 0, 16, Pole.North, Pole.South, numberTextureButt);
                boxes[index][5].RotateYaw = angle[i];
                boxes[index][6] = B(7, 9, 0, 16, 0, 16, 0, 16, Pole.East, Pole.West, numberTextureSide);
                boxes[index][6].RotateYaw = angle[i];
            }
            // Все стороны
            boxes[15] = new Box[11];
            boxes[15][0] = B(7, 9, 9, 16, 7, 9, 0, 7, Pole.Up, Pole.Down, numberTextureButt);
            boxes[15][0].RotateYaw = glm.pi90;
            boxes[15][1] = B(7, 9, 9, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
            boxes[15][1].RotateYaw = glm.pi90;
            boxes[15][2] = B(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
            boxes[15][2].RotateYaw = glm.pi90;
            boxes[15][3] = B(7, 9, 9, 16, 9, 16, 0, 16, Pole.West, numberTextureSide);
            boxes[15][3].RotateYaw = glm.pi90;
            boxes[15][4] = B(7, 9, 9, 16, 7, 9, 0, 7, Pole.Up, Pole.Down, numberTextureButt);
            boxes[15][4].RotateYaw = glm.pi270;
            boxes[15][5] = B(7, 9, 9, 16, 7, 9, 0, 16, Pole.South, numberTextureButt);
            boxes[15][5].RotateYaw = glm.pi270;
            boxes[15][6] = B(7, 9, 9, 16, 0, 7, 0, 16, Pole.East, numberTextureSide);
            boxes[15][6].RotateYaw = glm.pi270;
            boxes[15][7] = B(7, 9, 9, 16, 9, 16, 0, 16, Pole.West, numberTextureSide);
            boxes[15][7].RotateYaw = glm.pi270;
            boxes[15][8] = B(7, 9, 0, 16, 7, 9, 0, 16, Pole.Up, Pole.Down, numberTextureButt);
            boxes[15][9] = B(7, 9, 0, 16, 7, 9, 0, 16, Pole.North, Pole.South, numberTextureButt);
            boxes[15][10] = B(7, 9, 0, 16, 0, 16, 0, 16, Pole.East, Pole.West, numberTextureSide);

        }
    }
}
