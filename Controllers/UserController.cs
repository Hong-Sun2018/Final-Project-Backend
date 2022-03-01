using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Mvc;


namespace Final_Project_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        CookieOptions _cookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            IsEssential = true,
        };
        
        [Route("signup")]
        [HttpPost]
        public async Task<ActionResult<UserInfo>> SignUp([FromBody] User user)
        {
            user.UserName = user.UserName.Trim();
            if (user.UserName.Length == 0 || user.Password.Length == 0)
            {
                return BadRequest();
            }
     
            try
            {
                AppDbContext database = new AppDbContext();
                if ( database.Users.Where(userFromDb => userFromDb.UserName == user.UserName).ToArray().Length != 0)
                {
                    return Conflict();
                }
                else
                {   
                    user.Password = Util.CalculateMd5(user.Password);
                    await database.Users.AddAsync(user);
                    await database.SaveChangesAsync();
                    return StatusCode(201);
                }
            } catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Route("token-login")]
        [HttpGet]
        public async Task<ActionResult<UserInfo>> TokenLogin()
        {
            string? userToken = HttpContext.Request.Cookies["userToken"];
            if (userToken == null)
            {
                return Unauthorized();
            }
            
            try
            {
                AppDbContext database = new AppDbContext();
                User? user = CheckUserToken(userToken);
                if (user == null)
                {
                    HttpContext.Response.Cookies.Append("userToken", "", _cookieOptions);
                    return Unauthorized();
                } else
                {
                    UserInfo userInfo = new UserInfo(user.UserID, user.UserName);
                    userInfo.isAdmin = user.IsAdmin;
                    database.Update(user);
                    await database.SaveChangesAsync();
                    return userInfo;
                }
            } catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Route("token-login-admin")]
        [HttpGet]
        public async Task<ActionResult<UserInfo>> TokenLoginAdmin()
        {
            string? userToken = HttpContext.Request.Cookies["userToken"];
            if (userToken == null)
            {
                return Unauthorized();
            }

            try
            {
                AppDbContext database = new AppDbContext();
                User? user = CheckAdminToken(userToken);
                if (user == null)
                {
                    HttpContext.Response.Cookies.Append("userToken", "", _cookieOptions);
                    return Unauthorized();
                }
                else
                {
                    UserInfo userInfo = new UserInfo(user.UserID, user.UserName);
                    userInfo.isAdmin = user.IsAdmin;
                    database.Update(user);
                    await database.SaveChangesAsync();
                    return userInfo;
                }
            }
            catch (Exception)
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
                AppDbContext database = new AppDbContext();
                user.Password = Util.CalculateMd5(user.Password);
                User? userFromDb = database.Users.Where(u => u.UserName == user.UserName).First();

                if (userFromDb == null || userFromDb.Password != user.Password)
                    return Unauthorized();
                else
                {
                    UserInfo userInfo = new UserInfo(userFromDb.UserID, userFromDb.UserName);
                 
                    userFromDb.Token = Util.GenerateUserToken(userFromDb);
                    userFromDb.TimeLogin = Util.CurrentTimestamp();
                    userFromDb.NumWrongPwd = 0;
                    database.Users.Update(userFromDb);
                    await database.SaveChangesAsync();
                   
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
                AppDbContext database = new AppDbContext();
                user.Password = Util.CalculateMd5(user.Password);
                User[] usersFromDB = database.Users.Where(u => u.UserName == user.UserName).ToArray();
                if (usersFromDB.Length == 0)
                {
                    return Unauthorized();
                }
                User userFromDB = usersFromDB[0];
                if (userFromDB.Password != user.Password || !userFromDB.IsAdmin)
                {
                    return Unauthorized();
                }
                   
                userFromDB.Token = Util.GenerateUserToken(userFromDB);
                userFromDB.TimeLogin = Util.CurrentTimestamp();
                userFromDB.NumWrongPwd = 0;

                database.Users.Update(userFromDB);
                await database.SaveChangesAsync();

                UserInfo userInfo = new UserInfo(
                    userFromDB.UserID,
                    userFromDB.UserName
                );
                userInfo.isAdmin = userFromDB.IsAdmin;

                // _cookieOptions.Domain = HttpContext.Response.Headers.Host;
                HttpContext.Response.Cookies.Append("userToken", userFromDB.Token, _cookieOptions);

                return userInfo;
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        private User? CheckUserToken(string token)
        {
            AppDbContext database = new AppDbContext();
            // get user by token
            User[] users = database.Users.Where(user => user.Token == token).ToArray();
            long currentTime = Util.CurrentTimestamp();

            // if cannot find user
            if (users.Length != 1)
            {
                return null;
            }
            else if (currentTime - users[0].TimeLogin > 3 * 24 * 3600)
            {
                return null;
            }
            else if (currentTime - users[0].TimeToken > 1 * 24 * 3600)
            {
                users[0].TimeToken = currentTime;
                users[0].TimeLogin = currentTime;
                users[0].Token = Util.GenerateUserToken(users[0]);
                return users[0];
            }
            else
            {
                users[0].TimeLogin = currentTime;
                return users[0];
            }
        }

        private User? CheckAdminToken(string token)
        {
            AppDbContext database = new AppDbContext();
            // get user by token
            User[] users = database.Users.Where(user => user.Token == token).ToArray();
            long currentTime = Util.CurrentTimestamp();

            // if cannot find user
            if (users.Length != 1)
            {
                return null;
            }
            else if (!users[0].IsAdmin)
            {
                return null;
            }
            else if (currentTime - users[0].TimeLogin > 3 * 24 * 3600)
            {
                return null;
            }
            else if (currentTime - users[0].TimeToken > 1 * 24 * 3600)
            {
                users[0].TimeToken = currentTime;
                users[0].TimeLogin = currentTime;
                users[0].Token = Util.GenerateUserToken(users[0]);
                return users[0];
            }
            else
            {
                users[0].TimeLogin = currentTime;
                return users[0];
            }
        }
    }
}
