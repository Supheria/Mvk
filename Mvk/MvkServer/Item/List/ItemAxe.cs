using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Item;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет топор
    /// </summary>
    public class ItemAxe : ItemAbTool
    {
        public ItemAxe(EnumItem eItem, int numberTexture, int numberTexture3d, int level, int maxDamage, float force, float damage, int pause)
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
                
                // Топорище
                new ItemQuadSide(numberTexture3d, 14, 13, 24, 21, 1).SetSide(Pole.Down, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 14, 13, 24, 21, 1).SetSide(Pole.Up, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 14, 3, 24, 13).SetSide(Pole.East, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 14, 3, 24, 13).SetSide(Pole.West, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 24, 3, 32, 13).SetSide(Pole.South, false, -4, 19, -5, 4, 29, 5),
                new ItemQuadSide(numberTexture3d, 24, 3, 32, 13).SetSide(Pole.North, false, -4, 19, -5, 4, 29, 5),

                new ItemQuadSide(numberTexture3d, 10, 13, 14, 15, 1).SetSide(Pole.Down, false, -1, 19, -9, 1, 29, -5),
                new ItemQuadSide(numberTexture3d, 10, 13, 14, 15, 1).SetSide(Pole.Up, false, -1, 19, -9, 1, 29, -5),
                new ItemQuadSide(numberTexture3d, 10, 3, 14, 13, 2).SetSide(Pole.East, false, -1, 19, -9, 1, 29, -5),
                new ItemQuadSide(numberTexture3d, 10, 3, 14, 13).SetSide(Pole.West, false, -1, 19, -9, 1, 29, -5),

                new ItemQuadSide(numberTexture3d, 2, 20, 10, 22, 1).SetSide(Pole.Down, false, -1, 13, -16, 1, 33, -9),
                new ItemQuadSide(numberTexture3d, 2, 20, 10, 22, 1).SetSide(Pole.Up, false, -1, 13, -16, 1, 33, -9),
                new ItemQuadSide(numberTexture3d, 10, 0, 2, 20).SetSide(Pole.East, false, -1, 13, -16, 1, 33, -9),
                new ItemQuadSide(numberTexture3d, 2, 0, 10, 20).SetSide(Pole.West, false, -1, 13, -16, 1, 33, -9),
                new ItemQuadSide(numberTexture3d, 0, 0, 2, 20).SetSide(Pole.South, false, -1, 13, -16, 1, 33, -9),
                new ItemQuadSide(numberTexture3d, 0, 0, 2, 20).SetSide(Pole.North, false, -1, 13, -16, 1, 33, -9)
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
            return eMaterial == EnumMaterial.Wood || eMaterial == EnumMaterial.Leaves || material.Glass 
                || block.EBlock == EnumBlock.Brol || (eMaterial == EnumMaterial.Door && block.EBlock != EnumBlock.DoorIron);
        }

        /// <summary>
        /// Может ли выпасть предмет после разрушения блока тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public override bool CanHarvestBlock(BlockBase block)
        {
            EnumMaterial eMaterial = block.Material.EMaterial;
            return eMaterial == EnumMaterial.Wood || eMaterial == EnumMaterial.Leaves || (eMaterial == EnumMaterial.Door && block.EBlock != EnumBlock.DoorIron);
        }
    }
}
