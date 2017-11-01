using System.ComponentModel;

namespace Framework.Settings
{
    public interface ISiteService : IDependency {
        ISite GetSiteSettings();
    }
}