using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreSQL.Models
{
    public class FileShareData
    {
        public string FileName { get; set; }
        public string LastModified { get; set; }
        public string Size { get; set; }
    }
}
