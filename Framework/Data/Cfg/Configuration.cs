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


        public string ConnectString { get; }
        public string ProviderName { get; }

        public Configuration(string connectString,string providerName)
        {
            ConnectString = connectString;
            ProviderName = providerName;
        }
        

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        
        
       
    }
}