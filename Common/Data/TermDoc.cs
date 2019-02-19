using LinqToDB.Mapping;
using System;

namespace Common.Data
{
    [Table(Name = "tblTermDoc")]
    public class TermDoc
    {
        [PrimaryKey, Column(Name = "fldId"), NotNull]
        public Guid Id { get; set; }

        [Column(Name = "fldDocUrl"), NotNull]
        public string DocumentPath { get; set; }

        [Column(Name = "fldLineNo"), NotNull]
        public int LineNumber { get; set; }

        [Column(Name = "fldLinePos"), NotNull]
        public int LinePosition { get; set; }

        [Column(Name = "fldTermId"), NotNull]
        public string TermId { get; set; }
    }
}