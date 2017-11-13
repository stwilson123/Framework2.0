using Autofac.Configuration;
using Microsoft.Extensions.Configuration;

namespace Framework.AutofacExtend.Config
{
    public class ConfigurationSettingsReaderFactory  
    {
//        public static ConfigurationModule CreateConfigurationSettingsReader(string fileName)
//        {
//            // Add the configuration to the ConfigurationBuilder.
//            var config = new ConfigurationBuilder();
//            config.AddXmlFile(fileName);
//          
//            return new ConfigurationModule(config.Build());
//        }
//        
//        public static ConfigurationModule CreateConfigurationSettingsReader()
//        {
//            // Add the configuration to the ConfigurationBuilder.
//            var config = new ConfigurationBuilder();
//        
//          
//            return new ConfigurationModule(config.Build());
//        }
        public static ConfigurationModule CreateConfigurationSettingsReader(string fileName)
        {
           
          
            return default(ConfigurationModule);
        }
        
        public static ConfigurationModule CreateConfigurationSettingsReader()
        {
            return default(ConfigurationModule);

        }
        
    }
}