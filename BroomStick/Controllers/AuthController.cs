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
        private readonly IConfiguration _config;
        private readonly Authenticator _authenticator;

        public AuthController(BroomStick.DataClasses.BroomStick broomStick, Authenticator authenticator, IConfiguration config)
        {
            _broomStick = broomStick;
            _authenticator = authenticator;
            _config = config;
        }

        public bool IsAuthenticated(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authenticate")) return false;
            if (!_config["AuthenticationAPIKey"].Equals(request.Headers["Authenticate"])) return false;
            return true;
        }

        public class RegisterRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string userId { get; set; }
            public BsonDocument? metadata { get; set; }
        }

        [HttpPost("register")]
        public IActionResult CreateUser([FromBody] RegisterRequest request)
        {
            if (!IsAuthenticated(Request))
            {
                return BadRequest("Unauthorized");
            }
            try
            {
                _authenticator.CreateUser(request.userId, request.username, request.password, request.metadata);
            }catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }
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
            if (!IsAuthenticated(Request))
            {
                return BadRequest("Unauthorized");
            }
            var token = _authenticator.Authenticate(request.username, request.password);
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