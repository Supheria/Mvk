using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект листвы
    /// </summary>
    public class BlockLeavesPalm : BlockUniLeaves
    {
        /***
         * Met
         * 0-7 сторона
         * 8-15 второй радиус вида стороны
         */

        public BlockLeavesPalm() : base (1048, EnumBlock.LogPalm, true) { }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            ushort met;
            if (side == Pole.East) met = 2;
            else if (side == Pole.West) met = 6;
            else if (side == Pole.North) met = 4;
            else met = 0;
            return state.NewMet(met);
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            if (met > 7) met -= 8;
            vec2i vec = MvkStatic.AreaOne8[met];
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.Offset(-vec.x, 0, -vec.y)).GetEBlock();
            return enumBlock == eBlockLog || enumBlock == EBlock;
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitQuads()
        {
            quads = new QuadSide[16][];
            float angle = glm.pi90;
            for (int i = 0; i < 8; i++)
            {
                quads[i] = new QuadSide[] {
                    new QuadSide(0).SetTexture(Particle, 0, 0, 32, 48, 2).SetSide(Pole.East, true, 8, 0, -8, 8, 48, 24).SetTranslate(.22f, 0, 0).SetRotate(angle + .4f, 0, .7f).Wind(),
                    new QuadSide(0).SetTexture(Particle, 0, 48, 32, 96, 2).SetSide(Pole.East, true, 8, 48, -8, 8, 96, 24).SetTranslate(1.945f, -.918f, 0).SetRotate(angle + .4f, 0, 1.5f).Wind(3),
                };
                quads[i + 8] = new QuadSide[] {
                    new QuadSide(0).SetTexture(Particle + 2, 0, 0, 32, 48, 2).SetSide(Pole.East, true, 8, -16, -8, 8, 32, 24).SetRotate(angle, 0, 1.5f).Wind(),
                    new QuadSide(0).SetTexture(Particle + 2, 0, 48, 32, 96, 2).SetSide(Pole.East, true, 8, 32, -8, 8, 80, 24).SetTranslate(1.26f, -.69f, 0).SetRotate(angle, 0, 2.5f).Wind(3),
                };
                angle += glm.pi45;
            }
        }
    }
}
