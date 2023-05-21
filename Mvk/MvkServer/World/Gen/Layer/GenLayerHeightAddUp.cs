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
            if (param < 46)
            {
                // Ниже воды 
                int r = (46 - param) / 2;
                if (r > 0) param += Mth.Min(NextInt(r), NextInt(r));
            }
            else if (param > 47)
            {
                // Выше воды
                int r = param - 46;
                if (r > 0) param += NextInt(r);
            }
            return param;
        }
    }
}
