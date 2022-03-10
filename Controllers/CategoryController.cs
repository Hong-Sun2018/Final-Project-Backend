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
            SameSite = SameSiteMode.None,
            IsEssential = true
        };

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

        [HttpPost]
        [Route("create-category")]
        public async Task<ActionResult> CreateCategory([FromBody] Category category)
        {
            // if no token return unauthorized
            string? userToken = HttpContext.Request.Cookies["userToken"];
            if (userToken == null)
            {

                return Unauthorized();
            }

            try
            {
                User? user = CheckAdminToken(userToken);

                if (user != null)
                {
                    AppDbContext database = new AppDbContext();
                    category.CategoryName = category.CategoryName.Trim();
                    database.Debugs.Add(new Debug(category.CategoryName));
                    database.Debugs.Add(new Debug(category.CategoryID.ToString()));
                    database.SaveChanges();
                    if (category.CategoryName.Equals("") || category.ParentID == 0)
                    {
                        return BadRequest();
                    }
                    
                    database.Users.Update(user);
                    HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
                    Category[] categoriesFromDB = database.Categories.Where(c => c.CategoryName.Trim().Equals(category.CategoryName)).ToArray();
                    if (categoriesFromDB.Length != 0)
                    {
                        return Conflict();
                    }
                    
                    await database.Categories.AddAsync(category);
                    await database.SaveChangesAsync();
                    HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
                    return StatusCode(201);                    
                }
                else
                {
                    return Unauthorized();
                }
            } catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("{parentID}")]
        // [Route("get-cate-list")]
        public ActionResult<List<Category>> GetCategoryList(int parentID)
        { 
  
                AppDbContext database = new AppDbContext();
                List<Category> categoryList = database.Categories.Where(c => c.ParentID == parentID).ToList();
                return categoryList;

        }

        [HttpGet]
        [Route("get-cate-path")]
        public ActionResult<List<Category>> GetCategoryPath(int categoryID)
        {
            List<Category> catePath = new List<Category>();
            AppDbContext database = new AppDbContext();
            this.FindParent(categoryID, database, catePath);
            return catePath;
        }

        private void FindParent(int categoryID, AppDbContext database, List<Category> catePath)
        {
            Category currentCate = database.Categories.Where(c => c.CategoryID == categoryID).First();
            if (currentCate.ParentID != -1)
            {
                this.FindParent(currentCate.ParentID, database, catePath);
            }
            catePath.Add(currentCate);
        }
    }
}