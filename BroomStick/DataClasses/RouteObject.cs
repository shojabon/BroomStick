using BroomStick.DataClasses.Functions;
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
        public RoutesFunction Route { get; set; }
        public GroupFunction Group { get; set; }
        public CacheFunction Cache { get; set; }

        public BackendsFunction Backends { get; set; }

        public List<IRouteFunction> RouteFunctions = new();


        public async Task<String?> ExecuteRequest(HttpRequest request)
        {
            if (Backends == null || Backends.Endpoints.Count == 0)
            {
                throw new InvalidOperationException("No backends configured for route.");
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



            HttpResponseMessage? response = null;


            var backendPath = backendUrl + Route.GetRequestingExtendedPath(request.Path);


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
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }



        public void InitializeRouteAfter()
        {
            RouteFunctions.Add(Route);
            RouteFunctions.Add(Group);
            RouteFunctions.Add(Cache);
            RouteFunctions.Add(Backends);

            foreach (var routeFunction in RouteFunctions)
            {
                var instance = routeFunction as IRouteFunction;
                if (instance == null) continue;
                instance.Initialize();
            }
        }

        public bool IsAllowedToUseRoute(HttpRequest request)
        {
            foreach (var routeFunction in RouteFunctions)
            {
                IRouteFunction func = routeFunction as IRouteFunction;
                if (func == null) continue;
                if (!func.IsAllowedToUse(request)) return false;
            }
            return true;
        }

    }

}
