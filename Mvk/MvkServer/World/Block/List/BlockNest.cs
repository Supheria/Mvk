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
            Color = new vec3(.95f, .91f, .73f);
            FullBlock = false;
            AllSideForcibly = true;
            АmbientOcclusion = false;
            UseNeighborBrightness = true;
            Material = EnumMaterial.Interior;
            Shadow = false;
            LightOpacity = 0;
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
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public override bool BlocksNotSame(int met) => true;

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
                int met = state.met - 1;
                worldIn.SetBlockStateMet(pos, (ushort)met, true);
                // Добавляем предмет яйца
                ItemBase item = Items.GetItemCache(EnumItem.Egg);
                ItemStack itemStack = new ItemStack(item);
                // Пробуем взять в инвентарь
                if (!entityPlayer.Inventory.AddItemStackToInventory(itemStack))
                {
                    // Если не смогли взять, дропаем его
                    if (!worldIn.IsRemote)
                    {
                        // Дроп
                        entityPlayer.DropItem(itemStack, true);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

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
            boxes = new Box[5][];

            boxes[0] = new Box[] {
                new Box()
                {
                    From = new vec3(0),
                    To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[6], MvkStatic.Xy[16]),
                    UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[10]),
                    UVTo = new vec2(MvkStatic.Uv[16], MvkStatic.Uv[16]),
                    Faces = new Face[]
                    {
                        new Face(Pole.East, 960, Color),
                        new Face(Pole.North, 960, Color),
                        new Face(Pole.South, 960, Color),
                        new Face(Pole.West, 960, Color)
                    }
                },
                new Box()
                {
                    From = new vec3(0),
                    To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[6], MvkStatic.Xy[16]),
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 961, Color),
                        new Face(Pole.Down, 960, Color)
                    }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[3], MvkStatic.Xy[4], MvkStatic.Xy[3]),
                    To = new vec3(MvkStatic.Xy[13], MvkStatic.Xy[4], MvkStatic.Xy[13]),
                    UVFrom = new vec2(MvkStatic.Uv[3], MvkStatic.Uv[3]),
                    UVTo = new vec2(MvkStatic.Uv[13], MvkStatic.Uv[13]),
                    Faces = new Face[] { new Face(Pole.Up, 960, Color) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[13], MvkStatic.Xy[4], MvkStatic.Xy[3]),
                    To = new vec3(MvkStatic.Xy[13], MvkStatic.Xy[6], MvkStatic.Xy[13]),
                    UVFrom = new vec2(MvkStatic.Uv[3], MvkStatic.Uv[10]),
                    UVTo = new vec2(MvkStatic.Uv[13], MvkStatic.Uv[12]),
                    Faces = new Face[] { new Face(Pole.West, 960, Color) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[3], MvkStatic.Xy[4], MvkStatic.Xy[3]),
                    To = new vec3(MvkStatic.Xy[3], MvkStatic.Xy[6], MvkStatic.Xy[13]),
                    UVFrom = new vec2(MvkStatic.Uv[3], MvkStatic.Uv[10]),
                    UVTo = new vec2(MvkStatic.Uv[13], MvkStatic.Uv[12]),
                    Faces = new Face[] { new Face(Pole.East, 960, Color) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[3], MvkStatic.Xy[4], MvkStatic.Xy[3]),
                    To = new vec3(MvkStatic.Xy[13], MvkStatic.Xy[6], MvkStatic.Xy[3]),
                    UVFrom = new vec2(MvkStatic.Uv[3], MvkStatic.Uv[10]),
                    UVTo = new vec2(MvkStatic.Uv[13], MvkStatic.Uv[12]),
                    Faces = new Face[] { new Face(Pole.South, 960, Color)}
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[3], MvkStatic.Xy[4], MvkStatic.Xy[13]),
                    To = new vec3(MvkStatic.Xy[13], MvkStatic.Xy[6], MvkStatic.Xy[13]),
                    UVFrom = new vec2(MvkStatic.Uv[3], MvkStatic.Uv[10]),
                    UVTo = new vec2(MvkStatic.Uv[13], MvkStatic.Uv[12]),
                    Faces = new Face[] { new Face(Pole.North, 960, Color)}
                }
            };

            boxes[1] = new Box[boxes[0].Length + 1];
            for (int i = 0; i < boxes[0].Length; i++)
            {
                boxes[1][i] = boxes[0][i];
            }
            boxes[1][boxes[0].Length] = new Box()
            {
                From = new vec3(MvkStatic.Xy[6], MvkStatic.Xy[4], MvkStatic.Xy[5]),
                To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[6], MvkStatic.Xy[7]),
                UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[0]),
                UVTo = new vec2(MvkStatic.Uv[2], MvkStatic.Uv[2]),
                Faces = new Face[]
                    {
                        new Face(Pole.Up, 962, Color),
                        new Face(Pole.East, 962, Color),
                        new Face(Pole.North, 962, Color),
                        new Face(Pole.South, 962, Color),
                        new Face(Pole.West, 962, Color)
                    }
            };

            boxes[2] = new Box[boxes[1].Length + 1];
            for (int i = 0; i < boxes[1].Length; i++)
            {
                boxes[2][i] = boxes[1][i];
            }
            boxes[2][boxes[1].Length] = new Box()
            {
                From = new vec3(MvkStatic.Xy[9], MvkStatic.Xy[3], MvkStatic.Xy[4]),
                To = new vec3(MvkStatic.Xy[11], MvkStatic.Xy[5], MvkStatic.Xy[6]),
                UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[0]),
                UVTo = new vec2(MvkStatic.Uv[2], MvkStatic.Uv[2]),
                Faces = new Face[]
                    {
                        new Face(Pole.Up, 962, Color),
                        new Face(Pole.East, 962, Color),
                        new Face(Pole.North, 962, Color),
                        new Face(Pole.South, 962, Color),
                        new Face(Pole.West, 962, Color)
                    }
            };

            boxes[3] = new Box[boxes[2].Length + 1];
            for (int i = 0; i < boxes[2].Length; i++)
            {
                boxes[3][i] = boxes[2][i];
            }
            boxes[3][boxes[2].Length] = new Box()
            {
                From = new vec3(MvkStatic.Xy[4], MvkStatic.Xy[3], MvkStatic.Xy[8]),
                To = new vec3(MvkStatic.Xy[6], MvkStatic.Xy[5], MvkStatic.Xy[10]),
                UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[0]),
                UVTo = new vec2(MvkStatic.Uv[2], MvkStatic.Uv[2]),
                Faces = new Face[]
                    {
                        new Face(Pole.Up, 962, Color),
                        new Face(Pole.East, 962, Color),
                        new Face(Pole.North, 962, Color),
                        new Face(Pole.South, 962, Color),
                        new Face(Pole.West, 962, Color)
                    }
            };

            boxes[4] = new Box[boxes[3].Length + 1];
            for (int i = 0; i < boxes[3].Length; i++)
            {
                boxes[4][i] = boxes[3][i];
            }
            boxes[4][boxes[3].Length] = new Box()
            {
                From = new vec3(MvkStatic.Xy[10], MvkStatic.Xy[3], MvkStatic.Xy[8]),
                To = new vec3(MvkStatic.Xy[12], MvkStatic.Xy[5], MvkStatic.Xy[10]),
                UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[0]),
                UVTo = new vec2(MvkStatic.Uv[2], MvkStatic.Uv[2]),
                Faces = new Face[]
                    {
                        new Face(Pole.Up, 962, Color),
                        new Face(Pole.East, 962, Color),
                        new Face(Pole.North, 962, Color),
                        new Face(Pole.South, 962, Color),
                        new Face(Pole.West, 962, Color)
                    }
            };
        }
    }
}
