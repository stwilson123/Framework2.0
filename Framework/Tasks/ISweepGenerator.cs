namespace Framework.Tasks
{
    public interface ISweepGenerator : ISingletonDependency {
        void Activate();
        void Terminate();
    }
}