using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Лампа выключена
    /// </summary>
    public class BlockElLampOff : BlockAbElectricity
    {
        /// <summary>
        /// Блок Лампа выключена
        /// </summary>
        public BlockElLampOff() : base(387)
        {
            InitBoxs(387, false, new vec3(1));
        }
    }
}
