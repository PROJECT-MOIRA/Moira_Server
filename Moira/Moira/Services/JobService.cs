using Moira.Common;
using Moira.DatabBase;
using Moira.Interface;
using Moira.Models.Job;
using Moira.Models.JobJoin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Moira.Services
{
    public partial class MoiraService : IService
    {
        public DBManager<JobModel> jobDBManager = new DBManager<JobModel>();

        #region Job_Service
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
                            Console.WriteLine("전체 구인구직 게시글 조회 : " + ResponseStatus.OK);
                            var resp = new Response<List<JobModel>> { data = jobs, message = ResponseMessage.OK, status = ResponseStatus.OK };
                            return resp;
                        }
                        else
                        {
                            Console.WriteLine("전체 구인구직 게시글 조회 : " + ResponseStatus.NOT_FOUND);
                            return new Response<List<JobModel>> { data = tempArr, message = "구인 구직 게시글이 존재하지 않습니다.", status = ResponseStatus.NOT_FOUND };
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("전체 구인구직 게시글 조회 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                    Console.WriteLine("GET ALL JOBS ERROR : " + e.Message);
                    return new Response<List<JobModel>> { data = tempArr, message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("전체 구인구직 게시글 조회 : " + ResponseStatus.BAD_REQUEST);
                return new Response<List<JobModel>> { data = tempArr, message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> WriteJob(string field, string description, int people_num, int isDeadline, string writer, string contact, string title)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (field != null && description != null && people_num.ToString().Length > 0 && isDeadline.ToString().Length > 0
                    && field.Trim().Length > 0 && description.Trim().Length > 0 && title != null && title.Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new JobModel();
                            model.field = field;
                            model.description = description;
                            model.people_num = people_num;
                            model.is_deadline = isDeadline;
                            model.writer = writer;
                            model.contact = contact;
                            model.title = title;

                            string insertSql = @"
INSERT INTO job_tb(
    field,
    description,
    people_num,
    is_deadline,
    writer,
    contact,
    title
)
VALUES(
    @field,
    @description,
    @people_num,
    @is_deadline,
    @writer,
    @contact,
    @title
);";
                            if (await jobDBManager.InsertAsync(db, insertSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("job_tb", "job_idx"));
                                Console.WriteLine("구인구직 게시글 작성 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("구인구직 게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("구인구직 게시글 작성 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("WRITE JOB ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("구인구직 게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("구인구직 게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> DeleteJob(string writer, int job_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (job_idx.ToString() != null && job_idx.ToString().Length > 0 && writer != null && writer.Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new JobModel();
                            model.job_idx = job_idx;
                            model.writer = writer;

                            string deleteSql = $@"
DELETE FROM
    job_tb
WHERE
    writer = '{writer}'
AND
    job_idx = '{job_idx}'    
;";
                            if (await jobDBManager.DeleteAsync(db, deleteSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("job_tb", "job_idx"));
                                Console.WriteLine("구인구직 게시글 삭제 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("구인구직 게시글 삭제 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("구인구직 게시글 삭제 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("WRITE JOB ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("구인구직 게시글 삭제 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("구인구직 게시글 삭제 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> UpdateJob(string field, string description, int people_num, int is_deadline, string writer, string contact, int job_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (field != null && field.Trim().Length > 0 && description != null && description.Trim().Length > 0 &&
                    people_num.ToString().Length > 0 && is_deadline.ToString().Length > 0 && writer != null && writer.Length > 0 &&
                    contact != null && contact.Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new JobModel();
                            model.job_idx = job_idx;
                            model.field = field;
                            model.description = description;
                            model.people_num = people_num;
                            model.is_deadline = is_deadline;
                            model.writer = writer;
                            model.contact = contact;

                            string updateSql = $@"
UPDATE 
    job_tb
SET
    field = '{field}',
    description = '{description}',
    people_num = '{people_num}',
    is_deadline = '{is_deadline}',
    contact = '{contact}'
WHERE
    writer = '{writer}'
AND
    job_idx = '{job_idx}'
;";
                            if (await jobDBManager.UpdateAsync(db, updateSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, updateSql);
                                Console.WriteLine("구인구직 게시글 수정 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("구인구직 게시글 수정 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("구인구직 게시글 수정 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("JOB UPDATE ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("구인구직 게시글 수정 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("구인구직 게시글 수정 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> SetJobDeadLine(string writer, int is_deadline, int job_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (is_deadline.ToString() != null && is_deadline.ToString().Length > 0 && job_idx.ToString() != null && job_idx.ToString().Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new JobModel();
                            model.writer = writer;
                            model.job_idx = job_idx;
                            model.is_deadline = is_deadline;

                            string updateSql = $@"
UPDATE 
    job_tb
SET
    is_deadline = '{is_deadline}'
WHERE
    writer = '{writer}'
AND
    job_idx = '{job_idx}'
;";
                            if (await jobDBManager.UpdateAsync(db, updateSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, updateSql);
                                Console.WriteLine("특정 구인구직 게시글 마감여부 수정 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("특정 구인구직 게시글 마감여부 수정 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("특정 구인구직 게시글 마감여부 수정 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("SEPECIFIC JOB UPDATE ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("특정 구인구직 게시글 마감여부 수정 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else
            {
                Console.WriteLine("특정 구인구직 게시글 마감여부 수정 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }


        public async Task<Response> ParticipateJob(int member_idx, int job_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (member_idx.ToString().Length > 0 && job_idx.ToString().Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new JobJoinModel();
                            model.job_idx = job_idx;
                            model.member_idx = member_idx;

                            string insertSql = @"
INSERT INTO job_join_tb(
    member_idx,
    job_idx,
)
VALUES(
    @member_idx,
    @job_idx
);";

                            if (await jobDBManager.UpdateAsync(db, insertSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, insertSql);
                                Console.WriteLine("구인구직 참여 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("구인구직 참여 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("구인구직 참여 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("JOB PARTICIPATE ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("구인구직 참여 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else
            {
                Console.WriteLine("구인구직 참여 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }
        #endregion
    }
}
