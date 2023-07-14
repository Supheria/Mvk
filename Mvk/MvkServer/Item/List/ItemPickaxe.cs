using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Item;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет кирка
    /// </summary>
    public class ItemPickaxe : ItemAbTool
    {
        public ItemPickaxe(EnumItem eItem, int numberTexture, int numberTexture3d, int level, int maxDamage, float force, float damage, int pause)
            : base(eItem, numberTexture, level, maxDamage, force, damage, pause)
        {
            ItemUseAction = EnumItemAction.Axe;
            Quads = new ItemQuadSide[]
            {
                // Палка
                new ItemQuadSide(numberTexture3d, 0, 22, 6, 26, 1).SetSide(Pole.Up, false, -2, -12, -3, 2, 30, 3),
                new ItemQuadSide(numberTexture3d, 0, 26, 32, 32, 1).SetSide(Pole.East, false, -2, -12, -3, 2, 30, 3),
                new ItemQuadSide(numberTexture3d, 0, 26, 32, 32, 1).SetSide(Pole.West, false, -2, -12, -3, 2, 30, 3),
                new ItemQuadSide(numberTexture3d, 0, 26, 32, 30, 1).SetSide(Pole.South, false, -2, -12, -3, 2, 30, 3),
                new ItemQuadSide(numberTexture3d, 0, 26, 32, 30, 1).SetSide(Pole.North, false, -2, -12, -3, 2, 30, 3),
                
                // Кирка
                new ItemQuadSide(numberTexture3d, 20, 10, 30, 18, 1).SetSide(Pole.Down, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 10, 10, 20, 18, 1).SetSide(Pole.Up, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 10, 0, 20, 10).SetSide(Pole.East, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 10, 0, 20, 10).SetSide(Pole.West, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 20, 0, 28, 10).SetSide(Pole.South, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 20, 0, 28, 10).SetSide(Pole.North, false, -4, 19, -5, 4, 29, 5),

                new ItemQuadSide(numberTexture3d, 0, 0, 6, 18).SetSide(Pole.Up, false, -3, 30, -18, 3, 34, 0).SetRotate(0, -.3f),
                new ItemQuadSide(numberTexture3d, 0, 0, 6, 18).SetSide(Pole.Down, false, -3, 30, -18, 3, 34, 0).SetRotate(0, -.3f),
                new ItemQuadSide(numberTexture3d, 6, 0, 10, 18, 1).SetSide(Pole.East, false, -3, 30, -18, 3, 34, 0).SetRotate(0, -.3f),
                new ItemQuadSide(numberTexture3d, 6, 0, 10, 18, 1).SetSide(Pole.West, false, -3, 30, -18, 3, 34, 0).SetRotate(0, -.3f),
                new ItemQuadSide(numberTexture3d, 0, 18, 6, 22).SetSide(Pole.North, false, -3, 30, -18, 3, 34, 0).SetRotate(0, -.3f),

                new ItemQuadSide(numberTexture3d, 0, 0, 6, 18).SetSide(Pole.Up, false, -3, 20, 0, 3, 24, 18).SetRotate(0, .3f),
                new ItemQuadSide(numberTexture3d, 0, 0, 6, 18).SetSide(Pole.Down, false, -3, 20, 0, 3, 24, 18).SetRotate(0, .3f),
                new ItemQuadSide(numberTexture3d, 6, 0, 10, 18, 1).SetSide(Pole.East, false, -3, 20, 0, 3, 24, 18).SetRotate(0, .3f),
                new ItemQuadSide(numberTexture3d, 6, 0, 10, 18, 1).SetSide(Pole.West, false, -3, 20, 0, 3, 24, 18).SetRotate(0, .3f),
                new ItemQuadSide(numberTexture3d, 0, 18, 6, 22).SetSide(Pole.South, false, -3, 20, 0, 3, 24, 18).SetRotate(0, .3f),

                //new ItemQuadSide(numberTexture3d, 0, 0, 6, 18).SetSide(Pole.Up, false, -3, 25, -23, 3, 29, -5).SetRotate(0, -.3f),
                //new ItemQuadSide(numberTexture3d, 0, 0, 6, 18).SetSide(Pole.Down, false, -3, 25, -23, 3, 29, -5),
                //new ItemQuadSide(numberTexture3d, 6, 0, 10, 18, 1).SetSide(Pole.East, false, -3, 25, -23, 3, 29, -5),
                //new ItemQuadSide(numberTexture3d, 6, 0, 10, 18, 1).SetSide(Pole.West, false, -3, 25, -23, 3, 29, -5),
                //new ItemQuadSide(numberTexture3d, 0, 18, 6, 22).SetSide(Pole.North, false, -3, 25, -23, 3, 29, -5),
            };
        }

        /// <summary>
        /// Может ли блок быть разрушен тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public override bool CanDestroyedBlock(BlockBase block)
        {
            MaterialBase material = block.Material;
            EnumMaterial eMaterial = material.EMaterial;
            return eMaterial == EnumMaterial.Solid || eMaterial == EnumMaterial.Ore || material.Glass 
                || block.EBlock == EnumBlock.Brol || (eMaterial == EnumMaterial.Door && block.EBlock == EnumBlock.DoorIron);
        }

        /// <summary>
        /// Может ли выпасть предмет после разрушения блока тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public override bool CanHarvestBlock(BlockBase block)
        {
            EnumMaterial eMaterial = block.Material.EMaterial;
            return eMaterial == EnumMaterial.Solid || eMaterial == EnumMaterial.Ore || (eMaterial == EnumMaterial.Door && block.EBlock == EnumBlock.DoorIron);
        }
    }
}
