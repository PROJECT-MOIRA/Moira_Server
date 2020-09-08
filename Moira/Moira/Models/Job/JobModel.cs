using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moira.Models.Job
{
    public class JobModel
    {
        public string field { get; set; }
        public string description { get; set; }
        public int people_num { get; set; }
        public bool is_deadline { get; set; }
        public string writer { get; set; }
        public string contact { get; set; }
    }
}
