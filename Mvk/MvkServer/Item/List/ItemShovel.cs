using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Item;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет лопата
    /// </summary>
    public class ItemShovel : ItemAbTool
    {
        public ItemShovel(EnumItem eItem, int numberTexture, int numberTexture3d, int level, int maxDamage, float force, float damage, int pause, bool isModelStall = true)
            : base(eItem, numberTexture, level, maxDamage, force, damage, pause)
        {
            ItemUseAction = EnumItemAction.Shovel;
            if (isModelStall)
            {
                Quads = new ItemQuadSide[]
                {
                    //South up
                    // Палка
                    new ItemQuadSide(numberTexture3d, 0, 21, 2, 23).SetSide(Pole.Up, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 20, 0, 24, 32).SetSide(Pole.East, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 24, 0, 20, 32).SetSide(Pole.West, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 24, 0, 28, 32).SetSide(Pole.South, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 28, 0, 32, 32).SetSide(Pole.North, false, -2, -12, -2, 2, 20, 2),

                    // Лопатнище
                    new ItemQuadSide(numberTexture3d, 0, 0, 16, 1).SetSide(Pole.Up, false, -8, 12, -2, 8, 34, 0),
                    new ItemQuadSide(numberTexture3d, 0, 21, 16, 22).SetSide(Pole.Down, false, -8, 12, -2, 8, 34, 0),
                    new ItemQuadSide(numberTexture3d, 0, 0, 1, 22).SetSide(Pole.East, false, -8, 12, -2, 8, 34, 0),
                    new ItemQuadSide(numberTexture3d, 0, 0, 1, 22).SetSide(Pole.West, false, -8, 12, -2, 8, 34, 0),
                    new ItemQuadSide(numberTexture3d, 0, 0, 16, 22).SetSide(Pole.South, false, -8, 12, -2, 8, 34, 0),
                    new ItemQuadSide(numberTexture3d, 0, 0, 16, 22).SetSide(Pole.North, false, -8, 12, -2, 8, 34, 0),
                };
            }
            else
            {
                Quads = new ItemQuadSide[]
                {
                    // Палка
                    new ItemQuadSide(numberTexture3d, 0, 0, 32, 4, 1).SetSide(Pole.East, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 0, 4, 32, 8, 1).SetSide(Pole.West, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 0, 0, 32, 4, 3).SetSide(Pole.South, false, -2, -12, -2, 2, 20, 2),
                    new ItemQuadSide(numberTexture3d, 0, 0, 32, 4, 1).SetSide(Pole.North, false, -2, -12, -2, 2, 20, 2),
                
                    // Пупсик
                    new ItemQuadSide(numberTexture3d, 0, 8, 4, 11, 3).SetSide(Pole.Up, false, -5, 12, -2, -2, 17, 2),
                    new ItemQuadSide(numberTexture3d, 0, 8, 4, 11, 1).SetSide(Pole.Down, false, -5, 12, -2, -2, 17, 2),
                    new ItemQuadSide(numberTexture3d, 0, 0, 5, 4, 1).SetSide(Pole.West, false, -5, 12, -2, -2, 17, 2),
                    new ItemQuadSide(numberTexture3d, 0, 1, 5, 4, 1).SetSide(Pole.South, false, -5, 12, -2, -2, 17, 2),
                    new ItemQuadSide(numberTexture3d, 0, 1, 5, 4, 1).SetSide(Pole.North, false, -5, 12, -2, -2, 17, 2),

                    // Верхушка
                    new ItemQuadSide(numberTexture3d, 14, 16, 18, 22, 1).SetSide(Pole.Up, false, -2, 20, -2, 4, 30, 2),
                    new ItemQuadSide(numberTexture3d, 0, 16, 4, 22, 1).SetSide(Pole.Down, false, -2, 20, -2, 4, 30, 2),
                    new ItemQuadSide(numberTexture3d, 4, 22, 14, 16, 1).SetSide(Pole.South, false, -2, 20, -2, 4, 30, 2),
                    new ItemQuadSide(numberTexture3d, 4, 16, 14, 22, 1).SetSide(Pole.North, false, -2, 20, -2, 4, 30, 2),
                    new ItemQuadSide(numberTexture3d, 0, 0, 10, 4, 1).SetSide(Pole.East, false, -2, 20, -2, 4, 30, 2),
                    new ItemQuadSide(numberTexture3d, 0, 0, 10, 4, 1).SetSide(Pole.West, false, -2, 20, -2, 4, 30, 2),
                };
            }
        }

        /// <summary>
        /// Может ли блок быть разрушен тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public override bool CanDestroyedBlock(BlockBase block)
        {
            MaterialBase material = block.Material;
            EnumMaterial eMaterial = material.EMaterial;
            return eMaterial == EnumMaterial.Loose || eMaterial == EnumMaterial.Leaves || material.Glass;
        }

        /// <summary>
        /// Может ли выпасть предмет после разрушения блока тикущим предметом
        /// </summary>
        /// <param name="block">блок который разрушаем</param>
        public override bool CanHarvestBlock(BlockBase block) => block.Material.EMaterial == EnumMaterial.Loose;
    }
}
