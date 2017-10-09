using System.Collections.Generic;
using System.Web.Security;

namespace Nat.Web.Tools.Security
{
    public interface IGroupProvider
    {
        IEnumerable<string> GetGroupsForUser(string userName);
        IEnumerable<string> GetAllGroups();
        IEnumerable<MembershipUser> GetUsersForGroup(string groupName);
        IEnumerable<string> GetRolesForGroup(string groupName);
        IEnumerable<KeyValuePair<string, string>> GetAllGroupsWithDescription();
        IEnumerable<KeyValuePair<string, string>> GetGroupsForUserWithDescription(string userName);
        bool CanCurrentUserViewMembership(string groupName);
        bool CanCurrentUserEditMembership(string groupName);
        void RemoveUsersFromGroups(IEnumerable<string> userNames, IEnumerable<string> groupNames);
        void AddUsersInGroups(IEnumerable<string> userNames, IEnumerable<string> groupNames);
        void RemoveRolesFromGroups(IEnumerable<string> roleNames, IEnumerable<string> groupNames);
        void AddRolesToGroups(IEnumerable<string> roleNames, IEnumerable<string> groupNames);
        void AddGroup(string groupName, string groupDescription, bool viewOnlyWhoInGroup, bool editOnlyOwner, string ownerUserName, string ownerGroupName);
    }
}