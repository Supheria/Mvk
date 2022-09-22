using MvkServer.World.Block.List;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Перечень блоков
    /// </summary>
    public class Blocks
    {
        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// LightOpacity << 4 | LightValue
        /// LightOpacity = blocksLightOpacity[0] >> 4
        /// LightValue = blocksLightOpacity[0] & 0xF
        /// </summary>
        public static byte[] blocksLightOpacity;
        /// <summary>
        /// Массив всех кэш блоков
        /// </summary>
        public static BlockBase[] blocksInt;
        /// <summary>
        /// Массив нужности случайного тика для блока
        /// </summary>
        public static bool[] blocksRandomTick;
        /// <summary>
        /// Массив с дополнительными metdata свыше 4 бита
        /// </summary>
        public static bool[] blocksAddMet;

        private static BlockBase ToBlock(EnumBlock eBlock)
        {
            switch (eBlock)
            {
                case EnumBlock.Air: return new BlockAir();
                case EnumBlock.Debug: return new BlockDebug();
                case EnumBlock.Bedrock: return new BlockBedrock();
                case EnumBlock.Stone: return new BlockStone();
                case EnumBlock.Cobblestone: return new BlockCobblestone();
                case EnumBlock.Limestone: return new BlockLimestone();
                case EnumBlock.Granite: return new BlockGranite();
                case EnumBlock.Sandstone: return new BlockSandstone();
                case EnumBlock.Dirt: return new BlockDirt();
                case EnumBlock.Turf: return new BlockTurf();
                case EnumBlock.Sand: return new BlockSand();
                case EnumBlock.Gravel: return new BlockGravel();
                case EnumBlock.Clay: return new BlockClay();
                case EnumBlock.Water: return new BlockWater();
                case EnumBlock.Lava: return new BlockLava();
                case EnumBlock.Oil: return new BlockOil();
                case EnumBlock.Fire: return new BlockFire();
                case EnumBlock.LogOak: return new BlockLogOak();
                case EnumBlock.LogBirch: return new BlockLogBirch();
                case EnumBlock.LogSpruce: return new BlockLogSpruce();
                case EnumBlock.LogFruit: return new BlockLogFruit();
                case EnumBlock.LogPalm: return new BlockLogPalm();
                case EnumBlock.PlanksOak: return new BlockPlanksOak();
                case EnumBlock.PlanksBirch: return new BlockPlanksBirch();
                case EnumBlock.PlanksSpruce: return new BlockPlanksSpruce();
                case EnumBlock.PlanksFruit: return new BlockPlanksFruit();
                case EnumBlock.PlanksPalm: return new BlockPlanksPalm();
                case EnumBlock.LeavesOak: return new BlockLeavesOak();
                case EnumBlock.LeavesBirch: return new BlockLeavesBirch();
                case EnumBlock.LeavesSpruce: return new BlockLeavesSpruce();
                case EnumBlock.LeavesFruit: return new BlockLeavesFruit();
                case EnumBlock.LeavesPalm: return new BlockLeavesPalm();
                case EnumBlock.SaplingOak: return new BlockSaplingOak();
                case EnumBlock.SaplingBirch: return new BlockSaplingBirch();
                case EnumBlock.SaplingSpruce: return new BlockSaplingSpruce();
                case EnumBlock.SaplingFruit: return new BlockSaplingFruit();
                case EnumBlock.SaplingPalm: return new BlockSaplingPalm();
                case EnumBlock.Cactus: return new BlockCactus();
                case EnumBlock.TallGrass: return new BlockTallGrass();
                case EnumBlock.FlowerDandelion: return new BlockFlowerDandelion();
                case EnumBlock.FlowerClover: return new BlockFlowerClover();
                case EnumBlock.OreCoal: return new BlockOreCoal();
                case EnumBlock.OreIron: return new BlockOreIron();
                case EnumBlock.OreGold: return new BlockOreGold();
                case EnumBlock.Brol: return new BlockBrol();
                case EnumBlock.Glass: return new BlockGlass();
                case EnumBlock.GlassWhite: return new BlockGlassWhite();
                case EnumBlock.GlassRed: return new BlockGlassRed();
                case EnumBlock.GlassPane: return new BlockGlassPane();
                case EnumBlock.GlassPaneWhite: return new BlockGlassPaneWhite();
                case EnumBlock.GlassPaneRed: return new BlockGlassPaneRed();
            }

            return new BlockAir(true);
        }

        /// <summary>
        /// Инициализировать все блоки для кэша
        /// </summary>
        public static void Initialized()
        {
            int count = BlocksCount.COUNT + 1;
            blocksInt = new BlockBase[count];
            blocksLightOpacity = new byte[count];
            blocksRandomTick = new bool[count];
            blocksAddMet = new bool[count];

            for (int i = 0; i < count; i++)
            {
                EnumBlock enumBlock = (EnumBlock)i;
                BlockBase block = ToBlock(enumBlock);
                block.SetEnumBlock(enumBlock);
                blocksInt[i] = block;
                blocksLightOpacity[i] = (byte)(block.LightOpacity << 4 | block.LightValue);
                blocksRandomTick[i] = block.NeedsRandomTick;
                blocksAddMet[i] = block.IsAddMet;
            }
        }

        /// <summary>
        /// Получить объект блока с кеша, для получения информационных данных
        /// </summary>
        public static BlockBase GetBlockCache(EnumBlock eBlock) => blocksInt[(int)eBlock];
        /// <summary>
        /// Получить объект блока с кеша, для получения информационных данных
        /// </summary>
        public static BlockBase GetBlockCache(int index) => blocksInt[index];
    }
}
