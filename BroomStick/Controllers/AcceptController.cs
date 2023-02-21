using BroomStick.DataClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BroomStick.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [Route("{*path}")]
        public async Task<IActionResult> HandleRoute(string path, [FromServices] IHttpContextAccessor httpContextAccessor)
        {
            var routes = DataClasses.BroomStick.RouteObjects;
            if(routes == null || routes.Count == 0 || path == null)
            {
                return NotFound("Fail");
            }
            RouteObject? matchedRoute = routes.FirstOrDefault(r => r.IsAllowedToUseRoute(Request));
            if (matchedRoute != null)
            {
                var response = await matchedRoute.ExecuteRequest(Request);
                return Ok(response);
            }
            else
            {
                return NotFound("Fail");
            }
        }
    }
}
