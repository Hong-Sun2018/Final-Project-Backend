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
            foreach (OrderProduct orderProduct in orderProducts)
            {
                Product? product = this.database.Products.Find(orderProduct.ProductId);
                if (product == null || product.ProductStock < orderProduct.ProductQuantity)
                {
                    return BadRequest();
                }
                else
                {
                    product.ProductStock = product.ProductStock - orderProduct.ProductQuantity;
                    this.database.Products.Update(product);
                    await this.database.OrderProduct.AddAsync(orderProduct);
                }
            }
            this.database.Users.Update(user);
            CartProduct[] cartProducts = this.database.CartProduct.Where(cp => cp.UserID == user.UserID).ToArray();
            this.database.CartProduct.RemoveRange(cartProducts);
            await this.database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> GetOrderList()
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            Order[] orders = this.database.Orders.Where(o => o.UserID == user.UserID).ToArray();

            foreach (Order order in orders)
            {
                List<OrderProduct> orderProducts = this.database.OrderProduct.Where(op => op.OrderId == order.OrderID).ToList();
                double totalAmount = 0;
                foreach (OrderProduct orderProduct in orderProducts)
                {
                    totalAmount += orderProduct.ProductQuantity * orderProduct.ProductPris;
                }
                order.TotalAmount = totalAmount;
            }
            this.database.Users.Update(user);
            await this.database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            return Ok(orders);
        }

        [HttpGet("{orderID}")]
        public async Task<ActionResult> GetOrderDetail(int orderID)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckUserToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            Order? order = this.database.Orders.Find(orderID);
            if (order == null || order.UserID != user.UserID)
            {
                return NotFound();
            }

            OrderProduct[] orderProducts = this.database.OrderProduct.Where(op => op.OrderId == orderID).ToArray();
            if (orderProducts.Length == 0)
            {
                return NotFound();
            }

            var orderProdWithProdName = orderProducts
                .Join(this.database.Products,
                    orderProd => orderProd.ProductId,
                    prod => prod.ProductID,
                    (orderProd, prod) => new { orderProd.Id, orderProd.ProductQuantity, orderProd.ProductPris, prod.ProductName, prod.File1, prod.FileType1 }
                ).ToArray();

            this.database.Users.Update(user);
            await this.database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            return Ok(orderProdWithProdName);
        }

        [HttpGet]
        [Route("get-total-number")]
        public async Task<ActionResult> GetTotalNumber()
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckAdminToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            this.database.Users.Update(user);
            int number = this.database.Orders.Count();
            await this.database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            return Ok(number);
        }

        [HttpGet]
        [Route("get-orders-all-users")]
        public async Task<ActionResult> GetOrdersAllUsers()
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckAdminToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            this.database.Update(user);
            await database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            Order[] orders = this.database.Orders.ToArray();
            foreach (Order order in orders)
            {
                OrderProduct[] orderProducts = this.database.OrderProduct.Where(op => op.OrderId == order.OrderID).ToArray();
                double total = 0;
                foreach (OrderProduct orderProd in orderProducts)
                {
                    total += orderProd.ProductQuantity * orderProd.ProductPris;
                }
                order.TotalAmount = total;
            }
            var ordersWithUserName = orders
                .Join(
                    this.database.Users,
                    order => order.UserID,
                    user => user.UserID,
                    (order, user) => new { user.UserName, order.OrderID, order.OrderTime, order.TotalAmount }
                );
            return Ok(ordersWithUserName);
        }

        [HttpGet]
        [Route("get-order")]
        public async Task<ActionResult> GetOrder(int id)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckAdminToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            Order? order = this.database.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            User userFromOrder = null;
            if (order.UserID != user.UserID)
            {
                userFromOrder = this.database.Users.Find(order.UserID);
                if (userFromOrder == null)
                {
                    return NotFound();
                }
            } 
            else
            {
                userFromOrder = user;
            }

            OrderProduct[] orderProducts = this.database.OrderProduct.Where(op => op.OrderId == id).ToArray();
            if (orderProducts.Length == 0)
            {
                return NotFound();
            }

            var orderProdWithProdName = orderProducts
                .Join(this.database.Products,
                    orderProd => orderProd.ProductId,
                    prod => prod.ProductID,
                    (orderProd, prod) => new { orderProd.Id, orderProd.ProductQuantity, orderProd.ProductPris, prod.ProductName, prod.File1, prod.FileType1 }
                ).ToArray();

            this.database.Users.Update(user);
            await this.database.SaveChangesAsync();
            HttpContext.Response.Cookies.Append("userToken", user.Token, _cookieOptions);
            return Ok(new { order.OrderID, order.OrderTime, userFromOrder.UserID, userFromOrder.UserName, orderProdWithProdName });

        }

    }    
}
