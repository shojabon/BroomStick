namespace BroomStick.DataClasses.Functions
{
    public class GroupFunction : IRouteFunction
    {

        public List<string> AllowedGroups { get; set; }

        public APIResponse IsAllowedToUse(HttpRequest request)
        {
            var userObject = Authenticator.Authenticator.GetUser(request.Headers["Authenticate"]);
            var PermissionLevel = 0;
            if (userObject != null)
            {
                PermissionLevel = GetGroupPermissionLevel(userObject.GetGroup());
            }

            bool Allowed = false;
            foreach(var group in AllowedGroups)
            {
                if(PermissionLevel >= GetGroupPermissionLevel(group))
                {
                    Allowed = true;
                    break;
                }
            }
            if(!Allowed) return CommonAPIResponse.UnAuthorized;
            return CommonAPIResponse.Success;
        }

        private int GetGroupPermissionLevel(string Group)
        {
            int PermissionLevel = 0;
            if (BroomStick.Configuration == null) return PermissionLevel;
            var Defenition = BroomStick.Configuration.GetSection("GroupDefinition").Get<string[]>(); ;
            if (Defenition == null) return PermissionLevel;
            if (!Defenition.Contains(Group)) return PermissionLevel; 
            PermissionLevel = Defenition.Length - Array.IndexOf(Defenition, Group) - 1;
            return PermissionLevel;
        }
    }
}
