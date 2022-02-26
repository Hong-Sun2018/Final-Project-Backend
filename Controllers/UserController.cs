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
        private readonly CookieOptions _cookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        };

        private AppDbContext _database = new AppDbContext();
        
        [Route("signup")]
        [HttpPost]
        public async Task<ActionResult<UserInfo>> SignUp([FromBody] User user)
        {
            if (user.UserName.Length == 0 || user.Password.Length == 0)
            {
                return BadRequest();
            }
     
            try
            {
                if ( _database.Users.Where(userFromDb => userFromDb.UserName == user.UserName).ToArray().Length != 0)
                {
                    return Conflict();
                }
                else
                {   
                    user.Password = Util.CalculateMd5(user.Password);
                    await _database.Users.AddAsync(user);
                    await _database.SaveChangesAsync();
                    return StatusCode(201);
                }
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

            try
            {
                user.Password = Util.CalculateMd5(user.Password);
                User? userFromDb = _database.Users.Where(u => u.UserName == user.UserName).First();

                if (userFromDb == null || userFromDb.Password != user.Password)
                    return Unauthorized();
                else
                {
                    UserInfo userInfo = new UserInfo(userFromDb.UserID, userFromDb.UserName);
                 
                    userFromDb.Token = Util.GenerateUserToken(userFromDb);
                    userFromDb.TimeLogin = Util.CurrentTimestamp();
                    userFromDb.NumWrongPwd = 0;
                    _database.Users.Update(userFromDb);
                    await _database.SaveChangesAsync();
                   
                    HttpContext.Response.Cookies.Append("userToken", userFromDb.Token, _cookieOptions);
                    return userInfo;
                }
            }
            catch (Exception)
            {

                return StatusCode(500);
            }
        }

        [Route("signin-admin")]
        [HttpPost]
        public async Task<ActionResult<UserInfo>> SignInAdmin([FromBody] User user)
        {
            if (user.UserName.Length == 0 || user.Password.Length == 0)
            {
                return Unauthorized();
            }

            
            try
            {
                user.Password = Util.CalculateMd5(user.Password);
                User userFromDB = _database.Users.Where(u => u.UserName == user.UserName).First();
               
                if (userFromDB == null ||
                    userFromDB.Password != user.Password ||
                    !userFromDB.IsAdmin)
                {
                    return Unauthorized();
                }

                userFromDB.Token = Util.GenerateUserToken(userFromDB);
                userFromDB.TimeLogin = Util.CurrentTimestamp();
                userFromDB.NumWrongPwd = 0;

                _database.Users.Update(userFromDB);
                await _database.SaveChangesAsync();

                UserInfo userInfo = new UserInfo(
                    userFromDB.UserID,
                    userFromDB.UserName
                );
                userInfo.isAdmin = userFromDB.IsAdmin;

                CookieOptions cookieOptions = new CookieOptions()
                {
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    IsEssential = true,
                    
                };
                HttpContext.Response.Cookies.Append("userToken", userFromDB.Token, cookieOptions);

                return userInfo;
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
