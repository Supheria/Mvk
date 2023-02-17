using MvkAssets;
using MvkClient.Renderer.Model;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер книги
    /// </summary>
    public class RenderBook : RendererLivingEntity
    {
        public RenderBook(RenderManager renderManager, ModelBase model) : base(renderManager, model)
        {
            texture = AssetsTexture.Book;
            // соотношение высоты 1.0, к цельной модели 2.0, 1.0/2.0 = 0.5
            scale = 2.0f;
            shadowSize = .25f;
        }
    }
}
