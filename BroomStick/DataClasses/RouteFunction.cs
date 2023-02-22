using Microsoft.AspNetCore.Mvc;

namespace BroomStick.DataClasses
{
    public interface IRouteFunction
    {
        bool IsAllowedToUse(HttpRequest request)
        {
            return true;
        }

        void Initialize() { }

        void HandleRequest(HttpRequest request, HttpContent proxiedReuqest) { }
    }
}
