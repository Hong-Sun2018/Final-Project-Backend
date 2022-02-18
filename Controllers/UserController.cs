using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Final_Project_Backend.Controllers
{
    [Route("user/User")]
    [ApiController]

    public class UserController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            UserDB userDB = new();
            if (await userDB.CreateUser(user))
            {
                return CreatedAtAction("Sign Up", null, null);
            }
            else
                return Conflict();
        }

        [HttpGet("{token}")]
        public async Task<ActionResult<User>> GetUser()
        {
            return null;
        }
    }


}
