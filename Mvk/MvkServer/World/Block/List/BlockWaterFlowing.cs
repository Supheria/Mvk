using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок тикучей воды
    /// </summary>
    public class BlockWaterFlowing : BlockAbLiquidFlowing
    {
        /// <summary>
        /// Блок тикучей воды
        /// </summary>
        public BlockWaterFlowing() : base()
        {
            // Скорость высыхания
            dryingSpeed = 3;

            Translucent = true;
            BiomeColor = true;
            LightOpacity = 0;
            Material = EnumMaterial.Water;
            faces = new Face[]
            {
                new Face(63, true).SetAnimation(32, 2).SetBiomeColor(),
                new Face(62, true).SetAnimation(64, 1).SetBiomeColor()
            };
            InitBoxs();
        }

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[met];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 color = new vec3(0.24f, 0.45f, 0.88f);

            boxes = new Box[16][];

            for (int i = 0; i < 16; i++)
            {
                boxes[i] = new Box[] {
                    new Box()
                    {
                        From = new vec3(0),
                        To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[i + 1], MvkStatic.Xy[16]),
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, 63, true, color).SetAnimation(32, 2),
                            new Face(Pole.Down, 63, true, color).SetAnimation(32, 2)
                        }
                    },
                    new Box()
                    {
                        From = new vec3(0),
                        To = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[i + 1], MvkStatic.Xy[16]),
                        UVFrom = new vec2(0, MvkStatic.Uv[16 - i]),
                        UVTo = new vec2(MvkStatic.Uv[16], MvkStatic.Uv[16]),
                        Faces = new Face[]
                        {
                            new Face(Pole.East, 62, true, color).SetAnimation(64, 1),
                            new Face(Pole.North, 62, true, color).SetAnimation(64, 1),
                            new Face(Pole.South, 62, true, color).SetAnimation(64, 1),
                            new Face(Pole.West, 62, true, color).SetAnimation(64, 1)
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Проверка бесконечного источника
        /// </summary>
        protected override bool CheckEndlessSource(WorldBase world, BlockPos blockPos)
        {
            /*
              
            // Новый вариант
            if (blockPos.Y <= 47)
            {
                EnumBiome biome = world.GetBiome(blockPos);
                if (biome == EnumBiome.River || biome == EnumBiome.Sea
                    || biome == EnumBiome.Swamp || biome == EnumBiome.Beach)
                {
                    // Количество блоков стоячей жидкости, для бескончного источника
                    BlockState blockState;
                    BlockBase block;

                    for (int i = 0; i < 4; i++)
                    {
                        blockState = world.GetBlockState(blockPos.Offset(EnumFacing.GetHorizontal(i)));
                        if (blockState.GetBlock().Material == EnumMaterial.Water)
                        {
                            // Бесконечный источник
                            blockState = world.GetBlockState(blockPos.OffsetDown());
                            block = blockState.GetBlock();
                            if (block.FullBlock || block.EBlock == EnumBlock.Water)
                            {
                                world.SetBlockState(blockPos, new BlockState(eBlock), 14);
                                world.SetBlockTick(blockPos, tickRate);
                                return true;
                            }
                        }
                    }

                    blockState = world.GetBlockState(blockPos.OffsetDown());
                    block = blockState.GetBlock();
                    if (block.FullBlock || block.EBlock == EnumBlock.Water)
                    {
                        world.SetBlockState(blockPos, new BlockState(eBlock), 14);
                        world.SetBlockTick(blockPos, tickRate);
                        return true;
                    }
                }
            }
            */
            
            // Олд версия как в майне
            // Бесконечный источник только в реках и моря на высоте от 31 до 47 включительно
            if (blockPos.Y > 30 && blockPos.Y <= 47)
            {
                EnumBiome biome = world.GetBiome(blockPos);
                if (biome == EnumBiome.River || biome == EnumBiome.Sea 
                    || biome == EnumBiome.Swamp || biome == EnumBiome.Beach)
                {
                    // Количество блоков стоячей жидкости, для бескончного источника
                    int counteStagnantLiquid = 0;
                    BlockState blockState;

                    for (int i = 0; i < 4; i++)
                    {
                        blockState = world.GetBlockState(blockPos.Offset(EnumFacing.GetHorizontal(i)));
                        if (blockState.GetBlock().EBlock == eBlock) counteStagnantLiquid++;
                    }
                    if (counteStagnantLiquid >= 2)
                    {
                        // Бесконечный источник
                        blockState = world.GetBlockState(blockPos.OffsetDown());
                        BlockBase block = blockState.GetBlock();
                        if (block.FullBlock || block.EBlock == EnumBlock.Water)
                        {
                            world.SetBlockState(blockPos, new BlockState(eBlock), 14);
                            world.SetBlockTick(blockPos, tickRate);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
