namespace BroomStick.DataClasses.Functions
{
    public class AuthenticationFunction : IRouteFunction
    {
        public APIResponse IsAllowedToUse(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authenticate")) return CommonAPIResponse.UnAuthorized;
            if (!Authenticator.AuthenticateToken(request.Headers["Authenticate"])) return CommonAPIResponse.UnAuthorized;
            return CommonAPIResponse.Success;
        }
        public void HandleRequest(HttpRequest request, HttpContent proxiedReuqest)
        {
            proxiedReuqest.Headers.Add("Authenticate", request.Headers["Authenticate"].ToArray());
        }
    }
}
