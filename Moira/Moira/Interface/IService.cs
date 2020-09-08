using Moira.Models;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Moira.Interface
{
    [ServiceContract]
    public partial interface IService
    {
        #region Member_Service
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/auth/register")]
        [return: MessageParameter(Name = "SignUp")]
        Task<Response> SignUp(string id, string pw, string name, string grade, string contact, string email);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/auth/login")]
        [return: MessageParameter(Name = "Login")]
        Task<Response<MemberModel>> Login(string id, string pw);
        #endregion


    }

    public class Response
    {
        public string message { get; set; }
        public int status { get; set; }
    }

    public class Response<T>
    {
        public string message { get; set; }
        public int status { get; set; }
        public T data { get; set; }
    }
}
