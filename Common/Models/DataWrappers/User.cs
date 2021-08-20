namespace Common.Models
{
    public class User
    {
        public Entity Entity { get; init; }
        public static bool TryCast(Entity entity, out User user)
        {
            switch (entity.Type)
            {
                case EntityType.User:
                    user = new User() { Entity = entity };
                    return true;
                default:
                    user = null;
                    return false;
            }
        }
    }
}
