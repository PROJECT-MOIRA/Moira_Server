using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moira.Models.Study
{
    public class StudyModel
    {
        public int study_idx { get; set; }
        public string subject { get; set; }
        public int people_num { get; set; }
        public string schedule_description { get; set; }
        public int is_deadline { get; set; }
        public string writer { get; set; }
        public string contact { get; set; }
        public string title { get; set; }
    }
}
