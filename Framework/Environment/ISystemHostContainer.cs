namespace Framework.Environment
{
    public interface ISystemHostContainer {
        T Resolve<T>();
    }
}