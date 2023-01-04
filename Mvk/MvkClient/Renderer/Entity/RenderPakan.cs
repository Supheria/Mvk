using MvkAssets;
using MvkClient.Renderer.Model;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер пакан
    /// </summary>
    public class RenderPakan : RendererLivingEntity
    {
        public RenderPakan(RenderManager renderManager, ModelBase model) : base(renderManager, model)
        {
            texture = AssetsTexture.Pakan;
            // соотношение высоты 1.0, к цельной модели 2.0, 1.0/2.0 = 0.5
            scale = 2.0f;
            shadowSize = .5f;
        }
    }
}
