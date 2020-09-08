namespace Moira.Models
{
    public partial class MemberModel
    {
        public string token { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string grade { get; set; }
        public string contact { get; set; }
        public string email { get; set; }
    }

    public class Member : MemberModel
    {
        public string pw { get; set; }
    }
}
