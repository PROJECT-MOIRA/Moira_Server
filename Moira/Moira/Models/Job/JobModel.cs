using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moira.Models.Job
{
    public class JobModel
    {
        public int job_idx { get; set; }
        public string field { get; set; }
        public string description { get; set; }
        public int people_num { get; set; }
        public int is_deadline { get; set; }
        public string writer { get; set; }
        public string contact { get; set; }
        public string title { get; set; }
    }
}
