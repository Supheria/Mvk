using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет пустое ведро
    /// </summary>
    public class ItemBucket : ItemBase
    {
        public ItemBucket() 
        {
            MaxDamage = 0;
            MaxStackSize = 16;
            EItem = EnumItem.Bucket;
            NumberTexture = 96;
            UpId();
        }

        /// <summary>
        /// Вызывается всякий раз, когда этот предмет экипирован и нажата правая кнопка мыши.
        /// </summary>
        public override ItemStack OnItemRightClick(ItemStack itemStackIn, WorldBase worldIn, EntityPlayer playerIn)
        {
            if (playerIn.Inventory.GetCurrentItem().Equals(itemStackIn)
                && itemStackIn.Amount >= 1 && itemStackIn.Item != null && itemStackIn.Item.EItem == EnumItem.Bucket)
            {
                vec3 pos = playerIn.Position;
                pos.y += playerIn.GetEyeHeight();
                vec3 vec = EntityLiving.GetRay(playerIn.RotationYawHead, playerIn.RotationPitch);
                MovingObjectPosition moving = worldIn.RayCastBlock(pos, vec, MvkGlobal.RAY_CAST_DISTANCE, false, true);
                if (moving.IsLiquid)
                {
                    // Можно добавить проверку на блок жидкости
                    bool result = worldIn.SetBlockToAir(moving.BlockLiquidPosition);
                    if (result)
                    {
                        ItemAbBucketLiquid item = null;
                        switch (moving.EBlockLiquid)
                        {
                            case EnumBlock.Lava: item = new ItemBucketLava(); break;
                            case EnumBlock.Oil: item = new ItemBucketOil(); break;
                            case EnumBlock.Water: item = new ItemBucketWater(); break;
                        }
                        if (item != null)
                        {
                            ItemStack itemStack = new ItemStack(item);
                            if (itemStackIn.Amount == 1)
                            {
                                playerIn.Inventory.SetCurrentItem(itemStack);
                            }
                            else
                            {
                                playerIn.Inventory.DecrStackSize(playerIn.Inventory.CurrentItem, 1);
                                // Берём
                                playerIn.Inventory.AddItemStackToInventory(worldIn, playerIn, itemStack);
                            }
                            worldIn.PlaySound(playerIn, item.BlockLiquid.SampleBreak(worldIn), moving.BlockLiquidPosition.ToVec3(), 1f, 1f);
                            return itemStack;
                        }
                    }
                }
            }
            return itemStackIn;
        }
            
    }
}
