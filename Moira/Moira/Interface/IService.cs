using Moira.Models;
using Moira.Models.Job;
using System.Collections.Generic;
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

        #region Job_Service
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/job")]
        [return: MessageParameter(Name = "Job")]
        Task<Response<List<JobModel>>> GetAllJobs();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="description"></param>
        /// <param name="peopleNum"></param>
        /// <param name="isDeadline"></param>
        /// <param name="writer"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/job")]
        [return: MessageParameter(Name = "Write Job")]
        Task<Response> WriteJob(string field, string description, int peopleNum, bool isDeadline, string writer, string contact);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobIdx"></param>
        /// <returns></returns>
        [WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/job")]
        [return: MessageParameter(Name = "Delete Job")]
        Task<Response> DeleteJob(int jobIdx);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/job")]
        [return: MessageParameter(Name = "Update Job")]
        Task<Response<JobModel>> UpdateJob();
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
