using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BroomStick.DataClasses
{
    public class BroomStick
    {
        public static List<RouteObject> RouteObjects { get; } = new List<RouteObject>();
        public static Authenticator Authenticator { get; } = new Authenticator();
        public BroomStick()
        {
            this.LoadRoutes();
        }

        public void LoadRoutes()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string? directoryPath = configuration["RoutesDirectory"];
            if (string.IsNullOrEmpty(directoryPath))
            {
                Console.WriteLine("Error: DirectoryPath not specified in appsettings.json");
                return;
            }

            List<string> jsonFilePaths = GetJsonFilePaths(directoryPath);

            foreach (string jsonFilePath in jsonFilePaths)
            {
                string json = File.ReadAllText(jsonFilePath);
                JObject data = JObject.Parse(json);

                JObject defaultJson = (JObject)(data["default"] ?? new JObject());

                if (data["routes"] is JArray routesJson)
                {
                    foreach (JObject routeJson in routesJson)
                    {
                        JObject mergedJson = MergeJsonObjects((JObject)defaultJson.DeepClone(), routeJson);
                        var routeObject = JsonConvert.DeserializeObject<RouteObject>(mergedJson.ToString());
                        if (routeObject == null) continue;
                        routeObject.InitializeRouteAfter();
                        RouteObjects.Add(routeObject);
                    }
                }
            }
        }

        static List<string> GetJsonFilePaths(string directoryPath)
        {
            List<string> jsonFilePaths = new List<string>();

            try
            {
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    if (Path.GetExtension(filePath) == ".json")
                    {
                        jsonFilePaths.Add(filePath);
                    }
                }

                foreach (string directory in Directory.GetDirectories(directoryPath))
                {
                    List<string> subDirectoryJsonFilePaths = GetJsonFilePaths(directory);
                    jsonFilePaths.AddRange(subDirectoryJsonFilePaths);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return jsonFilePaths;
        }

        public JObject MergeJsonObjects(JObject defaultJson, JObject targetJson)
        {
            defaultJson.Merge(targetJson, new JsonMergeSettings
            {
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                MergeArrayHandling = MergeArrayHandling.Replace
            });

            return defaultJson;
        }

    }

}
