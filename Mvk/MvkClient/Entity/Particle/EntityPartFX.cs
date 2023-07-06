using MvkAssets;
using MvkClient.Renderer.Block;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Эффект части блока или предмета
    /// </summary>
    public class EntityPartFX : EntityFX
    {
        private readonly float textureU1;
        private readonly float textureV1;
        private readonly float textureU2;
        private readonly float textureV2;

        public EntityPartFX(WorldBase world, vec3 pos, vec3 motion, EnumBlock eBlock) : base(world, pos, motion)
        {
            particleGravity = 1f;
            particleScale /= 2f;
            color = new vec3(.8f);

            BlockBase block = Blocks.GetBlockCache(eBlock);
            // пробуем цвет травы подкрасить
            color *= (block.IsBiomeColorGrass() ? BlockColorBiome.GrassDefault : block.GetColorGuiOrPartFX());
            int u = block.Particle % 64;
            int v = block.Particle / 64;

            textureUV = new vec2i(u, v); // В зависимости от блока надо будет вытянуть положение
            Texture = AssetsTexture.AtlasBlocks;
            // Координаты текстуры доп смещения микро частички
            // (0..12) в пикселах, и в смещении (0..0.01171875) (12 / (16 * 64))
            textureU1 = textureUV.x / 64f + rand.Next(13) / 1024f;
            textureV1 = textureUV.y / 64f + rand.Next(13) / 1024f;
            textureU2 = textureU1 + .00390625f; // 4-ая часть блока
            textureV2 = textureV1 + .00390625f; // 4-ая часть блока
        }

        public EntityPartFX(WorldBase world, vec3 pos, vec3 motion, EnumItem eItem) : base(world, pos, motion)
        {
            particleGravity = 1f;
            particleScale /= 2f;
            color = new vec3(.8f);

            ItemBase item = Items.GetItemCache(eItem);
            int u = item.NumberTexture % 32;
            int v = item.NumberTexture / 32;

            textureUV = new vec2i(u, v); // В зависимости от блока надо будет вытянуть положение
            Texture = AssetsTexture.AtlasItems;
            // Координаты текстуры доп смещения микро частички
            // (0..24) в пикселах, и в смещении (0..0.0234375) (12 / (32 * 32))
            textureU1 = textureUV.x / 32f + rand.Next(25) / 1024f;
            textureV1 = textureUV.y / 32f + rand.Next(25) / 1024f;
            textureU2 = textureU1 + .0078125f; // 4-ая часть предмета
            textureV2 = textureV1 + .0078125f; // 4-ая часть предмета
        }

        /// <summary>
        /// Рендер прямоугольника частицы
        /// </summary>
        public override void RenderRectangle(float timeIndex) 
            => Render(particleScale * .1f, textureU1, textureV1, textureU2, textureV2);
    }
}
