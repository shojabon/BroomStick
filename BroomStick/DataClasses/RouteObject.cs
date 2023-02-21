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

namespace BroomStick.DataClasses
{
    public class RouteObject
    {
        public RoutesFunction Routes { get; set; }
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

            foreach (var header in request.Headers)
            {
                if (!header.Key.StartsWith("x-")) continue;
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value.ToArray());
            }


            // json body
            string json = "{}";
            if(request.HasFormContentType)
            {
                Dictionary<string, string> formData = request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                json = JsonConvert.SerializeObject(formData);
            }
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            switch (request.Method)
            {
                case "POST":
                    response = await httpClient.PostAsync(backendUrl + request.Path, content);
                    break;
                case "PUT":
                    response = await httpClient.PutAsync(backendUrl + request.Path, content);
                    break;
                case "GET":
                    response = await httpClient.GetAsync(backendUrl + request.Path);
                    break;
                case "DELETE":
                    response = await httpClient.DeleteAsync(backendUrl + request.Path);
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
            RouteFunctions.Add(Routes);
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
