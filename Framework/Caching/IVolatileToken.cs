namespace Framework.Caching {
    public interface IVolatileToken {
        bool IsCurrent { get; }
    }
}