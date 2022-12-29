using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Маленький блок
    /// </summary>
    public class BlockUniPiece : BlockBase
    {
        /// <summary>
        /// Текстура кусочка
        /// </summary>
        private readonly int numberTexture;
        /// <summary>
        /// Предмет который выпадет
        /// </summary>
        private readonly EnumItem enumItem;

        /// <summary>
        /// Отладочный блок
        /// </summary>
        public BlockUniPiece(int numberTexture, EnumItem enumItem)
        {
            this.numberTexture = numberTexture;
            this.enumItem = enumItem;
            Particle = numberTexture;
            FullBlock = false;
            АmbientOcclusion = false;
            Shadow = false;
            AllSideForcibly = true;
            UseNeighborBrightness = true;
            IsReplaceable = true;
            LightOpacity = 0;
            Material = EnumMaterial.Piece;
            InitBoxs();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 1;

        /// <summary>
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public override bool BlocksNotSame(int met) => true;

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        /// <summary>
        /// Коробки
        /// </summary>
        //public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[(xc + zc + xb + zb) & 4];

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        public override ItemBase GetItemDropped(BlockState state, Rand rand, int fortune)
            => Items.GetItemCache(enumItem);

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos blockPos, int met)
        {
            vec3 pos = blockPos.ToVec3();
            return new AxisAlignedBB[] { new AxisAlignedBB(pos + new vec3(MvkStatic.Xy[4],
                0, MvkStatic.Xy[4]), pos + new vec3(MvkStatic.Xy[12], MvkStatic.Xy[4], MvkStatic.Xy[12])) };
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                DropBlockAsItem(worldIn, blockPos, state, 0);
                worldIn.SetBlockToAir(blockPos, 30);
            }
        }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            worldIn.SetBlockToAir(pos, 30);

            ItemBase item = Items.GetItemCache(enumItem);
            ItemStack itemStack = new ItemStack(item);
            ItemStack itemStackIn = entityPlayer.Inventory.GetCurrentItem();
            if (itemStackIn == null)
            {
                // Если в руке пусто, берём сюда предмет
                entityPlayer.Inventory.SetCurrentItem(itemStack);
            }
            else if (itemStackIn.Item.EItem == enumItem && itemStackIn.Amount < itemStackIn.Item.MaxStackSize)
            {
                // Если в руке этот же предмет, но есть места, сумируем
                itemStackIn.AddAmount(1);
                entityPlayer.Inventory.SetCurrentItem(itemStackIn);
            }
            else
            {
                // Если в руке другой предмет, пробуем взять в инвентарь
                if (!entityPlayer.Inventory.AddItemStackToInventory(itemStack))
                {
                    // Если не смогли взять, дропаем его
                    if (!worldIn.IsRemote)
                    {
                        // Дроп
                        entityPlayer.DropItem(itemStack, true);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos) 
            => worldIn.GetBlockState(blockPos.OffsetDown()).GetBlock().FullBlock;

        private Box[] B(vec3 offsetMet)
        {
            return new Box[] {
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[4], MvkStatic.Xy[0], MvkStatic.Xy[4]),
                    To = new vec3(MvkStatic.Xy[12], MvkStatic.Xy[4], MvkStatic.Xy[12]),
                    UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[8]),
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, numberTexture),
                        new Face(Pole.Down, numberTexture)
                    },
                    Translate = offsetMet
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[4], MvkStatic.Xy[0], MvkStatic.Xy[4]),
                    To = new vec3(MvkStatic.Xy[12], MvkStatic.Xy[4], MvkStatic.Xy[12]),
                    UVFrom = new vec2(MvkStatic.Uv[0], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[4]),
                    Faces = new Face[]
                    {
                        new Face(Pole.North, numberTexture),
                        new Face(Pole.South, numberTexture),
                        new Face(Pole.East, numberTexture),
                        new Face(Pole.West, numberTexture)
                    },
                    Translate = offsetMet
                }
            };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[1][];
            boxes[0] = B(new vec3(0));
            //vec3[] offsetMet = new vec3[]
            //{
            //    new vec3(0),
            //    new vec3(.25f, 0, .25f),
            //    new vec3(-.25f, 0, .25f),
            //    new vec3(.25f, 0, -.25f),
            //    new vec3(-.25f, 0, -.25f)
            //};
            //boxes = new Box[5][];
            //for (int i = 0; i < 5; i++)
            //{
            //    boxes[i] = B(offsetMet[i]);
            //}
        }

        
    }
}
