namespace Framework.Data.Cfg
{
    public class PersistenceConfigurer : IPersistenceConfigurer
    {
        public PersistenceConfigurer(string connectString, string providerName)
        {
            _connectString = connectString;
            _providerName = providerName;
        }

        private string _connectString { get; set; }
        private string _providerName { get; set; }

        public Configuration ConfigureProperties(Configuration ormConfig)
        {
            if (ormConfig == null)
                return new Configuration(_connectString,_providerName);
            return ormConfig;
        }
    }
}