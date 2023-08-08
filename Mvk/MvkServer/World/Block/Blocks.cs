using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.World.Block.List;
using MvkServer.World.Gen.Feature;

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
                case EnumBlock.Stone: return new BlockUniSolid(2, EnumItem.PieceStone);
                case EnumBlock.Cobblestone: return new BlockUniSolid(3, EnumItem.PieceStone);
                case EnumBlock.Limestone: return new BlockUniSolid(4, EnumItem.PieceLimestone, 20);
                case EnumBlock.Granite: return new BlockUniSolid(5, EnumItem.PieceGranite);
                case EnumBlock.Sandstone: return new BlockSandstone();
                case EnumBlock.Dirt: return new BlockUniLoose(64, EnumItem.PieceDirt);
                case EnumBlock.Turf: return new BlockTurf();
                case EnumBlock.Sand: return new BlockSand();
                case EnumBlock.Gravel: return new BlockGravel();
                case EnumBlock.Clay: return new BlockUniLoose(70, EnumItem.PieceClay);
                case EnumBlock.Water: return new BlockWater();
                case EnumBlock.WaterFlowing: return new BlockWaterFlowing();
                case EnumBlock.Lava: return new BlockLava();
                case EnumBlock.LavaFlowing: return new BlockLavaFlowing();
                case EnumBlock.Oil: return new BlockOil();
                case EnumBlock.OilFlowing: return new BlockOilFlowing();
                case EnumBlock.Fire: return new BlockFire();
                case EnumBlock.LogOak: return new BlockUniLog(129, 128, new WorldGenTreeOak2(), 8);
                case EnumBlock.LogBirch: return new BlockUniLog(136, 135, new WorldGenTreeBirch2(), 3);
                case EnumBlock.LogSpruce: return new BlockUniLog(143, 142, null, 5);
                case EnumBlock.LogFruit: return new BlockUniLog(150, 149, new WorldGenTreeFruit2(), 6);
                case EnumBlock.LogPalm: return new BlockLogPalm();
                case EnumBlock.PlanksOak: return new BlockUniWood(131, 130).SetCraftRecipe(2, 5, new Element(EnumBlock.LogOak)).SetCraftTools(EnumItem.AxeStone, EnumItem.AxeIron, EnumItem.AxeSteel);
                case EnumBlock.PlanksBirch: return new BlockUniWood(138, 137).SetCraftRecipe(2, 5, new Element(EnumBlock.LogBirch)).SetCraftTools(EnumItem.AxeStone, EnumItem.AxeIron, EnumItem.AxeSteel);
                case EnumBlock.PlanksSpruce: return new BlockUniWood(145, 144).SetCraftRecipe(2, 5, new Element(EnumBlock.LogSpruce)).SetCraftTools(EnumItem.AxeStone, EnumItem.AxeIron, EnumItem.AxeSteel);
                case EnumBlock.PlanksFruit: return new BlockUniWood(152, 151).SetCraftRecipe(2, 5, new Element(EnumBlock.LogFruit)).SetCraftTools(EnumItem.AxeStone, EnumItem.AxeIron, EnumItem.AxeSteel);
                case EnumBlock.PlanksPalm: return new BlockUniWood(160, 159).SetCraftRecipe(2, 5, new Element(EnumBlock.LogPalm)).SetCraftTools(EnumItem.AxeStone, EnumItem.AxeIron, EnumItem.AxeSteel);
                case EnumBlock.LeavesOak: return new BlockUniLeaves(1024, EnumBlock.LogOak, EnumBlock.SaplingOak);
                case EnumBlock.LeavesBirch: return new BlockUniLeaves(1030, EnumBlock.LogBirch, EnumBlock.SaplingBirch);
                case EnumBlock.LeavesSpruce: return new BlockUniLeaves(1036, EnumBlock.LogSpruce, EnumBlock.SaplingSpruce);
                case EnumBlock.LeavesFruit: return new BlockUniLeaves(1042, EnumBlock.LogFruit, EnumBlock.SaplingFruit, true);
                case EnumBlock.LeavesPalm: return new BlockLeavesPalm();
                case EnumBlock.SaplingOak: return new BlockUniSapling(132, new WorldGenTreeOak2());
                case EnumBlock.SaplingBirch: return new BlockUniSapling(139, new WorldGenTreeBirch2());
                case EnumBlock.SaplingSpruce: return new BlockUniSapling(146, new WorldGenTreeSpruce2());
                case EnumBlock.SaplingFruit: return new BlockUniSapling(153, new WorldGenTreeFruit2());
                case EnumBlock.SaplingPalm: return new BlockUniSapling(161, new WorldGenTreePalm2());
                case EnumBlock.Cactus: return new BlockCactus();
                case EnumBlock.Grass: return new BlockGrass();
                case EnumBlock.FlowerDandelion: return new BlockFlowerDandelion();
                case EnumBlock.FlowerClover: return new BlockFlowerClover();
                case EnumBlock.OreCoal: return new BlockOreCoal();
                case EnumBlock.OreIron: return new BlockOreIron();
                case EnumBlock.OreGold: return new BlockOreGold();
                case EnumBlock.Brol: return new BlockBrol();
                case EnumBlock.Glass: return new BlockUniGlass(320, new vec3(1f), false);
                case EnumBlock.GlassWhite: return new BlockUniGlass(322, new vec3(1f));
                case EnumBlock.GlassRed: return new BlockUniGlass(322, new vec3(1f, 0, 0));
                case EnumBlock.GlassPane: return new BlockUniGlassPane(320, 321, new vec3(1f), false);
                case EnumBlock.GlassPaneWhite: return new BlockUniGlassPane(322, 323, new vec3(1f));
                case EnumBlock.GlassPaneRed: return new BlockUniGlassPane(322, 323, new vec3(1f, 0, 0));
                case EnumBlock.Terracotta: return new BlockUniSolid(9, EnumItem.PieceTerracotta);
                case EnumBlock.Basalt: return new BlockBasalt();
                case EnumBlock.Obsidian: return new BlockUniSolid(12, EnumItem.Block, 40, 20); // Resistance = 2000 minecraft
                case EnumBlock.ElWire: return new BlockElWire();
                case EnumBlock.ElLampOn: return new BlockElLampOn();
                case EnumBlock.ElLampOff: return new BlockElLampOff();
                case EnumBlock.ElBattery: return new BlockElBattery();
                case EnumBlock.ElSwitch: return new BlockElSwitch();
                case EnumBlock.Tnt: return new BlockTnt();
                case EnumBlock.TntClock: return new BlockTntClock();
                case EnumBlock.DoorOak: return new BlockUniDoor(576, EnumItem.DoorOak);
                case EnumBlock.DoorBirch: return new BlockUniDoor(578, EnumItem.DoorBirch);
                case EnumBlock.DoorSpruce: return new BlockUniDoor(580, EnumItem.DoorSpruce);
                case EnumBlock.DoorFruit: return new BlockUniDoor(582, EnumItem.DoorFruit);
                case EnumBlock.DoorPalm: return new BlockUniDoor(584, EnumItem.DoorPalm);
                case EnumBlock.DoorIron: return new BlockUniDoor(586, EnumItem.DoorIron);
                case EnumBlock.SmallStone: return new BlockUniPiece(2, EnumItem.PieceStone);
                case EnumBlock.Nest: return new BlockNest();
                case EnumBlock.Tina: return new BlockTina();
                case EnumBlock.TallGrass: return new BlockTallGrass();
                case EnumBlock.Coconut: return new BlockCoconut();
                case EnumBlock.Apple: return new BlockApple();
                case EnumBlock.CraftingTableCarpentry: return new BlockCraftingTableCarpentry();
                case EnumBlock.ToolmakerTable: return new BlockToolmakerTable();
                case EnumBlock.Box: return new BlockBox();
            }

            return new BlockAir(true);
        }

        /// <summary>
        /// Инициализировать все блоки для кэша
        /// </summary>
        public static void Initialized()
        {
            // Инициализировать все материалы
            Materials.Initialized();
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
