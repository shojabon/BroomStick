﻿using Microsoft.AspNetCore.Mvc;

namespace BroomStick.DataClasses
{
    public interface IRouteFunction
    {

        public APIResponse IsAllowedToUse(HttpRequest request)
        {
            return CommonAPIResponse.Success;
        }

        public bool MatchRoute(HttpRequest request)
        {
            return true;
        }

        public void Initialize() { }

        public void HandleRequest(HttpRequest request, HttpContent proxiedReuqest) { }
    }
}
