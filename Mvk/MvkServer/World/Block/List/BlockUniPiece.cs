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
            SetUnique();
            IsReplaceable = true;
            Material = EnumMaterial.Piece;
            InitQuads();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 1;

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met];

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
            int index = met;
            if (index > 4) index -= 5;
            if (index > 4) index -= 5;

            if (index == 1)
            {
                pos.x += .25f;
                pos.z += .25f;
            }
            else if (index == 2)
            {
                pos.x -= .25f;
                pos.z += .25f;
            }
            else if (index == 3)
            {
                pos.x += .25f;
                pos.z -= .25f;
            }
            else if (index == 4)
            {
                pos.x -= .25f;
                pos.z -= .25f;
            }
            return new AxisAlignedBB[] { new AxisAlignedBB(pos + new vec3(MvkStatic.Xy[4],
                0, MvkStatic.Xy[4]), pos + new vec3(MvkStatic.Xy[12], MvkStatic.Xy[4], MvkStatic.Xy[12])) };
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                DropBlockAsItem(worldIn, blockPos, neighborState, 0);
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
                // Берём
                entityPlayer.Inventory.AddItemStackToInventory(worldIn, entityPlayer, itemStack);
            }
            return true;
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0) 
            => worldIn.GetBlockState(blockPos.OffsetDown()).GetBlock().FullBlock;

        private void InitQuads()
        {
            vec3[] offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.25f, 0, .25f),
                new vec3(-.25f, 0, .25f),
                new vec3(.25f, 0, -.25f),
                new vec3(-.25f, 0, -.25f)
            };
            float[] yaw = new float[] { 0, glm.pi45, glm.pi20 };
            quads = new QuadSide[15][];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    quads[i * 5 + j] = new QuadSide[]
                    {
                        new QuadSide(0).SetTexture(numberTexture, 0, 0, 8, 8).SetSide(Pole.Up, false, 4, 0, 4, 12, 4, 12).SetRotate(yaw[i]).SetTranslate(offsetMet[j]),
                        new QuadSide(0).SetTexture(numberTexture, 0, 0, 8, 8).SetSide(Pole.Down, false, 4, 0, 4, 12, 4, 12).SetRotate(yaw[i]).SetTranslate(offsetMet[j]),
                        new QuadSide(0).SetTexture(numberTexture, 0, 0, 8, 8).SetSide(Pole.East, false, 4, 0, 4, 12, 4, 12).SetRotate(yaw[i]).SetTranslate(offsetMet[j]),
                        new QuadSide(0).SetTexture(numberTexture, 0, 0, 8, 8).SetSide(Pole.West, false, 4, 0, 4, 12, 4, 12).SetRotate(yaw[i]).SetTranslate(offsetMet[j]),
                        new QuadSide(0).SetTexture(numberTexture, 0, 0, 8, 8).SetSide(Pole.North, false, 4, 0, 4, 12, 4, 12).SetRotate(yaw[i]).SetTranslate(offsetMet[j]),
                        new QuadSide(0).SetTexture(numberTexture, 0, 0, 8, 8).SetSide(Pole.South, false, 4, 0, 4, 12, 4, 12).SetRotate(yaw[i]).SetTranslate(offsetMet[j])
                    };
                }
            }
        }
    }
}
