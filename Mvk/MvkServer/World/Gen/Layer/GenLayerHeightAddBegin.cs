namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Объект по добавлении высот (холмов и луж)
    /// </summary>
    public class GenLayerHeightAddBegin : GenLayerHeightAddParam
    {
        public GenLayerHeightAddBegin(long baseSeed, GenLayer parent) : base(baseSeed, parent) { }

        protected override int GetParam(int param)
        {
            if (param > 49 && NextInt(6) == 0)
            {
                // Добавляем холм
                param += NextInt(20) + 1;
            }
            else if (param > 51 && NextInt(6) == 0)
            {
                // Добавляем лужу
                param -= NextInt(4) + 1;
            }
            return param;
        }
    }
}
