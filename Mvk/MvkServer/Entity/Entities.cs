using MvkServer.Entity.List;
using MvkServer.World;
using System;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект всех сущностей
    /// </summary>
    public class Entities
    {
        /// <summary>
        /// Создать объект сущности сущности
        /// </summary>
        public static EntityBase CreateEntityByEnum(WorldBase world, EnumEntities enumEssences)
        {
            switch(enumEssences)
            {
                case EnumEntities.Chicken: return new EntityChicken(world);
                case EnumEntities.Item: return new EntityItem(world);
                case EnumEntities.Piece: return new EntityPiece(world);
                case EnumEntities.Chemoglot: return new EntityChemoglot(world);
                case EnumEntities.Pakan: return new EntityPakan(world);
            }
            throw new Exception("Объекта сущности [" + enumEssences.ToString() + "] не существует");
        }
    }
}
