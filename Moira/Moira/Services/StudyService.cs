using Moira.Common;
using Moira.DatabBase;
using Moira.Interface;
using Moira.Models.Study;
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
        public DBManager<StudyModel> studyDBManager = new DBManager<StudyModel>();

        #region Study_Service
        public async Task<Response<List<StudyModel>>> GetAllStudies()
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            List<StudyModel> tempArr = new List<StudyModel>();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                try
                {
                    List<StudyModel> jobs = new List<StudyModel>();
                    using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                    {
                        db.Open();

                        string selectSql = @"
SELECT
    *
FROM
    study_tb
";
                        jobs = await studyDBManager.GetListAsync(db, selectSql, "");

                        if (jobs != null && jobs.Count > 0)
                        {
                            Console.WriteLine("전체 스터디 게시글 조회 : " + ResponseStatus.OK);
                            var resp = new Response<List<StudyModel>> { data = jobs, message = ResponseMessage.OK, status = ResponseStatus.OK };
                            return resp;
                        }
                        else
                        {
                            Console.WriteLine("전체 스터디 게시글 조회 : " + ResponseStatus.NOT_FOUND);
                            return new Response<List<StudyModel>> { data = tempArr, message = "구인 구직 게시글이 존재하지 않습니다.", status = ResponseStatus.NOT_FOUND };
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("전체 스터디 게시글 조회 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                    Console.WriteLine("GET ALL STUDIES ERROR : " + e.Message);
                    return new Response<List<StudyModel>> { data = tempArr, message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("전체 스터디 게시글 조회 : " + ResponseStatus.BAD_REQUEST);
                return new Response<List<StudyModel>> { data = tempArr, message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> WriteStudy(string subject, int people_num, string schedule_description, int is_deadline, string writer, string contact, string title)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (subject != null && subject.ToString().Length > 0 && people_num.ToString().Length > 0 && title != null && title.Length > 0 &&
                    schedule_description != null && schedule_description.Length > 0 && is_deadline.ToString().Length > 0 && writer != null && contact != null)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new StudyModel();
                            model.subject = subject;
                            model.people_num = people_num;
                            model.schedule_description = schedule_description;
                            model.is_deadline = is_deadline;
                            model.writer = writer;
                            model.contact = contact;
                            model.title = title;

                            string insertSql = @"
INSERT INTO study_tb(
    subject,
    schedule_description,
    people_num,
    is_deadline,
    writer,
    contact,
    title
)
VALUES(
    @subject,
    @schedule_description,
    @people_num,
    @is_deadline,
    @writer,
    @contact,
    @title
);";
                            if (await jobDBManager.InsertAsync(db, insertSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("study_tb", "study_idx"));
                                Console.WriteLine("스터디 게시글 작성 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("스터디 게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("스터디 게시글 작성 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("WRITE STUDY ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("스터디 게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("스터디 게시글 작성 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> DeleteStudy(string writer, int study_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (writer != null && writer.Length > 0 && study_idx.ToString() != null && writer.ToString().Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new StudyModel();
                            model.study_idx = study_idx;
                            model.writer = writer;

                            string deleteSql = $@"
DELETE FROM
    study_tb
WHERE
    writer = '{writer}'
AND
    study_idx = '{study_idx}'    
;";
                            if (await jobDBManager.DeleteAsync(db, deleteSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("job_tb", "job_idx"));
                                Console.WriteLine("스터디 게시글 삭제 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("스터디 게시글 삭제 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("스터디 게시글 삭제 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("DELETE STUDY ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("스터디 게시글 삭제 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("스터디 게시글 삭제 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> UpdateStudy(string subject, int people_num, string schedule_description, string writer, string contact, int is_deadline, int study_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (subject != null && subject.Trim().Length > 0 && schedule_description != null && schedule_description.Trim().Length > 0 &&
                    people_num.ToString().Length > 0 && is_deadline.ToString().Length > 0 && writer != null && writer.Length > 0 && study_idx.ToString().Length > 0 &&
                    contact != null && contact.Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new StudyModel();
                            model.subject = subject;
                            model.people_num = people_num;
                            model.schedule_description = schedule_description;
                            model.writer = writer;
                            model.contact = contact;
                            model.is_deadline = is_deadline;
                            model.study_idx = study_idx;

                            string updateSql = $@"
UPDATE 
    study_tb
SET
    subject = '{subject}',
    people_num = '{people_num}',
    schedule_description = '{schedule_description}',
    is_deadline = '{is_deadline}',
    contact = '{contact}'
WHERE
    writer = '{writer}'
AND
    study_idx = '{study_idx}'
;";
                            if (await studyDBManager.UpdateAsync(db, updateSql, model) == 1)
                            {
                                await studyDBManager.IndexSortSqlAsync(db, updateSql);
                                Console.WriteLine("스터디 게시글 수정 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("스터디 게시글 수정 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("스터디 게시글 수정 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("STUDY UPDATE ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("스터디 게시글 수정 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("스터디 게시글 수정 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> SetStudyDeadLine(string writer, int is_deadline, int study_idx)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (writer != null && writer.Length > 0 && is_deadline.ToString() != null && is_deadline.ToString().Length > 0 && study_idx.ToString() != null && study_idx.ToString().Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new StudyModel();
                            model.writer = writer;
                            model.study_idx = study_idx;
                            model.is_deadline = is_deadline;

                            string updateSql = $@"
UPDATE 
    study_tb
SET
    is_deadline = '{is_deadline}'
WHERE
    writer = '{writer}'
AND
    study_idx = '{study_idx}'
;";
                            if (await jobDBManager.UpdateAsync(db, updateSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, updateSql);
                                Console.WriteLine("특정 스터디 게시글 마감여부 수정 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("특정 스터디 게시글 마감여부 수정 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("특정 스터디 게시글 마감여부 수정 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("SEPECIFIC STUDY UPDATE ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("특정 스터디 게시글 마감여부 수정 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else
            {
                Console.WriteLine("특정 스터디 게시글 마감여부 수정 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }
        #endregion
    }
}
