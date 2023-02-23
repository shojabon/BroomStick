namespace BroomStick.DataClasses.Functions
{
    public class AuthenticationFunction : IRouteFunction
    {
        private RouteObject RouteObject;
        public AuthenticationFunction(RouteObject root)
        {
            this.RouteObject = root;
        }
        public APIResponse IsAllowedToUse(HttpRequest request)
        {
            if(RouteObject.Group.AllowedGroups == null || RouteObject.Group.AllowedGroups.Count == 0)
            {
                return CommonAPIResponse.Success;
            }
            if (!request.Headers.ContainsKey("Authenticate")) return CommonAPIResponse.UnAuthorized;
            if (!Authenticator.Authenticator.AuthenticateToken(request.Headers["Authenticate"])) return CommonAPIResponse.UnAuthorized;
            return CommonAPIResponse.Success;
        }
        public void HandleRequest(HttpRequest request, HttpContent proxiedReuqest)
        {
            proxiedReuqest.Headers.Add("Authenticate", request.Headers["Authenticate"].ToArray());
        }

    }
}
