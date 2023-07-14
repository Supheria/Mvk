namespace MvkServer.World.Block
{
    /// <summary>
    /// Перечень материалов
    /// </summary>
    public class Materials
    {
        /// <summary>
        /// Массив всех кэш материалов
        /// </summary>
        private static MaterialBase[] materialsInt;

        private static MaterialBase ToMaterial(EnumMaterial eMaterial)
        {
            MaterialBase material = new MaterialBase(eMaterial);
            switch (eMaterial)
            {
                case EnumMaterial.Air: return material.SetTurfDoesNotDry();
                case EnumMaterial.Water: return material.Liquid();
                case EnumMaterial.Lava: return material.Liquid().SetIgnites();
                case EnumMaterial.Oil: return material.Liquid();
                case EnumMaterial.Fire: return material.SetRequiresNoTool().SetIgnites();
                case EnumMaterial.Leaves: return material.SetRequiresNoTool();
                case EnumMaterial.Sapling: return material.SetRequiresNoTool().SetTurfDoesNotDry();
                case EnumMaterial.Glass: return material.SetGlass();
                case EnumMaterial.GlassPane: return material.SetGlass().SetTurfDoesNotDry();
                case EnumMaterial.Piece: return material.SetTurfDoesNotDry();
            }
            return material;
        }

        /// <summary>
        /// Инициализировать все материалы для кэша
        /// </summary>
        public static void Initialized()
        {
            int count = MaterialsCount.COUNT + 1;
            materialsInt = new MaterialBase[count];
            for (int i = 0; i < count; i++)
            {
                materialsInt[i] = ToMaterial((EnumMaterial)i);
            }
            return;
        }

        /// <summary>
        /// Получить объект материала с кеша, для получения информационных данных
        /// </summary>
        public static MaterialBase GetMaterialCache(EnumMaterial enumMaterial) => materialsInt[(int)enumMaterial];
        /// <summary>
        /// Получить объект материала с кеша, для получения информационных данных
        /// </summary>
        public static MaterialBase GetMaterialCache(int index) => materialsInt[index];
    }
}
