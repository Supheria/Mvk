using MvkServer.Entity.List;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Абстрактный класс инструмента
    /// </summary>
    public abstract class ItemAbTool : ItemBase
    {
        /// <summary>
        /// уровень инструмента, Качество дропа, 0-минимум, 1-левел инструмента, 2-левел, 3-левел
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Сила против блока
        /// </summary>
        protected readonly float force;
        /// <summary>
        /// Сила урона для атаки
        /// </summary>
        protected readonly float damage;
        /// <summary>
        /// Время паузы между разрушениями блоков в тактах
        /// </summary>
        protected readonly int pause;

        protected ItemAbTool(EnumItem enumItem, int numberTexture, int level, int maxDamage, float force, float damage, int pause)
            : base(enumItem, numberTexture, 1)
        {
            craft.SetTime(20);
            IsRenderQuadSide = true;
            MaxDamage = maxDamage;
            this.force = force;
            this.damage = damage;
            this.pause = pause;
            Level = level;
        }

        /// <summary>
        /// Вызывается, когда блок уничтожается с помощью этого предмета. 
        /// Верните true, чтобы активировать статистику «Использовать предмет».
        /// </summary>
        public override bool OnBlockDestroyed(ItemStack stack, WorldBase worldIn, BlockBase blockIn, BlockPos pos, EntityPlayer playerIn)
        {
            if (stack != null)
            {
                stack.DamageItem(worldIn, 1, playerIn, pos);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Вызывается, когда происходит удар по блоку с помощью этого предмета, вернёт true если сломался инструмент
        /// </summary>
        /// <param name="damage">сила урона на инструмент</param>
        public override bool OnHitOnBlock(ItemStack stack, WorldBase worldIn, BlockBase blockIn, BlockPos pos, EntityPlayer playerIn, int damage)
            => stack != null ? stack.DamageItem(worldIn, damage, playerIn, pos) : false;

        /// <summary>
        /// Получить силу против блока
        /// </summary>
        public override float GetStrVsBlock(BlockBase block) => force;

        /// <summary>
        /// Получить урон для атаки предметом который в руке
        /// </summary>
        public override float GetDamageToAttack() => damage;

        /// <summary>
        /// Время паузы между разрушениями блоков в тактах
        /// </summary>
        public override int PauseTimeBetweenBlockDestruction() => pause;

    }
}
