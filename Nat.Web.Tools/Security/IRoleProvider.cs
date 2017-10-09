using System.Collections.Generic;

namespace Nat.Web.Tools.Security
{
    public interface IRoleProvider
    {
        IDictionary<string, string> GetAllRolesWithDescription();
        string GetRoleDescription(string roleName);
        bool UserIsWebAdmin { get; }
        void CreateRole(string roleName, string description, string roleGroupName, bool isGroupOfPermissions);
        IEnumerable<KeyValuePair<string, string>> GetRoleGroupsWithDescription(string roleGroupName);
        IEnumerable<string> GetRoles(string roleGroupName);
        string[] GetAssignedRolesForUser(string username);
    }
}