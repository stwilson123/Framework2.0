using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace Framework.Data.Cfg
{
    [Serializable]
    public class Configuration : ISerializable
    {
        public const string DefaultCfgFileName = "database.cfg.xml";

        private  ConnectionStringSettings _connectStringSetting;
        
        public Configuration(ConnectionStringSettings connectStringSetting)
        {
            _connectStringSetting = connectStringSetting;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        
        
       
    }
}