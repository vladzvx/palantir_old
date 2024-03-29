﻿namespace Common.Models
{
    public class Ban
    {
        public Entity Entity { get; init; }
        public static bool TryCast(Entity entity, out Ban chat)
        {
            switch (entity.Type)
            {
                case EntityType.Stop:
                    chat = new Ban() { Entity = entity };
                    return true;
                default:
                    chat = null;
                    return false;
            }
        }
    }
}
