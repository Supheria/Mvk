using MvkServer.Util;

namespace MvkServer.World.Gen.Layer
{
    /// <summary>
    /// Объект по добавлении высот (неровности вверх)
    /// </summary>
    public class GenLayerHeightAddUp : GenLayerHeightAddParam
    {
        public GenLayerHeightAddUp(long baseSeed, GenLayer parent) : base(baseSeed, parent) { }

        protected override int GetParam(int param)
        {
            if (param < 45)
            {
                // Ниже воды 
                int r = (45 - param) / 2;
                if (r > 0) param += Mth.Min(NextInt(r), NextInt(r));
            }
            else if (param > 46)
            {
                // Выше воды
                int r = param - 45;
                if (r > 0) param += NextInt(r);
            }
            return param;
        }
    }
}
