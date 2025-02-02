﻿using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок брол, автор Вероника
    /// </summary>
    public class BlockBrol : BlockAbOre
    {
        /// <summary>
        /// Блок брол, автор Вероника
        /// </summary>
        public BlockBrol() : base(261)
        {
            LightValue = 15;
            АmbientOcclusion = false;
            canDropPresent = false;
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 25;

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitQuads() => InitQuads(259, 260, 261, 261, 261, 261);

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            BlockState blockStateUp = world.GetBlockState(blockPos.OffsetUp());
            if (blockStateUp.GetBlock().IsAir)
            {
                world.SpawnParticle(Entity.EnumParticle.BlockPart, 3,
                new vec3(blockPos.X + .5f, blockPos.Y + 1.0625f, blockPos.Z + .5f),
                new vec3(1, .125f, 1), 0, (int)EBlock);
            }
        }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            worldIn.SetBlockToAir(pos);
            if (!worldIn.IsRemote)
            {
                // Берём брол
                entityPlayer.Inventory.AddItemStackToInventory(worldIn, entityPlayer,
                    new ItemStack(Items.GetItemCache(EBlock)));
                // Чпок
                worldIn.PlaySoundPop(pos.ToVec3() + .5f);
            }
            return true;
        }
    }
}
