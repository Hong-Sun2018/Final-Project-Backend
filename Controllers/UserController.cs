using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace Final_Project_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [Route("signup")]
        [HttpPost]
        public async Task<ActionResult<UserInfo>> SignUp([FromBody] User user)
        {
            if (user.UserName.Length == 0 || user.Password.Length == 0)
            {
                return BadRequest();
            }
            UserDB userDB = new();
            try
            {
              
                if (await userDB.CreateUser(user))
                {
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.None
                    };
                    HttpContext.Response.Cookies.Append("userToken", user.Token, cookieOptions);
                    return new UserInfo(user.UserID, user.UserName);
                }
                else
                    return Conflict();
            } catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Route("signin")]
        [HttpPost]
        public async Task<ActionResult<UserInfo>> SignIn([FromBody] User user)
        {
            if (user.UserName.Length == 0 || user.Password.Length == 0) 
                return Unauthorized();

            UserDB userDB = new();

            try
            {
                User? userFromDB = userDB.GetUserByUsername(user.UserName);

                if (userFromDB == null || userFromDB.Password != user.Password)
                    return Unauthorized();
                else
                {
                    UserInfo userInfo = new UserInfo(userFromDB.UserID, userFromDB.UserName);
                 
                    userFromDB.Token = Util.GenerateUserToken(userFromDB);
                    userFromDB.TimeLogin = Util.CurrentTimestamp();
                    userFromDB.NumWrongPwd = 0;
                    userDB.UpdateUser(userFromDB);

                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.None,
                    };
                    HttpContext.Response.Cookies.Append("userToken", userFromDB.Token, cookieOptions);
                    return userInfo;
                }
            }
            catch (Exception)
            {

                return StatusCode(500);
            }
        }
    }

    


}
