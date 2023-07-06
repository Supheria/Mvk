using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /***
     * Met
     * 0 - Нет яиц
     * 1 - 4 количество яиц
     */

    /// <summary>
    /// Блок гнезда
    /// </summary>
    public class BlockNest : BlockBase
    {
        /// <summary>
        /// Блок Песчаник
        /// </summary>
        public BlockNest()
        {
            SetUnique();
            Material = EnumMaterial.Interior;
            Particle = 960;
            Combustibility = true;
            IgniteOddsSunbathing = 30;
            BurnOdds = 60;
            Resistance = .2f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitBoxs();
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
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;
        /// <summary>
        /// Является ли блок проходимым на нём, т.е. можно ли ходить по нему
        /// </summary>
       // public override bool IsPassableOnIt(int met) => true;

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            return state.NewMet(0);
        }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            if (state.met > 0)
            {
                if (!worldIn.IsRemote)
                {
                    int met = state.met - 1;
                    worldIn.SetBlockStateMet(pos, (ushort)met, true);
                    // Берём яйцо
                    entityPlayer.Inventory.AddItemStackToInventory(worldIn, entityPlayer,
                        new ItemStack(Items.GetItemCache(EnumItem.Egg)));
                    // Чпок
                    worldIn.PlaySoundPop(pos.ToVec3() + .5f);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met];

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            float posY1 = pos.Y + .25f;
            float posY2 = pos.Y + .375f;
            return new AxisAlignedBB[] {
                new AxisAlignedBB(new vec3(pos.X, pos.Y, pos.Z), new vec3(pos.X + 1, posY1, pos.Z + 1)),
                new AxisAlignedBB(new vec3(pos.X, posY1, pos.Z), new vec3(pos.X + 1, posY2, pos.Z + .125f)),
                new AxisAlignedBB(new vec3(pos.X, posY1, pos.Z + .875f), new vec3(pos.X + 1, posY2, pos.Z + 1)),
                new AxisAlignedBB(new vec3(pos.X, posY1, pos.Z + .125f), new vec3(pos.X + .125f, posY2, pos.Z + .875f)),
                new AxisAlignedBB(new vec3(pos.X + .875f, posY1, pos.Z + .125f), new vec3(pos.X + 1, posY2, pos.Z + .875f)),
            };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        private void InitBoxs()
        {
            quads = new QuadSide[5][];

            quads[0] = new QuadSide[]
            {
                new QuadSide(0).SetTexture(960).SetSide(Pole.Down),
                new QuadSide(0).SetTexture(960, 0, 10, 16, 16).SetSide(Pole.East, false, 0, 0, 0, 16, 6, 16),
                new QuadSide(0).SetTexture(960, 0, 10, 16, 16).SetSide(Pole.North, false, 0, 0, 0, 16, 6, 16),
                new QuadSide(0).SetTexture(960, 0, 10, 16, 16).SetSide(Pole.South, false, 0, 0, 0, 16, 6, 16),
                new QuadSide(0).SetTexture(960, 0, 10, 16, 16).SetSide(Pole.West, false, 0, 0, 0, 16, 6, 16),

                new QuadSide(0).SetTexture(960, 3, 3, 13, 13).SetSide(Pole.Up, false, 3, 4, 3, 13, 4, 13),
                new QuadSide(0).SetTexture(960, 3, 10, 13, 12).SetSide(Pole.West, false, 13, 4, 3, 13, 6, 13),
                new QuadSide(0).SetTexture(960, 3, 10, 13, 12).SetSide(Pole.East, false, 3, 4, 3, 3, 6, 13),
                new QuadSide(0).SetTexture(960, 3, 10, 13, 12).SetSide(Pole.South, false, 3, 4, 3, 13, 6, 3),
                new QuadSide(0).SetTexture(960, 3, 10, 13, 12).SetSide(Pole.North, false, 3, 4, 13, 13, 6, 13),

                new QuadSide(0).SetTexture(961).SetSide(Pole.Up, false, 0, 0, 0, 16, 6, 16)
            };

            int i;
            quads[1] = new QuadSide[quads[0].Length + 5];
            for (i = 0; i < quads[0].Length; i++)
            {
                quads[1][i] = quads[0][i];
            }
            quads[1][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 2).SetSide(Pole.Up, false, 6, 4, 5, 8, 6, 7);
            quads[1][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 2).SetSide(Pole.East, false, 6, 4, 5, 8, 6, 7);
            quads[1][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 2).SetSide(Pole.North, false, 6, 4, 5, 8, 6, 7);
            quads[1][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 2).SetSide(Pole.South, false, 6, 4, 5, 8, 6, 7);
            quads[1][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 2).SetSide(Pole.West, false, 6, 4, 5, 8, 6, 7);

            quads[2] = new QuadSide[quads[1].Length + 5];
            for (i = 0; i < quads[1].Length; i++)
            {
                quads[2][i] = quads[1][i];
            }
            quads[2][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 2).SetSide(Pole.Up, false, 9, 4, 4, 12, 5, 6);
            quads[2][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 1).SetSide(Pole.East, false, 9, 4, 4, 12, 5, 6);
            quads[2][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 1).SetSide(Pole.North, false, 9, 4, 4, 12, 5, 6);
            quads[2][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 1).SetSide(Pole.South, false, 9, 4, 4, 12, 5, 6);
            quads[2][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 1).SetSide(Pole.West, false, 9, 4, 4, 12, 5, 6);

            quads[3] = new QuadSide[quads[2].Length + 5];
            for (i = 0; i < quads[2].Length; i++)
            {
                quads[3][i] = quads[2][i];
            }
            quads[3][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 2).SetSide(Pole.Up, false, 4, 4, 8, 7, 5, 10);
            quads[3][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 1).SetSide(Pole.East, false, 4, 4, 8, 7, 5, 10);
            quads[3][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 1).SetSide(Pole.North, false, 4, 4, 8, 7, 5, 10);
            quads[3][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 1).SetSide(Pole.South, false, 4, 4, 8, 7, 5, 10);
            quads[3][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 1).SetSide(Pole.West, false, 4, 4, 8, 7, 5, 10);

            quads[4] = new QuadSide[quads[3].Length + 5];
            for (i = 0; i < quads[3].Length; i++)
            {
                quads[4][i] = quads[3][i];
            }
            quads[4][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 3).SetSide(Pole.Up, false, 9, 4, 8, 11, 5, 11);
            quads[4][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 1).SetSide(Pole.East, false, 9, 4, 8, 11, 5, 11);
            quads[4][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 1).SetSide(Pole.North, false, 9, 4, 8, 11, 5, 11);
            quads[4][i++] = new QuadSide(0).SetTexture(962, 0, 0, 2, 1).SetSide(Pole.South, false, 9, 4, 8, 11, 5, 11);
            quads[4][i++] = new QuadSide(0).SetTexture(962, 0, 0, 3, 1).SetSide(Pole.West, false, 9, 4, 8, 11, 5, 11);
        }
    }
}
