using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект древесины
    /// </summary>
    public abstract class BlockAbWood : BlockBase
    {
        /// <summary>
        /// Цвет торца
        /// </summary>
        private readonly vec3 colorButt;
        /// <summary>
        /// Цвет стороны
        /// </summary>
        private readonly vec3 colorSide;
        /// <summary>
        /// Номер текстуры торца
        /// </summary>
        private readonly int numberTextureButt;
        /// <summary>
        /// Номер текстуры стороны
        /// </summary>
        private readonly int numberTextureSide;
        /// <summary>
        /// Параметр метданных вверх когда ставит игрок для бревна 3 для досок 0
        /// </summary>
        protected int metUp = 0;

        public BlockAbWood(int numberTextureButt, int numberTextureSide, vec3 colorButt, vec3 colorSide)
        {
            this.colorButt = colorButt;
            this.colorSide = colorSide;
            this.numberTextureButt = numberTextureButt;
            this.numberTextureSide = numberTextureSide;
            Material = EnumMaterial.Wood;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
            Particle = numberTextureButt;
            InitBoxs();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 20;

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

        /// <summary>
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            int met = metUp;
            if (side == Pole.East || side == Pole.West) met = 1 + metUp;
            else if (side == Pole.South || side == Pole.North) met = 2 + metUp;

            return base.Put(worldIn, blockPos, new BlockState(state.Id(), met, state.lightBlock, state.lightSky), side, facing);
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[][] {
                new Box[] {
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, numberTextureButt, false, colorButt),
                            new Face(Pole.Down, numberTextureButt, false, colorButt),
                            new Face(Pole.East, numberTextureSide, false, colorSide),
                            new Face(Pole.North, numberTextureSide, false, colorSide),
                            new Face(Pole.South, numberTextureSide, false, colorSide),
                            new Face(Pole.West, numberTextureSide, false, colorSide)
                        }
                    }
                },
                new Box[] {
                    new Box()
                    {
                        RotateYawUV = 1,
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, numberTextureSide, false, colorSide),
                            new Face(Pole.Down, numberTextureSide, false, colorSide),
                            new Face(Pole.East, numberTextureButt, false, colorButt),
                            new Face(Pole.North, numberTextureSide, false, colorSide),
                            new Face(Pole.South, numberTextureSide, false, colorSide),
                            new Face(Pole.West, numberTextureButt, false, colorButt)
                        }
                    }
                },
                new Box[] {
                    new Box()
                    {
                        RotateYawUV = 1,
                        Faces = new Face[]
                        {
                            new Face(Pole.East, numberTextureSide, false, colorSide),
                            new Face(Pole.North, numberTextureButt, false, colorButt),
                            new Face(Pole.South, numberTextureButt, false, colorButt),
                            new Face(Pole.West, numberTextureSide, false, colorSide)
                        }
                    },
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, numberTextureSide, false, colorSide),
                            new Face(Pole.Down, numberTextureSide, false, colorSide),
                        }
                    }
                }
            };
        }
    }
}
