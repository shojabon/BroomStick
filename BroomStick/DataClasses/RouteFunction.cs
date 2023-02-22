using Microsoft.AspNetCore.Mvc;

namespace BroomStick.DataClasses
{
    public interface IRouteFunction
    {
        APIResponse IsAllowedToUse(HttpRequest request)
        {
            return CommonAPIResponse.Success;
        }

        public bool MatchRoute(HttpRequest request)
        {
            return true;
        }

        void Initialize() { }

        void HandleRequest(HttpRequest request, HttpContent proxiedReuqest) { }
    }
}
