using Framework.Data.Cfg;

namespace Framework.Data.Providers
{
    public interface IDataServicesProvider : ITransientDependency {
         Configuration BuildConfiguration(SessionFactoryParameters sessionFactoryParameters);
         IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);
    }
}