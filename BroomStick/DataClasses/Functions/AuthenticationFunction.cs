namespace BroomStick.DataClasses.Functions
{
    public class AuthenticationFunction : IRouteFunction
    {
        public bool IsAllowedToUse(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authenticate")) return false;
            if (!Authenticator.AuthenticateToken(request.Headers["Authenticate"])) return false;
            return true;
        }
        public void HandleRequest(HttpRequest request, HttpContent proxiedReuqest)
        {
            proxiedReuqest.Headers.Add("Authenticate", request.Headers["Authenticate"].ToArray());
        }
    }
}
