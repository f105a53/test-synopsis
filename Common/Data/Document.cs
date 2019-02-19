using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace Common.Data
{
    [Table(Name = "tblDocument")]
    public class Document
    {
        [PrimaryKey, Identity]
        [Column(Name = "fldUrl"), PrimaryKey, Identity]
        public string Path { get; set; }

        [Column(Name = "fldModifiedDate"), NotNull]
        public long LastModified { get; set; }
    }
}
