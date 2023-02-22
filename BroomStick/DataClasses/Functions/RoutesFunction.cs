using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace BroomStick.DataClasses.Functions
{
    public class RoutesFunction : IRouteFunction
    {

        public string Path { get; set; }
        public bool RemoveRequestPrefix { get; set; }


        public bool IsAllowedToUse(HttpRequest request)
        {
            return MatchesPath(request.Path);
        }

        public bool MatchesPath(string requestPath)
        {
            if (Path == null)
            {
                return false;
            }
            string route = CleanPath(requestPath);

            var pattern = Regex.Replace(Regex.Escape(Path), @"\<.*?\>", ".*");
            var match = Regex.Match(route, "^" + pattern + ".*$");
            if (match.Success)
            {
                return true;
            }

            return false;
        }

        public string GetRequestingExtendedPath(string path)
        {
            if (!RemoveRequestPrefix)
            {
                return path;
            }
            var pattern = Regex.Replace(Regex.Escape(Path), @"\<.*?\>", ".*");
            return Regex.Replace(path, pattern, "");
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
