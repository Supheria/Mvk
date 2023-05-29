namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Объект по добавлении высот (ущелены в море и не большие неровности)
    /// </summary>
    public class GenLayerHeightAddSea : GenLayerHeightAddParam
    {
        public GenLayerHeightAddSea(long baseSeed, GenLayer parent) : base(baseSeed, parent) { }

        protected override int GetParam(int param)
        {
            if (param < 29 && NextInt(6) == 0)
            {
                // Делаем ущелены в море
                param = NextInt(4) + 3;
            }
            // Добавляем не большие неровности
            param += NextInt(4) - 2;
            return param;
        }
    }
}
