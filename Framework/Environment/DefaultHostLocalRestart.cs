using System;
using Framework.Caching;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor;
using Framework.Environment.Descriptor.Models;
using Framework.Exceptions;
using Framework.FileSystems.AppData;
using Framework.Logging;

namespace Framework.Environment
{
    public class DefaultHostLocalRestart : IHostLocalRestart, IShellDescriptorManagerEventHandler, IShellSettingsManagerEventHandler {
        private readonly IAppDataFolder _appDataFolder;
        private const string fileName = "hrestart.txt";

        public DefaultHostLocalRestart(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Monitor(Action<IVolatileToken> monitor) {
            if (!_appDataFolder.FileExists(fileName))
                TouchFile();

            Logger.Debug("Monitoring virtual path \"{0}\"", fileName);
            monitor(_appDataFolder.WhenPathChanges(fileName));
        }

        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings) {
            //TouchFile();
        }

        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {
            //TouchFile();
        }

        private void TouchFile() {
            try {
                _appDataFolder.CreateFile(fileName, "Host Restart");
            }
            catch(Exception ex) {
                if (ex.IsFatal()) {
                    throw;
                } 
                Logger.Warning(ex, "Error updating file '{0}'", fileName);
            }
        }
    }
}