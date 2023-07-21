using MvkClient.Renderer;
using MvkServer.Item;

namespace MvkClient.Gui
{
    /// <summary>
    /// Объект просмотра предмета
    /// </summary>
    public class ViewItem : Control
    {
        /// <summary>
        /// Предмет
        /// </summary>
        public ItemBase Item { get; private set; }

        public void SetItem(ItemBase item) => Item = item;

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw(float timeIndex)
        {
            if (Item != null)
            {
                float ageInTicks = (screen.ClientMain.TickCounter + timeIndex) * 2f;
                GLRender.PushMatrix();
                GLRender.Translate(0, Item.IsTool() ? 25 : 0, 50);
                GLRender.Rotate(-20, 1, 0, 0);
                GLRender.Rotate(ageInTicks, 0, 1, 0);
                if (Item.EItem == EnumItem.Block) GLRender.Scale(35, -35, 35);
                else GLRender.Scale(50, -50, 50);
                screen.ClientMain.World.RenderEntityManager.Item.Render(Item);
                GLRender.PopMatrix();
            }
        }

        public override string ToString() => Item == null ? "null" : Item.ToString();
    }
}
