using Framework.Data.Conventions;

namespace Framework.Tests.Records
{
    public class BigRecord {
        public virtual int Id { get; set; }
        [StringLengthMax]
        public virtual string Body { get; set; }

        [StringLengthMax]
        public virtual string Banner { get; set; }
    }
}