using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект древесины
    /// </summary>
    public class BlockUniWood : BlockBase
    {
        /***
         * Met
         * 0 - вверх
         * 1/2 - бок
         */

        /// <summary>
        /// Номер текстуры торца
        /// </summary>
        protected readonly int numberTextureButt;
        /// <summary>
        /// Номер текстуры стороны
        /// </summary>
        protected readonly int numberTextureSide;
        /// <summary>
        /// Параметр метданных вверх когда ставит игрок для бревна 3 для досок 0
        /// </summary>
        protected int metUp = 0;

        public BlockUniWood(int numberTextureButt, int numberTextureSide)
        {
            this.numberTextureButt = numberTextureButt;
            this.numberTextureSide = numberTextureSide;
            Combustibility = true;
            IgniteOddsSunbathing = 10;
            BurnOdds = 20;
            Material = EnumMaterial.Wood;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
            Particle = numberTextureButt;
            Resistance = 5.0f;
            InitBoxs();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 20;

        /// <summary>
        /// Стороны целого блока для рендера 0 - 3 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met];

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            int met = metUp;
            if (side == Pole.East || side == Pole.West) met = 1 + metUp;
            else if (side == Pole.South || side == Pole.North) met = 2 + metUp;
            return state.NewMet((ushort)met);
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected virtual void InitBoxs()
        {
            quads = new QuadSide[][]
            {
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.South)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.South)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.Up),
                    new QuadSide(0).SetTexture(numberTextureSide).SetSide(Pole.Down),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.East),
                    new QuadSide(0).SetTexture(numberTextureSide, 1).SetSide(Pole.West),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.North),
                    new QuadSide(0).SetTexture(numberTextureButt).SetSide(Pole.South)
                }
            };
        }
    }
}
