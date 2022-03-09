using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Final_Project_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext database = new AppDbContext();

        private User? CheckUserToken(string? token)
        {
            if (token == null || token.Length < 32)
            {
                return null;
            }

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

        private User? CheckAdminToken(string? token)
        {
            if (token == null || token.Length < 32)
            {
                return null;
            }

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
        public async Task<ActionResult> CreateOrder()
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            Order order = new Order()
            {
                UserID = user.UserID,
                OrderTime = Util.CurrentTimestamp(),
                Status = OrderStatus.New
            };

            this.database.Add(order);
            this.database.SaveChanges();

            int orderID = order.OrderID;
            List<OrderProduct> orderProducts = this.database.CartProduct.Where(cp => cp.UserID == user.UserID)
                .Join(this.database.Products,
                    cartProduct => cartProduct.ProductID,
                    product => product.ProductID,
                    (cartProduct, product) => new OrderProduct() { OrderId = orderID, ProductId = product.ProductID, ProductPris = product.ProductPrice, ProductQuantity = cartProduct.ProductAmount }
                )
                .ToList();
            if (orderProducts.Count == 0)
            {
                this.database.Users.Update(user);
                await this.database.SaveChangesAsync();
                return NotFound();
            }
            this.database.Users.Update(user);
            await this.database.OrderProduct.AddRangeAsync(orderProducts);
            CartProduct[] cartProducts = this.database.CartProduct.Where(cp => cp.UserID == user.UserID).ToArray();
            this.database.CartProduct.RemoveRange(cartProducts);
            await this.database.SaveChangesAsync();
            return Ok();
        }
    }    
}
