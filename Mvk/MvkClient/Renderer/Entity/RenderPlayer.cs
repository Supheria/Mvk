using MvkAssets;
using MvkClient.Renderer.Entity.Layers;
using MvkClient.Renderer.Model;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер игрока
    /// </summary>
    public class RenderPlayer : RendererLivingEntity
    {
        public RenderPlayer(RenderManager renderManager, ModelPlayer model, bool invisible) : base(renderManager, model)
        {
            texture = AssetsTexture.Steve;
            // соотношение высоты 3.6, к цельной модели 2.0, 3.6/2.0 = 1.8
            scale = 1.8f;
            shadowSize = .5f;
            if (!invisible)
            {
                AddLayer(new LayerHeldItem(model.GetBoxArmRight(), renderManager.Item, false));
            }
        }
    }
}
