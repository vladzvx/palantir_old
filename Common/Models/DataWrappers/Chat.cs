﻿namespace Common.Models
{
    public class Chat
    {
        public Entity Entity { get; init; }
        public static bool TryCast(Entity entity, out Chat chat)
        {
            switch (entity.Type)
            {
                case EntityType.Channel:
                case EntityType.Group:
                    chat = new Chat() { Entity = entity };
                    return true;
                default:
                    chat = null;
                    return false;
            }
        }
    }
}
