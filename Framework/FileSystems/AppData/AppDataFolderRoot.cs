using System.Web.Hosting;

namespace Framework.FileSystems.AppData
{
    public class AppDataFolderRoot : IAppDataFolderRoot {
        public string RootPath {
            get { return "~/App_Data"; }
        }

        public string RootFolder {
            get { return HostingEnvironment.MapPath(RootPath); }
        }
    }
}