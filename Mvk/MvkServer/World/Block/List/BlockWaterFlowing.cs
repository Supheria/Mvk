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
            Material = Materials.GetMaterialCache(EnumMaterial.Water);
            sideLiquids = new SideLiquid[]
            {
                new SideLiquid(0, 63, 3, 32, 2),
                new SideLiquid(1, 63, 3, 32, 2),
                new SideLiquid(2, 62, 3, 64, 1),
                new SideLiquid(3, 62, 3, 64, 1),
                new SideLiquid(4, 62, 3, 64, 1),
                new SideLiquid(5, 62, 3, 64, 1)
            };
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
