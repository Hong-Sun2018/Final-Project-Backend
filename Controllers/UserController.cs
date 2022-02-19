using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace Final_Project_Backend.Controllers
{
    [Route("user/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            if (user.UserName.Length == 0 || user.Password.Length == 0)
            {
                return BadRequest();
            }
            UserDB userDB = new();
            if (await userDB.CreateUser(user)) {
                return StatusCode(201);
            }
            else 
                return Conflict();
        }

        [Route("signin/{username}/{password}")]
        [HttpGet]
        public async Task<ActionResult<User>> GetUser(string username, string password)
        {
            return new User("longbow","123123") ;
        }
    }

    


}
