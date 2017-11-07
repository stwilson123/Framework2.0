namespace Framework.Tasks
{
    public interface IBackgroundService : IDependency {
        void Sweep();
    }

}