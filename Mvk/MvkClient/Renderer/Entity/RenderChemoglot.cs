using MvkAssets;
using MvkClient.Renderer.Model;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер чемоглота
    /// </summary>
    public class RenderChemoglot : RendererLivingEntity
    {
        public RenderChemoglot(RenderManager renderManager, ModelBase model) : base(renderManager, model)
        {
            texture = AssetsTexture.Chemoglot;
            // соотношение высоты 1.0, к цельной модели 2.0, 1.0/2.0 = 0.5
            scale = 2.0f;
            shadowSize = .5f;
        }
    }
}
