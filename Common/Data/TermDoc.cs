using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using LinqToDB.Mapping;

namespace Common.Data
{
    [LinqToDB.Mapping.Table( Name = "tblTermDoc")]
    public class TermDoc
    {
        [PrimaryKey, Identity]
        [Column(Name = "fldId"), NotNull]
        public string Id { get; set; }

        [Column( Name = "fldDocUrl"), NotNull]
        public string DocumentPath { get; set; }

        [Column( Name = "fldTermId"), NotNull]
        public string TermId { get; set; }

        [Column(Name = "fldLinePos"), NotNull]
        public int LinePosition { get; set; }

        [Column(Name = "fldLineNo"), NotNull]
        public int LineNumber { get; set; }
    }
}
