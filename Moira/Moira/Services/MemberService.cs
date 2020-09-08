using Moira.Common;
using Moira.DatabBase;
using Moira.Interface;
using Moira.JWT;
using Moira.JWT.Models;
using Moira.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Moira.Services
{
    public partial class MoiraService : IService
    {
        public DBManager<MemberModel> memberDBManager = new DBManager<MemberModel>();

        #region Member_Service
        public async Task<Response> SignUp(string id, string pw, string grade, string contact, string name, string email)
        {
            if (id != null && pw != null && name != null && grade != null && contact != null && email != null &&
                    id.Trim().Length > 0 && pw.Trim().Length > 0 && name.Trim().Length > 0 && grade.Trim().Length > 0 &&
                    contact.Trim().Length > 0 && email.Trim().Length > 0)
            {
                try
                {
                    using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                    {
                        db.Open();

                        var model = new Member();
                        model.id = id;
                        model.pw = pw;
                        model.grade = grade;
                        model.contact = contact;
                        model.name = name;
                        model.email = email;

                        string insertSql = @"
INSERT INTO member_tb(
    id,
    pw,
    grade,
    contact,
    name,
    email
) 
VALUES(
    @id,
    @pw,
    @grade,
    @contact,
    @name,
    @email
);";

                        await memberDBManager.InsertAsync(db, insertSql, model);
                        await memberDBManager.IndexSortSqlAsync(db, ComDef.GetIndexSortSQL("member_tb", "member_idx"));
                        Console.WriteLine("회원 가입 : " + ResponseStatus.OK);
                        return new Response { message = "성공적으로 회원가입이 신청되었습니다.", status = ResponseStatus.OK };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("회원 가입 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                    Console.WriteLine("SIGNUP ERROR : " + e.Message);
                    return new Response { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                }
            }
            else // 검증 오류.
            {
                Console.WriteLine("회원 가입 : " + ResponseStatus.BAD_REQUEST);
                return new Response { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }

        public async Task<Response<MemberModel>> Login(string id, string pw)
        {
            if (id != null && pw != null && id.Trim().Length > 0 && pw.Trim().Length > 0)
            {
                try
                {
                    MemberModel user = new MemberModel();

                    using (IDbConnection db = new MySqlConnection(ComDef.DATA_BASE_URL))
                    {
                        db.Open();

                        string selectSql = $@"
SELECT
    name, 
    email,
    grade,
    contact
FROM 
    member_tb
WHERE
    id = '{id}'
AND
    pw = '{pw}'
;";
                        var response = await memberDBManager.GetSingleDataAsync(db, selectSql, id);

                        if (response != null) // 회원정보 조회 시, 값이 제대로 들어왔는지 확인.
                        {
                            user.id = id;
                            user.contact = response.contact;
                            user.grade = response.grade;
                            user.name = response.name;
                            user.email = response.email;

                            IAuthContainerModel model = JWTService.GetJWTContainerModel(user.name, user.email);
                            IAuthService authService = new JWTService(model.SecretKey);

                            string token = authService.GenerateToken(model);
                            user.token = token;

                            if (!authService.IsTokenValid(token))
                            {
                                throw new UnauthorizedAccessException();
                            }
                            else
                            {
                                List<Claim> claims = authService.GetTokenClaims(token).ToList();
                                Console.WriteLine("Login UserName : " + claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name)).Value);
                                Console.WriteLine("Login Eamil : " + claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Email)).Value);

                                Console.WriteLine("로그인 : " + ResponseStatus.OK);
                                return new Response<MemberModel> { data = user, message = ResponseMessage.OK, status = ResponseStatus.OK };
                            }
                        }
                        else
                        {
                            Console.WriteLine("로그인 : " + ResponseStatus.UNAUTHORIZED);
                            return new Response<MemberModel> { message = ResponseMessage.UNAUTHORIZED, status = ResponseStatus.UNAUTHORIZED };
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("로그인 : " + ResponseStatus.INTERNAL_SERVER_ERROR);
                    Console.WriteLine("LOGIN ERROR : " + e.Message);
                    return new Response<MemberModel> { message = ResponseMessage.INTERNAL_SERVER_ERROR, status = ResponseStatus.INTERNAL_SERVER_ERROR };
                }
            }
            else // 검증 오류.
            {
                Console.WriteLine("로그인 : " + ResponseStatus.BAD_REQUEST);
                return new Response<MemberModel> { message = ResponseMessage.BAD_REQUEST, status = ResponseStatus.BAD_REQUEST };
            }
        }
        #endregion
    }
}
