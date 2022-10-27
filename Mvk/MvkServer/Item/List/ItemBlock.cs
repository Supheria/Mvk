using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет блок
    /// </summary>
    public class ItemBlock : ItemBase
    {
        public BlockBase Block { get; private set; }

        public ItemBlock(BlockBase block)
        {
            Block = block;
            UpId();
        }

        public override string GetName() => "block." + Block.EBlock.ToString();

        /// <summary>
        ///  Вызывается, когда блок щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool ItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            if (CanPlaceBlockOnSide(stack, playerIn, worldIn, blockPos, side, facing))
            {
                if (Block.CanBlockStay(worldIn, blockPos))
                {
                    BlockState blockState = Block.OnBlockPlaced(worldIn, blockPos, new BlockState(Block.EBlock), side, facing);
                    bool result = worldIn.SetBlockState(blockPos, blockState, 15);
                    if (result) worldIn.PlaySound(playerIn, Block.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверка, может ли блок устанавливаться в этом месте
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public bool CanPlaceBlockOnSide(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase blockOld = blockState.GetBlock();
            // Проверка ставить блоки, на те которые можно, к примеру на траву
            if (!blockOld.IsReplaceable) return false;
            // Если устанавливаемый блок такой же как стоит
            if (blockOld == Block) return false;
            // Если стак пуст
            if (stack == null || stack.Amount == 0) return false;

            bool isCheckCollision = !Block.IsCollidable;
            if (!isCheckCollision)
            {
                AxisAlignedBB axisBlock = Block.GetCollision(blockPos, blockState.met);
                // Проверка коллизии игрока и блока
                isCheckCollision = axisBlock != null && !playerIn.BoundingBox.IntersectsWith(axisBlock)
                    && worldIn.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axisBlock, playerIn.Id).Count == 0;
            }

            //if (isCheckCollision)
            //{
            //    return Block.CanBlockStay(worldIn, blockPos);
            //}
            
            return isCheckCollision;
        }
    }
}
