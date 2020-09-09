using Moira.Common;
using Moira.DatabBase;
using Moira.Interface;
using Moira.Models.Portfolio;
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
        public DBManager<PortfolioModel> portfolioDBManager = new DBManager<PortfolioModel>();

        #region Portfolio_Service
        public async Task<Response<List<PortfolioModel>>> GetPortfolioInfos(string writer)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            List<PortfolioModel> tempArr = new List<PortfolioModel>();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (writer != null && writer.Length > 0)
                {
                    try
                    {
                        List<PortfolioModel> jobs = new List<PortfolioModel>();
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            string selectSql = $@"
SELECT
    *
FROM
    portfolio_tb
WHERE
    writer = '{writer}'
";
                            jobs = await portfolioDBManager.GetListAsync(db, selectSql, "");

                            if (jobs != null && jobs.Count > 0)
                            {
                                Console.WriteLine("포트폴리오 정보 조회 : " + ResponseStatus.OK);
                                var resp = new Response<List<PortfolioModel>> { data = jobs, message = ResponseMessage.OK, status = ResponseStatus.OK };
                                return resp;
                            }
                            else
                            {
                                Console.WriteLine("포트폴리오 정보 조회 : " + ResponseStatus.NOT_FOUND);
                                return new Response<List<PortfolioModel>> { data = tempArr, message = "구인 구직 게시글이 존재하지 않습니다.", status = ResponseStatus.NOT_FOUND };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("포트폴리오 정보 조회 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("GET ALL PORTFOLIO INFOS ERROR : " + e.Message);
                        return new Response<List<PortfolioModel>> { data = tempArr, message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("포트폴리오 정보 조회 : " + ResponseStatus.BAD_REQUEST);
                    return new Response<List<PortfolioModel>> { data = tempArr, message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("포트폴리오 정보 조회 : " + ResponseStatus.BAD_REQUEST);
                return new Response<List<PortfolioModel>> { data = tempArr, message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response> WritePortfolio(string writer, string description, string github, string blog, string rocketpunch)
        {
            WebOperationContext webOperationContext = WebOperationContext.Current;
            string requestHeaderValue = webOperationContext.IncomingRequest.Headers["token"].ToString();

            // Header에 토큰 값이 제대로 들어왔는지 확인 & 토큰이 유효한지 확인
            if (!(requestHeaderValue == null) && ComDef.jwtService.IsTokenValid(requestHeaderValue) == true)
            {
                if (description != null && description.Length > 0 && github != null && github.Length > 0
                    && blog != null && blog.Length > 0 && rocketpunch != null && rocketpunch.Length > 0
                    && writer != null && writer.Length > 0)
                {
                    try
                    {
                        using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                        {
                            db.Open();

                            var model = new PortfolioModel();
                            model.blog = blog;
                            model.github = github;
                            model.rocketpunch = rocketpunch;
                            model.description = description;
                            model.writer = writer;

                            string insertSql = @"
INSERT INTO portfolio_tb(
    github,
    blog,
    rocketpunch,
    description,
    writer
)
VALUES(
    @github,
    @blog,
    @rocketpunch,
    @description,
    @writer
);";
                            if (await jobDBManager.InsertAsync(db, insertSql, model) == 1)
                            {
                                await jobDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("job_tb", "job_idx"));
                                Console.WriteLine("포트폴리오 작성 : " + ResponseStatus.OK);
                                return new Response { message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                            else
                            {
                                Console.WriteLine("포트폴리오 작성 : " + ResponseStatus.BAD_REQUEST);
                                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("포트폴리오 작성 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                        Console.WriteLine("WRITE PORTFOLIO ERROR : " + e.Message);
                        return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                    }
                }
                else
                {
                    Console.WriteLine("포트폴리오 작성 : " + ResponseStatus.BAD_REQUEST);
                    return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
                }
            }
            else // Header에 토큰이 전송되지 않음 or 토큰이 유요하지 않음. => 검증 오류.
            {
                Console.WriteLine("포트폴리오 작성 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }
        #endregion
    }
}
