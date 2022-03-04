﻿using Final_Project_Backend.Models;
using Final_Project_Backend.Models.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<Product>> CreateProduct([FromForm] Product product)
        {
            string? token = HttpContext.Request.Cookies["userToken"];
            if (token == null)
            {
                HttpContext.Response.Cookies.Append("userToken", "", _cookieOptions);
                return Unauthorized();
            }
            try
            {
                AppDbContext database = new AppDbContext();
                User? user = this.CheckAdminToken(token);
                if (user == null)
                {
                    HttpContext.Response.Cookies.Append("userToken", "", _cookieOptions);
                    return Unauthorized();
                }

                if (product.FormFile1 != null)
                {
                    byte[]? bytes = null;
                    product.FileType1 = product.FormFile1.ContentType;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        product.FormFile1.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    product.File1 = bytes;
                }

                if (product.FormFile2 != null)
                {
                    byte[]? bytes = null;
                    product.FileType2 = product.FormFile2.ContentType;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        product.FormFile2.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    product.File2 = bytes;
                }

                if (product.FormFile3 != null)
                {
                    byte[]? bytes = null;
                    product.FileType3 = product.FormFile3.ContentType;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        product.FormFile3.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    product.File3 = bytes;
                }

                database.Users.Update(user);
                await database.Products.AddAsync(product);
                await database.SaveChangesAsync();
                return StatusCode(201);
            } catch (Exception)
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
                    List<Product> temp = database.Products.Where(p => p.ProductName.Contains(keyWord)).ToList();
                    products = products.Union(temp).ToList();
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
    }
}
