using LinqToDB.Mapping;

namespace Common.Data
{
    [Table(Name = "tblTerm")]
    public class Term
    {
        [PrimaryKey, Column(Name = "fldVal"), NotNull]
        public string Value { get; set; }
    }
}