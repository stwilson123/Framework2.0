namespace Framework.UI.Resources
{
    public interface IResourceFileHashProvider : ISingletonDependency {
        string GetResourceFileHash(string physicalPath);
    }
}