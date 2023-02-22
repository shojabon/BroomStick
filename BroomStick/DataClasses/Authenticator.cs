

using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BroomStick.DataClasses
{
    public class Authenticator
    {

        public static MongoClient? Mongodb = null;
        private IMongoCollection<BsonDocument> Collection;
        static string SecretToken = "testSecretForNow";
        public Authenticator()
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
            Mongodb = new MongoClient(configuration["MongodbConnection"]);

            // Get the database object
            var database = Mongodb.GetDatabase("BroomStick");
            Collection = database.GetCollection<BsonDocument>("accounts");
            CreateUser("Sho0uuid", "Sho0", "testpassword", new BsonDocument { });
            string jtw = Authenticate("Sho0", "testpassword");
            Console.WriteLine(jtw);
            Console.WriteLine(AuthenticateToken(jtw));
            Console.WriteLine(GetUser(jtw).UserId);

        }


        public void CreateUser(string userId, string username, string password, BsonDocument metadata)
        {
            // Hash the password using SHA256
            var hashedPassword = HashPassword(password);

            // Create a new document with the user data
            var user = new BsonDocument
        {
            {"user_id", userId},
            {"username", username},
            {"password", hashedPassword},
            {"metadata", metadata}
        };

            // Insert or update the user document
            var filter = Builders<BsonDocument>.Filter.Eq("user_id", userId);
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

        public AuthenticatedUser GetUser(string token)
        {
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
                        var userId = user["user_id"].AsString;
                        var metadata = user["metadata"].AsBsonDocument;
                        return new AuthenticatedUser(userId, username, metadata);
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
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }

    }
}
