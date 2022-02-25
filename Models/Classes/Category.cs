using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ParentID { get; set; }

        public Category(string categoryName, int parentID = -1)
        {
            this.CategoryName = categoryName;
            this.ParentID = parentID;   
        }
    }
}
