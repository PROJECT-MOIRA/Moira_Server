using Moira.Models;
using Moira.Models.Job;
using Moira.Models.Study;
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
        /// <summary>
        /// 회원가입 API
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pw"></param>
        /// <param name="grade"></param>
        /// <param name="contact"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/auth/register")]
        Task<Response> SignUp(string id, string pw, string grade, string contact, string name, string email);

        /// <summary>
        /// 로그인 API
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/auth/login")]
        Task<Response<MemberModel>> Login(string id, string pw);
        #endregion

        #region Job_Service
        /// <summary>
        /// 전체 구인구직 게시글 조회 API
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/job")]
        Task<Response<List<JobModel>>> GetAllJobs();

        /// <summary>
        /// 구인구직 게시글 작성 API
        /// </summary>
        /// <param name="field"></param>
        /// <param name="description"></param>
        /// <param name="people_num"></param>
        /// <param name="is_deadline"></param>
        /// <param name="writer"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/job")]
        Task<Response> WriteJob(string field, string description, int people_num, bool is_deadline, string writer, string contact);

        /// <summary>
        /// 구인구직 게시글 삭제 API
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="job_idx"></param>
        /// <returns></returns>
        [WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/job")]
        Task<Response> DeleteJob(string writer, int job_idx);

        /// <summary>
        /// 구인구직 게시글 수정 API
        /// </summary>
        /// <param name="field"></param>
        /// <param name="description"></param>
        /// <param name="peopleNum"></param>
        /// <param name="is_deadline"></param>
        /// <param name="wrtier"></param>
        /// <param name="contact"></param>
        /// <param name="job_idx"></param>
        /// <returns></returns>
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/job")]
        Task<Response> UpdateJob(string field, string description, int people_num, bool is_deadline, string wrtier, string contact, int job_idx);
        #endregion


        #region Study_Service
        /// <summary>
        /// 전체 스터디 게시물 조회
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/study")]
        Task<Response<List<StudyModel>>> GetAllStudies();

        /// <summary>
        /// 스터디 게시물 작성 API
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="people_num"></param>
        /// <param name="schedule_description"></param>
        /// <param name="is_deadline"></param>
        /// <param name="writer"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/study")]
        Task<Response> WriteStudy(string subject, int people_num, string schedule_description, bool is_deadline, string writer, string contact);

        ///// <summary>
        ///// 스터디 게시물 삭제 API
        ///// </summary>
        ///// <param name="writer"></param>
        ///// <param name="study_idx"></param>
        ///// <returns></returns>
        [WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/study")]
        Task<Response> DeleteStudy(string writer, int study_idx);

        /// <summary>
        /// 스터디 게시물 수정 API
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="people_num"></param>
        /// <param name="schedule_description"></param>
        /// <param name="writer"></param>
        /// <param name="contact"></param>
        /// <param name="is_deadline"></param>
        /// <param name="study_idx"></param>
        /// <returns></returns>
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/study")]
        Task<Response> UpdateStudy(string subject, int people_num, string schedule_description, string writer, string contact, bool is_deadline, int study_idx);
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
