using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly CookieOptions _cookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        };

        private AppDbContext _database = new AppDbContext();

        private string? CheckAdminToken(string token)
        {
            // get user by token
            User? user = _database.Users.Where(user => user.Token == token).First();

            // if cannot find user
            if (user == null)
            {
                return null;
            }
            else if (!user.IsAdmin)
            {
                return null;
            }
            else if (Util.CurrentTimestamp() - user.TimeLogin > 3 * 24 * 3600)
            {
                return null;
            }
            else
            {
                user.Token = Util.GenerateUserToken(user);
                user.TimeLogin = Util.CurrentTimestamp();
                _database.Users.Update(user);
                _database.SaveChangesAsync();
                return user.Token;
            }
        }

        private string? CheckUserToken(string token)
        {
            // get user by token
            User? user = _database.Users.Where(user => user.Token == token).First();

            if (user == null)
            {
                return null;
            }
            else if (Util.CurrentTimestamp() - user.TimeLogin > 3 * 24 * 3600)
            {
                return null;
            }
            else
            {
                user.Token = Util.GenerateUserToken(user);
                user.TimeLogin = Util.CurrentTimestamp();
                _database.Users.Update(user);
                _database.SaveChangesAsync();
                return user.Token;
            }
        }

       
        [HttpPost]
        [Route("create-category")]
        public async Task<ActionResult> CreateCategory([FromBody] Category category)
        {
            // Check is admin

            // if no token return unauthorized
            string? userToken = HttpContext.Request.Cookies["userToken"];
            if (userToken == null)
            {
                return Unauthorized();
            }
            
            try
            {
                string? newToken = CheckAdminToken(userToken);
                if (newToken != null)
                {
                    HttpContext.Response.Cookies.Delete("userToken", _cookieOptions);
                    HttpContext.Response.Cookies.Append("userToken", newToken, _cookieOptions);
                    if (_database.Categories.Where(cate => cate.CategoryName == category.CategoryName).ToArray().Length != 0)
                    {
                        return Conflict();
                    }
                    else
                    {
                        await _database.Categories.AddAsync(category);
                        await _database.SaveChangesAsync();
                        return StatusCode(201);
                    }
                } else
                {
                    return Unauthorized();
                }
            }catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
