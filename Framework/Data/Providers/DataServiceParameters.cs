using System.Collections.Generic;
using System.Linq;
using Framework.Environment.ShellBuilder.Models;

namespace Framework.Data.Providers
{
    public class DataServiceParameters {
        public string Provider { get; set; }
        public string DataFolder { get; set; }
        public string ConnectionString { get; set; }
    }
}