using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BroomStick.DataClasses.Authenticator
{
    public class Authenticator
    {

        public static MongoClient? Mongodb = null;
        private static IMongoCollection<BsonDocument>? Collection;
        static string SecretToken = "testSecretForNow";
        public static IConfiguration? Configuration;
        public static int AuthenticationDuration = 86400;

        public Authenticator(IWebHostEnvironment environment)
        {
            Configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .Build();
            SecretToken = Configuration["AuthenticatorSecret"];
            AuthenticationDuration = int.Parse(Configuration["AuthenticationDuration"]);

            Mongodb = new MongoClient(Configuration["MongodbConnection"]);

            // Get the database object
            var database = Mongodb.GetDatabase("BroomStick");
            Collection = database.GetCollection<BsonDocument>("accounts");
        }


        public void CreateUser(string userId, string username, string password, BsonDocument? metadata)
        {
            // Hash the password using SHA256
            var hashedPassword = HashPassword(password);
            if (metadata == null)
            {
                metadata = new BsonDocument();
            }

            // Create a new document with the user data
            var user = new BsonDocument
        {
            {"userId", userId},
            {"username", username},
            {"password", hashedPassword},
            {"metadata", metadata}
        };

            // Insert or update the user document
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var options = new FindOneAndReplaceOptions<BsonDocument>() { IsUpsert = true };
            Collection.FindOneAndReplace(filter, user, options);
        }


        public string Authenticate(string username, string password)
        {
            // Hash the password using SHA256
            var hashedPassword = HashPassword(password);

            // Find the user with the given username and password
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("username", username),
                Builders<BsonDocument>.Filter.Eq("password", hashedPassword)
            );
            var result = Collection.Find(filter).FirstOrDefault();

            if (result != null)
            {
                // Generate a JWT token for the authenticated user
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(SecretToken);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, username)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                return jwtToken;
            }
            else
            {
                // Return null if the authentication failed
                return null;
            }
        }

        public static bool AuthenticateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(SecretToken);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, AuthenticatedUser> cachedUsers = new Dictionary<string, AuthenticatedUser>();

        public static AuthenticatedUser GetUser(string token)
        {
            if (token == null) return null;

            // check if user is already in the cache
            if (cachedUsers.TryGetValue(token, out AuthenticatedUser cachedUser))
            {
                return cachedUser;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(SecretToken);
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

                if (claimsPrincipal.Identity.IsAuthenticated)
                {
                    var username = claimsPrincipal.Identity.Name;
                    var filter = Builders<BsonDocument>.Filter.Eq("username", username);
                    var user = Collection.Find(filter).FirstOrDefault();

                    if (user != null)
                    {
                        var userId = user["userId"].AsString;
                        var metadata = user["metadata"].AsBsonDocument;

                        // add user to cache
                        cachedUsers[token] = new AuthenticatedUser(userId, username, metadata);

                        return cachedUsers[token];
                    }
                }
            }
            catch (SecurityTokenException)
            {
                // Invalid token
            }
            catch
            {
                // Other errors
            }
            return null;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(SecretToken + password));
                return Convert.ToBase64String(hashBytes);
            }
        }

    }
}
