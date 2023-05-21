using MvkServer.Glm;
using MvkServer.World.Biome;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Цвет блоков от биома
    /// </summary>
    public class BlockColorBiome
    {
        /// <summary>
        /// Цвет травы
        /// </summary>
        public static vec3 Grass(EnumBiome eBiome)
        {
            switch (eBiome)
            {
                //case EnumBiome.River:
                //case EnumBiome.Sea:
                //    return new vec3(.06f, .73f, .85f);
                case EnumBiome.Desert:
                case EnumBiome.Tropics:
                case EnumBiome.MountainsDesert:
                    return new vec3(.96f, .73f, .35f);
                case EnumBiome.BirchForest:
                case EnumBiome.MixedForest:
                    return new vec3(.46f, .63f, .25f);
                case EnumBiome.ConiferousForest:
                    return new vec3(.38f, .60f, .20f);
                case EnumBiome.Swamp:
                    return new vec3(.56f, .63f, .35f);
            }
            return new vec3(.56f, .73f, .35f);
        }

        /// <summary>
        /// Цвет листвы
        /// </summary>
        public static vec3 Leaves(EnumBiome eBiome)
        {
            switch (eBiome)
            {
                case EnumBiome.Desert:
                case EnumBiome.MountainsDesert:
                    return new vec3(.76f, .73f, .35f);
                case EnumBiome.Tropics:
                    return new vec3(.65f, .73f, .35f);
                case EnumBiome.BirchForest:
                case EnumBiome.MixedForest:
                    return new vec3(.46f, .63f, .25f);
                case EnumBiome.ConiferousForest:
                    return new vec3(.38f, .60f, .20f);
                case EnumBiome.Swamp:
                    return new vec3(.56f, .63f, .35f);
            }
            return new vec3(.56f, .73f, .35f);
        }

        /// <summary>
        /// Цвет воды
        /// </summary>
        public static vec3 Water(EnumBiome eBiome)
        {
            switch (eBiome)
            {
                case EnumBiome.Swamp: return new vec3(.36f, .63f, .68f);
                case EnumBiome.Sea: return new vec3(.1f, .35f, .7f);
            }
            return new vec3(.24f, .45f, .88f);
        }
    }
}
