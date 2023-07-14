using MvkServer.Item.List;
using MvkServer.World.Block;

namespace MvkServer.Item
{
    /// <summary>
    /// Перечень предметов
    /// </summary>
    public class Items
    {
        /// <summary>
        /// Массив всех кэш предметов
        /// </summary>
        private static ItemBase[] itemsInt;
        /// <summary>
        /// Массив всех кэш предметов блоков
        /// </summary>
        private static ItemBase[] itemsBlockInt;
        /// <summary>
        /// Массив всех предметов которые попадают в креативный инвентарь 
        /// </summary>
        public static int[] inventory;

        private static ItemBase ToItem(EnumItem eItem, BlockBase block = null)
        {
            switch (eItem)
            {
                case EnumItem.Block: return new ItemBlock(block, EnumItem.Block, 0, -1);
                case EnumItem.Apple: return new ItemUniFood(EnumItem.Apple, 0, 4, 32, 16);
                case EnumItem.DoorOak: return new ItemDoor(EnumItem.DoorOak, 32, EnumBlock.DoorOak);
                case EnumItem.DoorBirch: return new ItemDoor(EnumItem.DoorBirch, 33, EnumBlock.DoorBirch);
                case EnumItem.DoorSpruce: return new ItemDoor(EnumItem.DoorSpruce, 34, EnumBlock.DoorSpruce);
                case EnumItem.DoorFruit: return new ItemDoor(EnumItem.DoorFruit, 35, EnumBlock.DoorFruit);
                case EnumItem.DoorPalm: return new ItemDoor(EnumItem.DoorPalm, 36, EnumBlock.DoorPalm);
                case EnumItem.DoorIron: return new ItemDoor(EnumItem.DoorIron, 37, EnumBlock.DoorIron);
                case EnumItem.Bucket: return new ItemBucket();
                case EnumItem.BucketWater: return new ItemBucketWater();
                case EnumItem.BucketLava: return new ItemBucketLava();
                case EnumItem.BucketOil: return new ItemBucketOil();
                case EnumItem.FlintAndSteel: return new ItemFlintAndSteel();
                case EnumItem.PieceDirt: return new ItemBase(EnumItem.PieceDirt, 192, 128);
                case EnumItem.PieceStone: return new ItemUniPiece(EnumItem.PieceStone, 193, .5f);
                case EnumItem.SpawnChicken: return new ItemUniSpawn(EnumItem.SpawnChicken, 224);
                case EnumItem.ExplosivesMin: return new ItemUniExplosives(EnumItem.ExplosivesMin, 256, 3, 2);
                case EnumItem.Explosives: return new ItemUniExplosives(EnumItem.Explosives, 258, 6, 3);
                case EnumItem.ExplosivesMax: return new ItemUniExplosives(EnumItem.ExplosivesMax, 260, 7, 5);
                case EnumItem.SpawnChemoglot: return new ItemUniSpawn(EnumItem.SpawnChemoglot, 225);
                case EnumItem.SpawnPakan: return new ItemUniSpawn(EnumItem.SpawnPakan, 226);
                case EnumItem.Egg: return new ItemUniFood(EnumItem.Egg, 1, 1, 16, 32);
                case EnumItem.PieceSand: return new ItemBase(EnumItem.PieceSand, 194, 128);
                case EnumItem.PieceSandstone: return new ItemUniPiece(EnumItem.PieceSandstone, 195, .32f);
                case EnumItem.PieceClay: return new ItemBase(EnumItem.PieceClay, 196, 128);
                case EnumItem.PieceTerracotta: return new ItemUniPiece(EnumItem.PieceTerracotta, 197, .32f);
                case EnumItem.PieceBasalt: return new ItemUniPiece(EnumItem.PieceBasalt, 198, .64f);
                case EnumItem.PieceLimestone: return new ItemUniPiece(EnumItem.PieceLimestone, 199, .32f);
                case EnumItem.PieceGravel: return new ItemBase(EnumItem.PieceGravel, 200, 128);
                case EnumItem.PieceGranite: return new ItemUniPiece(EnumItem.PieceGranite, 201, .32f);
                case EnumItem.SpawnBook: return new ItemUniSpawn(EnumItem.SpawnBook, 227);
                case EnumItem.TallGrass: return new ItemTallGrass(EnumItem.TallGrass, 288);
                case EnumItem.Coconut: return new ItemUniPiece(EnumItem.Coconut, 202, .16f);
                case EnumItem.HalfCoconut: return new ItemUniFood(EnumItem.HalfCoconut, 2, 6, 96, 16);
                case EnumItem.AxeStone: return new ItemAxeStone(132, 133, 1, 500, .5f, 2.5f, 6);
                case EnumItem.AxeIron: return new ItemAxe(EnumItem.AxeIron, 128, 129, 2, 1000, 2, 4, 5);
                case EnumItem.AxeSteel: return new ItemAxe(EnumItem.AxeSteel, 130, 131, 3, 5000, 5, 5, 4);
                case EnumItem.DiggingStick: return new ItemShovel(EnumItem.DiggingStick, 134, 135, 1, 500, .25f, 1.5f, 6, false);
                case EnumItem.ShovelIron: return new ItemShovel(EnumItem.ShovelIron, 136, 137, 2, 1000, 2, 2.5f, 5);
                case EnumItem.ShovelSteel: return new ItemShovel(EnumItem.ShovelSteel, 138, 139, 3, 5000, 5, 3.5f, 0);
                case EnumItem.Stick: return new ItemBase(EnumItem.Stick, 160, 32);
                case EnumItem.WoodChips: return new ItemBase(EnumItem.WoodChips, 203, 128);
                case EnumItem.DryGrass: return new ItemBase(EnumItem.DryGrass, 289, 128);
                case EnumItem.PickaxeIron: return new ItemPickaxe(EnumItem.PickaxeIron, 140, 141, 2, 1000, 2, 3, 6);
                case EnumItem.PickaxeSteel: return new ItemPickaxe(EnumItem.PickaxeSteel, 142, 143, 3, 5000, 5, 4, 5);
            }
            
            return null;
        }

