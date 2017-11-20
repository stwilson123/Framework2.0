using System.IO;

namespace Framework.Data.Bags.Serialization
{
    public interface IBagSerializer : IDependency {
        void Serialize(TextWriter tw, Bag o);
        Bag Deserialize(TextReader tr);
    }
}