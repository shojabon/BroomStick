namespace BroomStick.DataClasses.Functions
{
    public class BackendsFunction : IRouteFunction
    {

        public List<string> Endpoints { get; set; }

        public APIResponse IsAllowedToUse(HttpRequest request)
        {
            if(Endpoints == null || Endpoints.Count == 0)
            {
                return CommonAPIResponse.RouteNotFound;
            }
            return CommonAPIResponse.Success;
        }
    }
}
