using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
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
