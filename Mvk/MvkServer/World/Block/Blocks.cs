using MvkServer.Glm;
using MvkServer.Item;
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
                case EnumBlock.Stone: return new BlockUniSolid(2, new vec3(.7f), EnumItem.PieceStone);
                case EnumBlock.Cobblestone: return new BlockUniSolid(3, new vec3(.7f), EnumItem.PieceStone);
                case EnumBlock.Limestone: return new BlockUniSolid(4, new vec3(.76f, .77f, .67f), EnumItem.PieceLimestone, 20);
                case EnumBlock.Granite: return new BlockUniSolid(5, new vec3(.86f, .69f, .6f), EnumItem.PieceGranite);
                case EnumBlock.Sandstone: return new BlockSandstone();
                case EnumBlock.Dirt: return new BlockUniLoose(64, new vec3(.62f, .44f, .37f), EnumItem.PieceDirt);
                case EnumBlock.Turf: return new BlockTurf();
                case EnumBlock.Sand: return new BlockSand();
                case EnumBlock.Gravel: return new BlockUniLoose(69, new vec3(1), EnumItem.PieceGravel);
                case EnumBlock.Clay: return new BlockUniLoose(70, new vec3(1), EnumItem.PieceClay);
                case EnumBlock.Water: return new BlockWater();
                case EnumBlock.WaterFlowing: return new BlockWaterFlowing();
                case EnumBlock.Lava: return new BlockLava();
                case EnumBlock.LavaFlowing: return new BlockLavaFlowing();
                case EnumBlock.Oil: return new BlockOil();
                case EnumBlock.OilFlowing: return new BlockOilFlowing();
                case EnumBlock.Fire: return new BlockFire();
                case EnumBlock.LogOak: return new BlockUniLog(129, 128, new vec3(.79f, .64f, .43f), new vec3(.62f, .44f, .37f), EnumBlock.LeavesOak, 16, 5);
                case EnumBlock.LogBirch: return new BlockUniLog(136, 135, new vec3(.81f, .74f, .5f), new vec3(1f), EnumBlock.LeavesBirch);
                case EnumBlock.LogSpruce: return new BlockUniLog(143, 142, new vec3(.58f, .42f, .22f), new vec3(.35f, .25f, .12f), EnumBlock.LeavesSpruce, 21, 5);
                case EnumBlock.LogFruit: return new BlockUniLog(150, 149, new vec3(.84f, .72f, .3f), new vec3(.62f, .55f, .25f), EnumBlock.LeavesFruit, 14);
                case EnumBlock.LogPalm: return new BlockLogPalm();
                case EnumBlock.PlanksOak: return new BlockUniWood(131, 130, new vec3(.79f, .64f, .43f), new vec3(.79f, .64f, .43f));
                case EnumBlock.PlanksBirch: return new BlockUniWood(138, 137, new vec3(.81f, .74f, .5f), new vec3(.81f, .74f, .5f));
                case EnumBlock.PlanksSpruce: return new BlockUniWood(145, 144, new vec3(.58f, .42f, .22f), new vec3(.58f, .42f, .22f));
                case EnumBlock.PlanksFruit: return new BlockUniWood(152, 151, new vec3(.84f, .72f, .3f), new vec3(.84f, .72f, .3f));
                case EnumBlock.PlanksPalm: return new BlockUniWood(160, 159, new vec3(.8f, .62f, .5f), new vec3(.8f, .62f, .5f));
                case EnumBlock.LeavesOak: return new BlockUniLeaves(133);
                case EnumBlock.LeavesBirch: return new BlockUniLeaves(140);
                case EnumBlock.LeavesSpruce: return new BlockUniLeaves(147);
                case EnumBlock.LeavesFruit: return new BlockUniLeaves(154, true);
                case EnumBlock.LeavesPalm: return new BlockUniLeaves(162, true);
                case EnumBlock.SaplingOak: return new BlockSaplingOak();
                case EnumBlock.SaplingBirch: return new BlockSaplingBirch();
                case EnumBlock.SaplingSpruce: return new BlockSaplingSpruce();
                case EnumBlock.SaplingFruit: return new BlockSaplingFruit();
                case EnumBlock.SaplingPalm: return new BlockSaplingPalm();
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
                case EnumBlock.Terracotta: return new BlockUniSolid(9, new vec3(.62f, .44f, .37f), EnumItem.PieceTerracotta);
                case EnumBlock.Basalt: return new BlockBasalt();
                case EnumBlock.Obsidian: return new BlockUniSolid(12, new vec3(.6f), EnumItem.Block, 40, 20); // Resistance = 2000 minecraft
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
