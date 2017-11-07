namespace Framework.Tasks
{
    public interface IBackgroundTask : IDependency {
        void Sweep();
    }
}