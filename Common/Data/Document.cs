using System;
using LinqToDB.Mapping;

namespace Common.Data
{
    [Table(Name = "tblDocument")]
    public class Document
    {
        [PrimaryKey, Column(Name = "fldUrl"), NotNull]
        public string Path { get; set; }

        [Column(Name = "fldModifiedDate"), NotNull]
        public DateTime LastModified { get; set; }
    }
}