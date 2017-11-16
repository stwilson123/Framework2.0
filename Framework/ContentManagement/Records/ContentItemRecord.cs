using System.Collections.Generic;
using Framework.Data.Conventions;
using Framework.Environment.FieldStorage;

namespace Framework.ContentManagement.Records
{
    public class ContentItemRecord {
        public ContentItemRecord() {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Versions = new List<ContentItemVersionRecord>();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
            Infoset = new Infoset();
        }

        public virtual int Id { get; set; }
        public virtual ContentTypeRecord ContentType { get; set; }
        public virtual IList<ContentItemVersionRecord> Versions { get; set; }

        [StringLengthMax]
        public virtual string Data { get { return Infoset.Data; } set { Infoset.Data = value; } }
        public virtual Infoset Infoset { get; protected set; }
    }
}