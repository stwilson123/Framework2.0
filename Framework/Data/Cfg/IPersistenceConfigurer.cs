namespace Framework.Data.Cfg
{
    public interface IPersistenceConfigurer
    {
        Configuration ConfigureProperties(Configuration ormConfig);
    }
}