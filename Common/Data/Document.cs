using LinqToDB.Mapping;

namespace Common.Data
{
    [Table(Name = "tblDocument")]
    public class Document
    {
        [Column(Name = "fldModifiedDate")]
        [NotNull]
        public long LastModified { get; set; }

        [PrimaryKey]
        [Identity]
        [Column(Name = "fldUrl")]
        [PrimaryKey]
        [Identity]
        public string Path { get; set; }
    }
}