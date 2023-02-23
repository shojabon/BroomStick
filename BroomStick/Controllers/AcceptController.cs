using BroomStick.DataClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BroomStick.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [Route("{*path}")]
        public async Task<IActionResult> HandleRoute(string path, [FromServices] IHttpContextAccessor httpContextAccessor)
        {
            var routes = DataClasses.BroomStick.RouteObjects;
            RouteObject? matchedRoute = routes.FirstOrDefault(r => r.IsRouteMatching(Request));
            if(matchedRoute == null)
            {
                return new ObjectResult(CommonAPIResponse.RouteNotFound);
            }
            try
            {
                var response = await matchedRoute.ExecuteRequest(Request);
                if (response == null)
                {
                    return new ObjectResult(CommonAPIResponse.RouteNotFound);
                }
                return await response.GetObjectResult();
            }catch(Exception ex)
            {
                return await CommonAPIResponse.BackendDisconnected.GetObjectResult();
            }
        }
    }
}
