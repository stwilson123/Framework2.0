using Framework.ContentManagement;
using Framework.Security.Permissions;

namespace Framework.Security
{
    /// <summary>
    /// Entry-point for configured authorization scheme. Role-based system
    /// provided by default. 
    /// </summary>
    public interface IAuthorizationService : IDependency {
        void CheckAccess(Permission permission, IUser user, IContent content);
        bool TryCheckAccess(Permission permission, IUser user, IContent content);
    }
}