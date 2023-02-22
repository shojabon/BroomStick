using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BroomStick.Controllers
{
    public class AuthController : ControllerBase
    {

        [HttpPost("create-user")]
        public IActionResult CreateUser(string userId, string username, string password, [FromBody] BsonDocument metadata)
        {
/*            DataClasses.Authenticator.CreateUser(userId, username, password, metadata);*/
            return Ok();
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticatea(string username, string password)
        {
            return Ok();
            /*var isAuthenticated = DataClasses.Authenticator.Authenticate(username, password);
            if (isAuthenticated)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }*/
        }
    }
}