        /// <summary>
        /// Инициализировать все предметы для кэша
        /// </summary>
        public static void Initialized()
        {
            int countBlock = BlocksCount.COUNT + 1;
            int countItem = ItemsCount.COUNT + 1;

            itemsInt = new ItemBase[countItem];
            itemsBlockInt = new ItemBase[countBlock];

            for (int i = 1; i < countBlock; i++)
            {
                BlockBase block = Blocks.GetBlockCache(i);
                ItemBase item = ToItem(EnumItem.Block, block);
                itemsBlockInt[i] = item;
            }
            for (int i = 1; i < countItem; i++)
            {
                ItemBase item = ToItem((EnumItem)i);
                itemsInt[i] = item;
            }

            inventory = new int[]
            {
                2, 3, 4, 5, 6, 7, 8, 9, 10, 11,
                20, 21, 22, 23, 24, 12, 54, 55, 56, 44,
                25, 26, 27, 28, 29, 40, 72, 41, 42, 43,
                30, 31, 32, 33, 34, 46, 47, 48, 49, 50,
                35, 36, 37, 38, 39, 4140, 45, 51, 52, 53, 

                57, 59, 60, 61, 70, 71, 62, 63, 74, 75,
                4097, 4142, 4118, 4114, 4115, 4116, 4117, 4125, 4126, 4127, 
                4098, 4099, 4100, 4101, 4102, 4103, 4149, 4147, 4148, 4141, 
                4122, 4123, 4131, 4132, 4133, 4134, 4135, 4136, 4137, 4138,
                4124, 4128, 4129, 4139, 0, 0, 0, 0, 0, 0,

                4144, 4119, 4143, 4145, 4120, 4146, 4121, 4150
            };
        }

        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных НЕ БЛОК!!!
        /// </summary>
        public static ItemBase GetItemCache(EnumItem eItem) => itemsInt[(int)eItem];
        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных БЛОК!!!
        /// </summary>
        public static ItemBase GetItemCache(EnumBlock eBlock) => itemsBlockInt[(int)eBlock];
        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных
        /// </summary>
        public static ItemBase GetItemCache(int index) 
            => index < 4096 ? itemsBlockInt[index] : itemsInt[index - 4096];
    }
}
