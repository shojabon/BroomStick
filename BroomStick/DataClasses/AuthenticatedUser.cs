using MongoDB.Bson;

namespace BroomStick.DataClasses
{
    public class AuthenticatedUser
    {
        public string UserId { get; }
        public string Username { get; }
        public BsonDocument Metadata { get; }

        public AuthenticatedUser(string userId, string username, BsonDocument metadata)
        {
            UserId = userId;
            Username = username;
            Metadata = metadata;
        }
    }
}
