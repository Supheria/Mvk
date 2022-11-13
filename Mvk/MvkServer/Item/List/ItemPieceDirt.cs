﻿using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет Земляной кусочек
    /// </summary>
    public class ItemPieceDirt : ItemBase
    {
        public ItemPieceDirt()
        {
            EItem = EnumItem.PieceDirt;
            NumberTexture = 192;
            MaxStackSize = 128;
            UpId();
        }

        /// <summary>
        ///  Вызывается, когда предмет щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool ItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            return false;
        }
    }
}
