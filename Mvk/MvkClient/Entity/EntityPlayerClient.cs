using MvkClient.World;
using MvkServer.Entity.List;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность игрока для клиента
    /// </summary>
    public abstract class EntityPlayerClient : EntityPlayer
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }

        public EntityPlayerClient(WorldClient world) : base(world)
        {
            ClientWorld = world;
            ClientMain = world.ClientMain;
        }

        /// <summary>
        /// Задать данные игрока
        /// </summary>
        public void SetDataPlayer(ushort id, string uuid, string name)
        {
            base.name = name;
            UUID = uuid;
            Id = id;
        }

        public override string ToString()
        {
            return name + " " + base.ToString();
        }
    }
}
