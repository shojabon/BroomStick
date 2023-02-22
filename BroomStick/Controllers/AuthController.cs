using BroomStick.DataClasses;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BroomStick.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BroomStick.DataClasses.BroomStick _broomStick;

        public AuthController(BroomStick.DataClasses.BroomStick broomStick)
        {
            _broomStick = broomStick;
            Console.Write("test");
            // perform some initialization or setup logic here using _broomStick
        }


        [HttpPost("create-user")]
        public IActionResult CreateUser(string userId, string username, string password, [FromBody] BsonDocument metadata)
        {
            DataClasses.BroomStick.Authenticator.CreateUser(userId, username, password, metadata);
            return Ok("User created successfully.");
        }


        public class AuthenticationRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticationRequest request)
        {
            var token = DataClasses.BroomStick.Authenticator.Authenticate(request.username, request.password);
            if (token != null)
            {
                return Ok(token);
            }
            else
            {
                return BadRequest("Invalid username or password.");
            }
        }
    }
}