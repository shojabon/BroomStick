using System.Text.RegularExpressions;

namespace BroomStick.DataClasses.Functions
{
    public class RoutesFunction : IRouteFunction
    {

        public List<string> Paths { get; set; }
        public bool RemoveRequestPrefix { get; set; }

        public void Initialize()
        {
            if (Paths != null)
            {
                Paths = Paths.OrderByDescending(s => s.Length).ToList();
            }
            else
            {
                Paths = new();
            }
        }

        public bool IsAllowedToUse(HttpRequest request)
        {
            Console.WriteLine("testa");
            return MatchesRoute(request.Path);
        }

        public bool MatchesRoute(string requestPath)
        {
            if (Paths == null || Paths.Count == 0)
            {
                return false;
            }
            string route = CleanPath(requestPath);

            foreach (var r in Paths)
            {
                var pattern = Regex.Replace(Regex.Escape(r), @"\<.*?\>", ".*");
                var match = Regex.Match(route, "^" + pattern + "$");
                Console.WriteLine(pattern + " " + route);
                if (match.Success)
                {
                    return true;
                }
            }

            return false;
        }


        private string CleanPath(string path)
        {
            // Remove path parameters if they exist
            int pathParamIndex = path.IndexOf('?');
            if (pathParamIndex != -1)
            {
                path = path.Substring(0, pathParamIndex);
            }

            // Remove trailing slashes
            while (path.EndsWith('/'))
            {
                path = path.Substring(0, path.Length - 1);
            }
            while (!path.StartsWith("/"))
            {
                path = "/" + path;
            }
            return path;
        }

    }
}
