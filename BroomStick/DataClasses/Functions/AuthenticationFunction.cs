using Amazon.Runtime.Internal;

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
            if (request.Headers.ContainsKey("Authenticate"))
            {
                if (!Authenticator.Authenticator.AuthenticateToken(request.Headers["Authenticate"])) return CommonAPIResponse.UnAuthorized;
            }
            return CommonAPIResponse.Success;
        }
        public void HandleRequest(HttpRequest request, HttpContent proxiedReuqest)
        {

            var userObject = Authenticator.Authenticator.GetUser(request.Headers["Authenticate"]);
            if(userObject == null) 
            {
                return;
            }
            proxiedReuqest.Headers.Add("Authenticate", request.Headers["Authenticate"].ToArray());
        }

    }
}
