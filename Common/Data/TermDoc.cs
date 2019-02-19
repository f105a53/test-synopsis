using LinqToDB.Mapping;

namespace Common.Data
{
    [Table(Name = "tblTermDoc")]
    public class TermDoc
    {
        [Column(Name = "fldDocUrl")] [NotNull] public string DocumentPath { get; set; }

        [PrimaryKey]
        [Identity]
        [Column(Name = "fldId")]
        [NotNull]
        public string Id { get; set; }

        [Column(Name = "fldLineNo")] [NotNull] public int LineNumber { get; set; }

        [Column(Name = "fldLinePos")]
        [NotNull]
        public int LinePosition { get; set; }

        [Column(Name = "fldTermId")] [NotNull] public string TermId { get; set; }
    }
}