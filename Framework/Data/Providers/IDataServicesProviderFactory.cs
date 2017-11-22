namespace Framework.Data.Providers
{
    public interface IDataServicesProviderFactory : IDependency {
        IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters);
    }
}