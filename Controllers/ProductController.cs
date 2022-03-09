using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;

namespace Final_Project_Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly CookieOptions _cookieOptions = new CookieOptions()
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            IsEssential = true
        };

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

        private  byte[] ResizeImageAndToBytesArray(IFormFile file)
        {
            Image image = Image.FromStream(file.OpenReadStream());
            double height = (double)image.Height;
            double width = (double)image.Width;
            double ratio = height / width;
            using var memStream = new MemoryStream();   
            if (width > 225)
            {
                int newWidth = 225;
                int newHeight = (int)(newWidth * ratio);
                Image resizedImage = new Bitmap(image, new Size(newWidth, newHeight));
                resizedImage.Save(memStream, image.RawFormat);
                return memStream.ToArray();
            }
            else
            {
                file.CopyTo(memStream);
                return memStream.ToArray();
            }
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromForm] Product product)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            
            try
            {
                AppDbContext database = new AppDbContext();
                User? user = this.CheckAdminToken(token);
                if (user == null)
                {
                    return Unauthorized();
                }

                if (product.FormFile1 != null)
                {
                    product.File1 = this.ResizeImageAndToBytesArray(product.FormFile1);
                    product.FileType1 = product.FormFile1.ContentType;
                }

                if (product.FormFile2 != null)
                {
                    product.File2 = this.ResizeImageAndToBytesArray(product.FormFile2);
                    product.FileType2 = product.FormFile2.ContentType;
                }

                if (product.FormFile3 != null)
                {
                    product.File3 = this.ResizeImageAndToBytesArray(product.FormFile3);
                    product.FileType3 = product.FormFile3.ContentType;
                }

                database.Users.Update(user);
                await database.Products.AddAsync(product);
                await database.SaveChangesAsync();
                return StatusCode(201);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
}

        [HttpGet("{CategoryID}/{KeyWords}")]
        public ActionResult<List<Product>> GetProducts(int CategoryID, string KeyWords)
        {
            string[] keyWordList = KeyWords.Split('_');
            List<int> categories = new List<int>();
            AppDbContext database = new AppDbContext();
            this.findChildrenCategory(CategoryID, categories, database);
            List<Product> products = new List<Product>();
            if (KeyWords.Trim().Equals("UndefinedKeyWord"))
            {
                products = database.Products.ToList();
            }
            else
            {
                foreach (string keyWord in keyWordList)
                {
                    if (keyWord.Length > 0)
                    {
                        List<Product> temp = database.Products.Where(p => p.ProductName.ToLower().Contains(keyWord.ToLower())).ToList();
                        products = products.Union(temp).ToList();
                    }
                }
            }

            if (CategoryID == 0)
            {
                return Ok(products);
            }
            else
            {
                List<Product> productsWithCategory = new List<Product>();
                foreach (int categoryID in categories)
                {
                    productsWithCategory = productsWithCategory.Union(products.Where(p => p.CategoryID == categoryID)).ToList();
                }
                return Ok(productsWithCategory);
            }
        }

        private void findChildrenCategory(int categoryID, List<int> categories, AppDbContext database)
        {
            categories.Add(categoryID);
            Category[] children = database.Categories.Where(c => c.ParentID == categoryID).ToArray();
            if (children.Length > 0)
            {
                foreach (Category child in children)
                {
                    findChildrenCategory(child.CategoryID, categories, database);
                }
            }
        }

        [HttpGet("{productID}")]
        public ActionResult<Product> GetProductById(int productID) 
        {
            AppDbContext database = new AppDbContext();
            List<Product> prodList = database.Products.Where(p => p.ProductID == productID).ToList();
            if (prodList.Count == 1) 
                return Ok(prodList.First());
            else 
                return NotFound();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateProductById([FromForm] Product product)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            User? user = this.CheckAdminToken(token);
            if (user == null)
            {
                return Unauthorized();
            }

            AppDbContext database = new AppDbContext();
            database.Users.Update(user);
            Product[] productsFromDB = database.Products.Where(p => p.ProductID == product.ProductID).ToArray();
            if (productsFromDB.Length != 1)
            {
                return NotFound();
            }

            if (product.FormFile1 != null)
            {
                productsFromDB[0].FileType1 = product.FormFile1.ContentType;
                productsFromDB[0].File1 = this.ResizeImageAndToBytesArray(product.FormFile1);
            }
            if (product.FormFile2 != null)
            {
                productsFromDB[0].FileType2 = product.FormFile2.ContentType;
                productsFromDB[0].File2 = this.ResizeImageAndToBytesArray(product.FormFile2);
            }
            if (product.FormFile3 != null)
            {
                productsFromDB[0].FileType3 = product.FormFile3.ContentType;
                productsFromDB[0].File3 = this.ResizeImageAndToBytesArray(product.FormFile3);
            }

            productsFromDB[0].ProductDesc = product.ProductDesc;
            productsFromDB[0].ProductName = product.ProductName;
            productsFromDB[0].ProductPrice = product.ProductPrice;
            productsFromDB[0].ProductStock = product.ProductStock;
            productsFromDB[0].CategoryID = product.CategoryID;
            database.Products.Update(productsFromDB[0]);
            await database.SaveChangesAsync();
            return StatusCode(204);
        }

        [HttpDelete("{productID}")]
        public async Task<ActionResult> DeleteProduct(int productID)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            AppDbContext database = new AppDbContext();
            User? user = this.CheckAdminToken(token);

            if (user == null)
            {
                return Unauthorized();
            }
            database.Users.Update(user);

            Product? prod = database.Products.Find(productID);
            if (prod == null)
            {
                await database.SaveChangesAsync();
                return NotFound();
            }
            else
            {
                database.Products.Remove(prod);
                await database.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
