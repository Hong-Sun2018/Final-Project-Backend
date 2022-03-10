using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext database = new AppDbContext();

        CookieOptions _cookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            IsEssential = true,
        };
        private User? CheckUserToken(string? token)
        {
            if (token == null || token.Length < 32)
            {
                return null;
            }

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

        [HttpPost]
        public async Task<ActionResult> CreateCartProduct([FromBody] CartProduct cartProduct)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            cartProduct.UserID = user.UserID;
            Product? prod = await this.database.Products.FindAsync(cartProduct.ProductID);
            if (prod == null)
            {
                return BadRequest();
            }
            else if (prod.ProductStock == 0)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                CartProduct[] cartProdFromDB = this.database.CartProduct
                    .Where(cp => cp.UserID == user.UserID)
                    .Where(cp => cp.ProductID == cartProduct.ProductID)
                    .ToArray();
                if (cartProdFromDB.Length == 0)
                {
                    await this.database.AddAsync(cartProduct);
                } 
                else if ((cartProdFromDB[0].ProductAmount + cartProduct.ProductAmount) < prod.ProductStock)
                {
                    return Conflict();
                }
                
                this.database.Update(user);
                await this.database.SaveChangesAsync();
                HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
                return StatusCode(201);
            } else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCartProduct()
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return NotFound();
            }

            var cartProductListWithProductDetail = this.database.CartProduct
                .Where(cartProd => cartProd.UserID == user.UserID)
                .Join(
                    this.database.Products,
                    cartProd => cartProd.ProductID,
                    prod => prod.ProductID,
                    (cartProd, prod) => new { cartProd.Id, cartProd.UserID, cartProd.ProductID, cartProd.ProductAmount, prod.ProductName, prod.ProductStock, prod.ProductPrice, prod.FileType1, prod.File1 }
                ).ToList();
            this.database.Users.Update(user);
            await this.database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            return Ok(cartProductListWithProductDetail);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCartProduct([FromBody] CartProduct cartProduct)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return Unauthorized();
            }
            else if (cartProduct.UserID != user.UserID)
            {
                return Unauthorized();
            }

            CartProduct? cartProductFromDB = this.database.CartProduct.Find(cartProduct.Id);
            Product? product = this.database.Products.Find(cartProduct.ProductID);
            if (cartProductFromDB == null || product == null)
            {
                return NotFound();
            }
            else if (cartProduct.ProductAmount >= 1 && cartProduct.ProductAmount <= product.ProductStock)
            {
                cartProductFromDB.ProductAmount = cartProduct.ProductAmount;
                this.database.CartProduct.Update(cartProductFromDB);
                this.database.Users.Update(user);
                await this.database.SaveChangesAsync();
                HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
                return Ok();
            }
            else 
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCartProduct(int id)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            CartProduct? cartProd = this.database.CartProduct.Find(id);
            if (cartProd == null)
            {
                return NotFound();
            }
            else if (cartProd.UserID != user.UserID) 
            {
                return Unauthorized();
            }else
            {
                this.database.CartProduct.Remove(cartProd);
                this.database.Users.Update(user);
                await this.database.SaveChangesAsync();
                HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
                return Ok();
            }
        }
    }
}
