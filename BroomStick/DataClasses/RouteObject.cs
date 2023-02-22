﻿using BroomStick.DataClasses.Functions;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.Loader;
using System.Collections;
using System.IO;

namespace BroomStick.DataClasses
{
    public class RouteObject
    {
        public RoutesFunction Route { get; set; } = new();
        public GroupFunction Group { get; set; } = new();
        public CacheFunction Cache { get; set; } = new();

        public BackendsFunction Backends { get; set; } = new();
        public AuthenticationFunction Authentication { get; set; } = new();

        public List<IRouteFunction> RouteFunctions = new();


        public async Task<APIResponse?> ExecuteRequest(HttpRequest request)
        {
            var allowedToUse = IsAllowedToUseRoute(request);
            if (allowedToUse.Status != "success")
            {
                return allowedToUse;
            }
            string backendUrl = Backends.Endpoints[new Random().Next(Backends.Endpoints.Count)];

            var httpClient = new HttpClient();


            HttpContent content = new StreamContent(request.Body);
            foreach (var header in request.Headers)
            {
                if (!header.Key.StartsWith("x-")) continue;
                content.Headers.Add(header.Key, header.Value.ToArray());
            }
            if(request.ContentType != null) content.Headers.Add("content-type", request.ContentType);
            if (request.HasFormContentType)
            {
                var formData = new FormUrlEncodedContent(request.Form.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
                content = formData;
            }

            foreach (var routeFunction in RouteFunctions)
            {
                var instance = routeFunction as IRouteFunction;
                if (instance == null) continue;
                instance.HandleRequest(request, content);
            }


            var backendPath = backendUrl + Route.GetRequestingExtendedPath(request.Path);
            HttpResponseMessage? response = null;

            switch (request.Method)
            {
                case "POST":
                    response = await httpClient.PostAsync(backendPath, content);
                    break;
                case "PUT":
                    response = await httpClient.PutAsync(backendPath, content);
                    break;
                case "GET":
                    response = await httpClient.GetAsync(backendPath);
                    break;
                case "DELETE":
                    response = await httpClient.DeleteAsync(backendPath);
                    break;
            }
            if(response == null) {
                return null;
            }
            var proxiedResponse = new APIResponse("success", null , null, (int) response.StatusCode);
            proxiedResponse.SetHttpResponse(response);
            return proxiedResponse;
        }



        public void InitializeRouteAfter()
        {
            RouteFunctions.Add(Route);
            RouteFunctions.Add(Group);
            RouteFunctions.Add(Cache);
            RouteFunctions.Add(Backends);
            RouteFunctions.Add(Authentication);

            foreach (var routeFunction in RouteFunctions)
            {
                var instance = routeFunction as IRouteFunction;
                if (instance == null) continue;
                instance.Initialize();
            }
        }

        public APIResponse IsAllowedToUseRoute(HttpRequest request)
        {
            foreach (var routeFunction in RouteFunctions)
            {
                IRouteFunction func = routeFunction as IRouteFunction;
                if (func == null) continue;
                var response = func.IsAllowedToUse(request);
                if (response.Status == "success") continue;
                return response;
            }
            return CommonAPIResponse.Success;
        }

        public bool IsRouteMatching(HttpRequest request)
        {
            foreach (var routeFunction in RouteFunctions)
            {
                IRouteFunction func = routeFunction as IRouteFunction;
                if (func == null) continue;
                if(!func.MatchRoute(request)) return false;
            }
            return true;
        }

    }

}
