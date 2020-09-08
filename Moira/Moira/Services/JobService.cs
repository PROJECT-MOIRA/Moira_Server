using Moira.Common;
using Moira.DatabBase;
using Moira.Interface;
using Moira.Models.Job;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Moira.Services
{
    public partial class MoiraService : IService
    {
        public DBManager<JobModel> jobDBManager = new DBManager<JobModel>();

        public async Task<Response<List<JobModel>>> GetAllJobs()
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            List<JobModel> tempArr = new List<JobModel>();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                try
                {
                    List<JobModel> jobs = new List<JobModel>();
                    using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                    {
                        db.Open();

                        string selectSql = @"
SELECT
    *
FROM
    job_tb
";
                        jobs = await jobDBManager.GetListAsync(db, selectSql, "");

                        if (jobs != null && jobs.Count > 0)
                        {
                            Console.WriteLine("전체 구인구직 조회 : " + ResponseStatus.OK);
                            var resp = new Response<List<JobModel>> { data = jobs, message = ResponseMessage.OK, status = ResponseStatus.OK };
                            return resp;
                        }
                        else
                        {
                            Console.WriteLine("전체 구인구직 조회 : " + ResponseStatus.NOT_FOUND);
                            return new Response<List<JobModel>> { data = tempArr, message = "구인 구직이 존재하지 않습니다.", status = ResponseStatus.NOT_FOUND };
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("전체 구인구직 조회 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                    Console.WriteLine("GET ALL JOBS ERROR : " + e.Message);
                    return new Response<List<JobModel>> { data = tempArr, message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("전체 구인구직 조회 : " + ResponseStatus.BAD_REQUEST);
                return new Response<List<JobModel>> { data = tempArr, message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> WriteJob(string field, string description, int peopleNum, bool isDeadline, string writer, string contact)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (field != null && description != null && peopleNum.ToString().Length > 0 && isDeadline.ToString().Length > 0
                    && field.Trim().Length > 0 && description.Trim().Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new JobModel();
                            model.field = field;
                            model.description = description;
                            model.people_num = peopleNum;
                            model.is_deadline = isDeadline;
                            model.writer = writer;
                            model.contact = contact;

                            string insertSql = @"
INSERT INTO job_tb(
    field,
    description,
    people_num,
    is_deadline,
    writer,
    contact
)
VALUES(
    @field,
    @description,
    @people_num,
    @is_deadline,
    @writer,
    @contact
);";
                            if (await jobDBManager.InsertAsync(db, insertSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("job_tb", "job_idx"));
                                Console.WriteLine("구인 구직 작성 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("게시글 작성 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("WRITE BULLETIN ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public Task<Response> DeleteJob(int jobIdx)
        {
            throw new NotImplementedException();
        }

        public Task<Response<JobModel>> UpdateJob()
        {
            throw new NotImplementedException();
        }
    }
}
