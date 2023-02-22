﻿

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
        public Authenticator()
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
            Mongodb = new MongoClient(configuration["MongodbConnection"]);

            // Get the database object
            var database = Mongodb.GetDatabase("BroomStick");
            Collection = database.GetCollection<BsonDocument>("accounts");
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
                var key = Encoding.ASCII.GetBytes("SecretTokenForNow");
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
