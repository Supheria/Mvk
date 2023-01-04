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
                case EnumItem.Block: return new ItemBlock(block);
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
                case EnumItem.PieceDirt: return new ItemUniPiece(EnumItem.PieceDirt, 192, .32f);
                case EnumItem.PieceStone: return new ItemUniPiece(EnumItem.PieceStone, 193, .5f);
                case EnumItem.SpawnChicken: return new ItemUniSpawn(EnumItem.SpawnChicken, 224);
                case EnumItem.ExplosivesMin: return new ItemUniExplosives(EnumItem.ExplosivesMin, 256, 3, 2);
                case EnumItem.Explosives: return new ItemUniExplosives(EnumItem.Explosives, 258, 6, 3);
                case EnumItem.ExplosivesMax: return new ItemUniExplosives(EnumItem.ExplosivesMax, 260, 7, 5);
                case EnumItem.SpawnChemoglot: return new ItemUniSpawn(EnumItem.SpawnChemoglot, 225);
                case EnumItem.SpawnPakan: return new ItemUniSpawn(EnumItem.SpawnPakan, 226);
                case EnumItem.Egg: return new ItemUniFood(EnumItem.Egg, 1, 1, 16, 32);
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
                25, 26, 27, 28, 29, 41, 42, 43, 62, 63,
                30, 31, 32, 33, 34, 46, 47, 48, 49, 50,
                35, 36, 37, 38, 39, 40, 45, 51, 52, 53, 

                57, 59, 60, 61, 70, 71, 0, 0, 0, 0,
                4097, 4118, 4114, 4115, 4116, 4117, 4125, 4126, 4127, 0,
                4098, 4099, 4100, 4101, 4102, 4103, 0, 0, 0, 0,
                4122, 4123, 0, 0, 0, 0, 0, 0, 0, 0,
                4124, 4128, 4129, 0, 0, 0, 0, 0, 0, 0
            };
        }

        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных НЕ БЛОК!!!
        /// </summary>
        public static ItemBase GetItemCache(EnumItem eItem) => itemsInt[(int)eItem];
        /// <summary>
        /// Получить объект предмета с кеша, для получения информационных данных
        /// </summary>
        public static ItemBase GetItemCache(int index) 
            => index < 4096 ? itemsBlockInt[index] : itemsInt[index - 4096];
    }
}
