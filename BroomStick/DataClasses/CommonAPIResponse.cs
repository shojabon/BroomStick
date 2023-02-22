using System.Net.NetworkInformation;

namespace BroomStick.DataClasses
{
    public static class CommonAPIResponse
    {
        public static readonly APIResponse Success = new("success", "Success", null, 200);
        public static readonly APIResponse UnAuthorized = new("unauthorized", "Not Authorized", null, 401);
        public static readonly APIResponse RouteNotFound = new("route_not_found", "Route Not Found", null, 404);
        public static readonly APIResponse PermissionInsufficient = new("permission_lacking", "Permission Insufficient", null, 403);
        public static readonly APIResponse RateLimited = new("rate_limited", "Rate limited", null, 429);
        public static readonly APIResponse NoBackendsFound = new("backend_not_found", "No backends found", null, 502);
    }
}
