using LinqToDB.Mapping;

namespace Common.Data
{
    [Table(Name = "tblTerm")]
    public class Term
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "fldVal")]
        [NotNull]
        public string Value { get; set; }
    }
}