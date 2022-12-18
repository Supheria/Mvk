namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет яблоко
    /// </summary>
    public class ItemApple : ItemAbFood
    {
        public ItemApple()
        {
            EItem = EnumItem.Apple;
            NumberTexture = 0;
            MaxStackSize = 16;
            UpId();
        }
    }
}
