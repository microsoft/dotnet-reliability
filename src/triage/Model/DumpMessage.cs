using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps.Model
{
    class DumpMessage
    {
        public string target_os { get; set; }
        public string state { get; set; }
        public string[] result_payload_uris { get; set; }
    }
}
