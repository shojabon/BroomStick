using System;
using System.Collections.Generic;
using Amazon.Runtime.Internal;
using BroomStick.DataClasses.Authenticator;
using Microsoft.AspNetCore.Http;

namespace BroomStick.DataClasses.Functions
{
    public class CacheFunction : IRouteFunction
    {
        public int Interval { get; set; }
        public bool TrackUser { get; set; }

        private DateTime LastResponseTimeGlobal; // only use when TrackUser is false
        private APIResponse? CachedResponseGlobal;

        private Dictionary<string, APIResponse> CachedResponsesByUser = new();
        private Dictionary<string, DateTime> LastResponseTimeByUser = new();

        public APIResponse? BeforeHandleRequest(HttpRequest request)
        {
            if(Interval== 0) return null;
            var currentTime = DateTime.UtcNow;


            var userObject = Authenticator.Authenticator.GetUser(request.Headers["Authenticate"]);
            if (userObject != null && TrackUser)
            {
                var userId = userObject.UserId;
                if (CachedResponsesByUser.ContainsKey(userId))
                {
                    var lastRequestTime = LastResponseTimeByUser[userId];
                    if ((currentTime - lastRequestTime).TotalSeconds < Interval)
                    {
                        // return cached APIResponse
                        return CachedResponsesByUser[userId];
                    }
                }
            }

            if (!TrackUser || userObject == null)
            {
                if ((currentTime - LastResponseTimeGlobal).TotalSeconds < Interval)
                {
                    // return cached APIResponse
                    return CachedResponseGlobal;
                }

                return null;
            }

            return null;
        }

        public void AfterHandleRequest(HttpRequest request, APIResponse proxiedReuqest)
        {
            if (Interval == 0) return;
            var currentTime = DateTime.UtcNow;

            var userObject = Authenticator.Authenticator.GetUser(request.Headers["Authenticate"]);
            if (userObject != null && TrackUser)
            {
                var userId = userObject.UserId;
                LastResponseTimeByUser[userId] = currentTime;
                CachedResponsesByUser[userId] = proxiedReuqest;
            }

            if (!TrackUser || userObject == null)
            {   
                LastResponseTimeGlobal = currentTime;
                CachedResponseGlobal = proxiedReuqest;
            }
        }
    }
}
