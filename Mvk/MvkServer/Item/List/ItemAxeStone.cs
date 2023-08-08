using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Item;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Каменный топор
    /// </summary>
    public class ItemAxeStone : ItemAbTool
    {
        public ItemAxeStone(int numberTexture, int numberTexture3d, int level, int maxDamage, float force, float damage, int pause)
            : base(EnumItem.AxeStone, numberTexture, level, maxDamage, force, damage, pause)
        {
            ItemUseAction = EnumItemAction.Axe;
            Quads = new ItemQuadSide[]
            {
                // Палка
                new ItemQuadSide(numberTexture3d, 0, 24, 4, 28, 1).SetSide(Pole.Up, false, -2, -12, -2, 2, 30, 2),
                new ItemQuadSide(numberTexture3d, 0, 28, 32, 32, 1).SetSide(Pole.East, false, -2, -12, -2, 2, 30, 2),
                new ItemQuadSide(numberTexture3d, 0, 28, 32, 32, 1).SetSide(Pole.West, false, -2, -12, -2, 2, 30, 2),
                new ItemQuadSide(numberTexture3d, 0, 28, 32, 30, 1).SetSide(Pole.South, false, -2, -12, -2, 2, 30, 2),
                new ItemQuadSide(numberTexture3d, 0, 28, 32, 30, 1).SetSide(Pole.North, false, -2, -12, -2, 2, 30, 2),
                
                // Топорище
                new ItemQuadSide(numberTexture3d, 0, 12, 14, 20, 1).SetSide(Pole.Down, false, -10, 17, -7, -2, 29, 7),
                new ItemQuadSide(numberTexture3d, 0, 12, 14, 20, 1).SetSide(Pole.Up, false, -10, 17, -7, -2, 29, 7),
                new ItemQuadSide(numberTexture3d, 0, 0, 14, 12).SetSide(Pole.East, false, -10, 17, -7, -2, 29, 7),
                new ItemQuadSide(numberTexture3d, 0, 0, 14, 12).SetSide(Pole.West, false, -10, 17, -7, -2, 29, 7),
                new ItemQuadSide(numberTexture3d, 14, 0, 22, 12).SetSide(Pole.South, false, -10, 17, -7, -2, 29, 7),
                new ItemQuadSide(numberTexture3d, 14, 0, 22, 12).SetSide(Pole.North, false, -10, 17, -7, -2, 29, 7)
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
            return eMaterial == EnumMaterial.Wood || eMaterial == EnumMaterial.Leaves
                || eMaterial == EnumMaterial.WoodTable || material.Glass 
                || block.EBlock == EnumBlock.Brol || eMaterial == EnumMaterial.Solid || eMaterial == EnumMaterial.Ore
                || eMaterial == EnumMaterial.Door;
        }

        /// <summary>
        /// Может ли выпасть предмет после разрушения блока тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public override bool CanHarvestBlock(BlockBase block)
        {
            EnumMaterial eMaterial = block.Material.EMaterial;
            EnumBlock eBlock = block.EBlock;
            return eMaterial == EnumMaterial.Wood || eMaterial == EnumMaterial.Leaves
                || eMaterial == EnumMaterial.WoodTable || eBlock == EnumBlock.Stone || eBlock == EnumBlock.OreCoal 
                || eBlock == EnumBlock.OreIron || eMaterial == EnumMaterial.Door;
        }
    }
}
