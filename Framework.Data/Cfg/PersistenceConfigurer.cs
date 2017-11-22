namespace Framework.Data.Cfg
{
    public class PersistenceConfigurer : IPersistenceConfigurer
    {
        public Configuration ConfigureProperties(Configuration ormConfig)
        {
            return ormConfig;
        }
    }
}